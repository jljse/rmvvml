using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Rmvvml
{
    /// <summary>
    /// ウィンドウをItemsControlのような感じで管理するためのコントロール
    /// 普通のコントロールではない
    /// </summary>
    public class WindowsControl : Control
    {
        #region ItemsSource

        /// <summary>
        /// WindowのもとになるViewModelをバインドする
        /// 無理矢理なことをするのでItemsControlにはできなかった
        /// </summary>
        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(WindowsControl), new PropertyMetadata(null, OnItemsSourceChanged));

        #endregion

        #region Items

        /// <summary>
        /// インスタンス化したウィンドウオブジェクト
        /// </summary>
        public List<Window> Items
        {
            get { return (List<Window>)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }

        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register("Items", typeof(List<Window>), typeof(WindowsControl), new PropertyMetadata(new List<Window>()));

        #endregion

        public WindowsControl()
        {
        }

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((WindowsControl)d).OnItemsSourceChanged(e);
        }

        private void OnItemsSourceChanged(DependencyPropertyChangedEventArgs e)
        {
            var oldCollection = e.OldValue as INotifyCollectionChanged;
            if (oldCollection != null)
            {
                oldCollection.CollectionChanged -= ItemsSource_CollectionChanged;
            }
            var newCollection = e.NewValue as INotifyCollectionChanged;
            if (newCollection != null)
            {
                newCollection.CollectionChanged += ItemsSource_CollectionChanged;
            }

            // 初期値として設定値入りのItemsSourceが設定された場合、それらを開く
            if (e.OldValue == null)
            {
                var initialItems = (e.NewValue as IEnumerable).OfType<object>().ToList();
                if(initialItems.Count > 0)
                {
                    ItemsSource_CollectionChanged(
                        e.NewValue,
                        new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, initialItems, 0));
                }
            }
        }

        private void ItemsSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // なんか怖いので全部UIスレッドにディスパッチする
            Application.Current.Dispatcher.Invoke(() =>
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        foreach (var item in e.NewItems)
                        {
                            // VMの型からDataTemplateを探し、対応するViewを生成する
                            var key = new DataTemplateKey(item.GetType());
                            var template = FindResource(key) as DataTemplate;
                            var content = template.LoadContent();

                            if (content is FrameworkElement)
                            {
                                var fe = content as FrameworkElement;
                                fe.DataContext = item;
                            }

                            if (content is Window)
                            {
                                var win = content as Window;
                                AddWindow(win);
                            }
                            else if (content is IVolatileWindowHandler)
                            {
                                var vol = content as IVolatileWindowHandler;
                                ShowAndDeleteVolatileWindow(vol);
                            }
                            else
                            {
                                // 自動でラップしても正しく動くけど、勝手にウィンドウを作るとわけがわからなくなりそうなのでやめた
                                // win = new Window { Content = content, };
                                throw new InvalidCastException("Content of DataTemplate must be Window");
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        foreach (var item in e.OldItems)
                        {
                            var win = Items.Where(w => w.DataContext == item).FirstOrDefault();
                            if (win != null)
                            {
                                win.Close();
                                if (win.IsLoaded)
                                {
                                    // IsLoadedでCloseがキャンセルされたかどうかわかるからエラーにする...と思ったけど、
                                    // 非同期になっていてうまく判断できなかった
                                    //throw new InvalidOperationException("Dont cancel Closing while removing ViewModel from ItemsSource");
                                }
                                Items.Remove(win);
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Move:
                        var fromIndex = e.OldStartingIndex;
                        var toIndex = e.NewStartingIndex;
                        if (fromIndex < 0 ||
                            fromIndex >= Items.Count ||
                            toIndex < 0 ||
                            toIndex >= Items.Count)
                        {
                            throw new IndexOutOfRangeException("index in CollectionChanged parameter from ItemsSource seems to be wrong");
                        }
                        var movingItem = Items.Where(w => w.DataContext == e.NewItems[0]).First();

                        if (toIndex == Items.Count - 1)
                        {
                            // 末尾への移動だった場合、Activateする なんか無理矢理すぎないか
                            movingItem.Activate();
                        }

                        break;
                    case NotifyCollectionChangedAction.Reset:
                        // 全削除
                        for(int i = Items.Count - 1; i >= 0; --i)
                        {
                            var win = Items[i];
                            win.Close();
                            Items.RemoveAt(i);
                        }
                        break;
                    default:
                        break;
                }
            });

        }

        // 生成したハンドラを使って一時的な画面を表示する
        // 表示後はItemsSourceからVMを削除する
        void ShowAndDeleteVolatileWindow(IVolatileWindowHandler vol)
        {
            if (vol is IShowDialog)
            {
                var sd = vol as IShowDialog;
                vol.Owner = Items.Where(w => w.DataContext == sd.Owner).FirstOrDefault();
            }
            vol.ShowDialog();
            // 処理が終わったらItemsSourceから消しちゃえ
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (ItemsSource is IList)
                {
                    var list = ItemsSource as IList;
                    for (var i = 0; i < list.Count; ++i)
                    {
                        if (list[i] == vol)
                        {
                            list.RemoveAt(i);
                            break;
                        }
                    }
                }
            }));
        }

        // 生成したwindowコントロールを管理下に加える
        void AddWindow(Window win)
        {
            // 画面側から閉じられた場合にItemsSourceを消すための処理
            win.Closed += Window_Closed;

            bool isShowDialog = false;
            if (win.DataContext is IShowDialog)
            {
                var sd = win.DataContext as IShowDialog;

                if (sd.Owner != null)
                {
                    var ownerWindow = Items.Where(w => w.DataContext == sd.Owner).FirstOrDefault();
                    if (ownerWindow == null)
                    {
                        // 親ウィンドウを保持する仕組みを入れることも考えたけど、複雑になりそうなのでやめた
                        throw new ArgumentException("Owner must be ViewModel of a Window");
                    }
                    else
                    {
                        win.Owner = ownerWindow;
                    }
                }

                if (sd.IsShowDialog)
                {
                    isShowDialog = true;
                }
            }

            Items.Add(win);
            if (isShowDialog)
            {
                win.ShowDialog();
            }
            else
            {
                win.Show();
            }
        }

        // 画面側の×ボタンから閉じられた場合ItemsSourceにVMが残ってしまうので取り除く
        private void Window_Closed(object sender, EventArgs e)
        {
            // もしもItemsSourceの中に残っていたら削除する
            var win = sender as Window;
            if (ItemsSource is IList)
            {
                var list = ItemsSource as IList;
                for (int i = 0; i < list.Count; ++i)
                {
                    if (list[i] == win.DataContext)
                    {
                        list.RemoveAt(i);
                        break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// ウィンドウに相当するViewModelが実装することで、ウィンドウの開き方を制御する
    /// </summary>
    public interface IShowDialog
    {
        /// <summary>
        /// trueの場合、モーダルダイアログ
        /// </summary>
        bool IsShowDialog { get; }
        /// <summary>
        /// オーナーウィンドウのViewModel
        /// </summary>
        object Owner { get; }
    }

    /// <summary>
    /// メッセージボックスのようなWindowコントロールでないウィンドウを表示する処理
    /// </summary>
    public interface IVolatileWindowHandler
    {
        /// <summary>
        /// 親ウィンドウ
        /// </summary>
        Window Owner { get; set; }
        /// <summary>
        /// 表示
        /// </summary>
        void ShowDialog();
    }

}
