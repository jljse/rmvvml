using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;
using System.Windows.Threading;

namespace Rmvvml
{
    /// <summary>
    /// データのドロップを可能にします
    /// </summary>
    public class DropBehavior : Behavior<FrameworkElement>
    {
        #region DropCommand

        /// <summary>
        /// ドロップ時に呼び出すコマンド
        /// </summary>
        public ICommand DropCommand
        {
            get { return (ICommand)GetValue(DropCommandProperty); }
            set { SetValue(DropCommandProperty, value); }
        }

        public static readonly DependencyProperty DropCommandProperty =
            DependencyProperty.Register("DropCommand", typeof(ICommand), typeof(DropBehavior), new PropertyMetadata(null));

        #endregion

        #region DataType

        /// <summary>
        /// ドラッグドロップのデータの種類
        /// DragBehaviorとDropBehaviorのペアリングのために使用
        /// </summary>
        public object DataType
        {
            get { return (object)GetValue(DataTypeProperty); }
            set { SetValue(DataTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DataType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataTypeProperty =
            DependencyProperty.Register("DataType", typeof(object), typeof(DropBehavior), new PropertyMetadata(null));

        #endregion

        /// <summary>
        /// 現在表示中のAdorner
        /// 
        /// </summary>
        DropAdorner ShowingAdorner { get; set; }

        /// <summary>
        /// 最後にDragOverイベントが発生した時刻 (最後にドラッグ中のマウスが乗っていた時刻)
        /// </summary>
        DateTime LastDragOver { get; set; }

        /// <summary>
        /// ドラッグ中のマウスが乗っているかをポーリングするためのタイマ
        /// </summary>
        DispatcherTimer Timer { get; set; }

        /// <summary>
        /// 現在のドラッグに対してドロップを受け入れ可能かどうか (優先順位が回ってくれば受け入れたいかどうか)
        /// </summary>
        bool IsAcceptable { get; set; } = false;

        /// <summary>
        /// 現在ドロップ先Adorner
        /// </summary>
        bool IsShowAdorner
        {
            get { return ShowingAdorner != null; }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Drop += OnDrop;
            AssociatedObject.PreviewDragOver += OnPreviewDragOver;
        }

        // MouseEnter/Leaveの挙動とDragEnter/Leaveの挙動が違う...マウスキャプチャしてる関係と思われる
        // - トンネルしていくイベントの回数が違う Dragはいちいち構成要素のコントロールに対して発生する
        // - DragEnter/Leaveは1つのコントロールしかEnter状態にならない？(必ずLeaveしてから次にEnterしている)
        //     MouseEnter/Leaveは複数のコントロールにEnterした状態になる
        // - スクロールの中にコントロールがある場合、DragEnter/Leaveを発生させずにBorderに出入りできてしまう
        //     中に入った時点でBorderからはLeaveしてるから？

        // じゃぁどうすればいいのか
        // DragOverは発火し続けるみたい
        //   未表示ならShowIndicator表示
        //   未起動なら0.1秒タイマを起動
        //   最後のDragOver時刻を更新
        // => Tickで最後のDragOver時刻から0.1秒経過していたら閉じる

        private void OnPreviewDragOver(object sender, DragEventArgs e)
        {
            LastDragOver = DateTime.Now;

            if(Timer == null)
            {
                Timer = new DispatcherTimer();
                Timer.Interval = TimeSpan.FromSeconds(0.05);
                Timer.Tick += Timer_Tick;
                Timer.Start();

                System.Diagnostics.Debug.WriteLine(
                    "DropBehavior.DragOver START ({0}, {1}, {2})",
                    sender.GetType().Name,
                    e.Source.GetType().Name,
                    e.OriginalSource.GetType().Name
                    );

                // ここでDrop対象かどうか判定する
                if(e.Data.GetDataPresent(DataType?.ToString() ?? "test"))
                {
                    IsAcceptable = true;
                }
                else
                {
                    IsAcceptable = false;
                }

                // 自分のindicatorを出したい時、
                //   まず親に表示中のやつがいないかチェックして、いれば隠す
                //   それから自分を表示

                if (IsAcceptable && !IsShowAdorner)
                {
                    var active = VisualTreeHelperExtension
                        .AncestorsOrSelf(AssociatedObject)
                        .Behaviors()
                        .OfType<DropBehavior>()
                        .Where(b => b != this)
                        .Where(b => b.IsShowAdorner)
                        .FirstOrDefault();

                    if (active != null)
                    {
                        active.HideIndicator();
                    }

                    this.ShowIndicator();
                }
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // DragOverが一定期間発生していなければ、マウスが外れたと判定して自分のAdornerを隠す
            if(DateTime.Now > LastDragOver + TimeSpan.FromSeconds(0.1))
            {
                Timer.Stop();
                Timer.Tick -= Timer_Tick;
                Timer = null;

                IsAcceptable = false;

                System.Diagnostics.Debug.WriteLine(
                    "{0} DropBehavior.DragOver FINISHED",
                    DateTime.Now.ToString("HH:mm:ss.fff")
                    );

                // 自分のindicatorを隠す場合、
                //   まず自分を隠す
                //   親にドロップ可能なやつがいないかチェックして、いれば表示

                if(IsShowAdorner)
                {
                    this.HideIndicator();

                    var acceptable = VisualTreeHelperExtension
                        .AncestorsOrSelf(AssociatedObject)
                        .Behaviors()
                        .OfType<DropBehavior>()
                        .Where(b => b != this)
                        .Where(b => b.IsAcceptable)
                        .FirstOrDefault();

                    if (acceptable != null)
                    {
                        acceptable.ShowIndicator();
                    }
                }

            }
        }

        /// <summary>
        /// このドロップ先のための標識を消す
        /// </summary>
        void HideIndicator()
        {
            if(ShowingAdorner != null)
            {
                System.Diagnostics.Debug.WriteLine("{0} HideAdorner {1}",
                    DateTime.Now.ToString("HH:mm:ss.fff"),
                    AssociatedObject.GetType().Name
                    );
                ShowingAdorner.Hide();
                ShowingAdorner = null;
            }
        }

        /// <summary>
        /// このドロップ先のための標識を表示する
        /// </summary>
        void ShowIndicator()
        {
            if(ShowingAdorner == null)
            {
                System.Diagnostics.Debug.WriteLine("{0} ShowAdorner {1}",
                    DateTime.Now.ToString("HH:mm:ss.fff"),
                    AssociatedObject.GetType().Name
                    );
                ShowingAdorner = new DropAdorner(AssociatedObject);
                ShowingAdorner.Show();
            }
        }

        private void OnDrop(object sender, DragEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("DropBehavior.OnDrop");

            // ドロップ風
            if(DropCommand != null)
            {
                var data = e.Data.GetData(DataType?.ToString() ?? "test");
                DropCommand.Execute(data);
                e.Handled = true;
            }
        }
    }

    public class DropAdorner : SimpleAdornerBase
    {
        public DropAdorner(UIElement adornedElement)
            : base(adornedElement)
        {
            Host.Children.Add(new Border
            {
                BorderBrush = Brushes.Red,
                BorderThickness = new Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                // よくわかんない
                IsHitTestVisible = false,
            });
        }
    }


}
