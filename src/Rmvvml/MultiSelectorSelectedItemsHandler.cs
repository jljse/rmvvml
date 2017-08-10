using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace Rmvvml
{
    /// <summary>
    /// SelectedItemsの処理を行う
    /// </summary>
    public class MultiSelectorSelectedItemsHandler
    {
        MultiSelector AssociatedObject { get; set; }
        bool IsUpdating { get; set; } = false;

        // VM側のコレクションが丸ごと変更された
        public void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var selector = d as MultiSelector;
            if (selector == null) return;

            if (e.NewValue != null)
            {
                AttachControl(selector);
            }
            else
            {
                DetachControl();
            }

            var oldVal = e.OldValue as INotifyCollectionChanged;
            if (oldVal != null)
            {
                oldVal.CollectionChanged -= CollectionChanged;
            }
            var newVal = e.NewValue as INotifyCollectionChanged;
            if (newVal != null)
            {
                newVal.CollectionChanged += CollectionChanged;
            }
        }

        public void AttachControl(MultiSelector selector)
        {
            if (AssociatedObject == null)
            {
                AssociatedObject = selector;
                AssociatedObject.SelectionChanged += SelectionChanged;
            }
            else
            {
                if (AssociatedObject != selector)
                {
                    throw new InvalidOperationException("event receiver is not correct");
                }
            }
        }

        public void DetachControl()
        {
            if (AssociatedObject != null)
            {
                AssociatedObject.SelectionChanged -= SelectionChanged;
                AssociatedObject = null;
            }
        }

        // VM側からの変更通知
        private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var vmList = MultiSelectorAtt.GetSelectedItems(AssociatedObject);
            if (vmList == null) return;

            if (IsUpdating) return;
            try
            {
                IsUpdating = true;

                System.Diagnostics.Debug.WriteLine("MultiSelectorSelectedItemsHandler.CollectionChanged");

                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        foreach (var item in e.NewItems)
                        {
                            var insertingItem = item;

                            if (insertingItem == null)
                            {
                                // nullはNewItemPlaceHolderとして扱う
                                insertingItem = CollectionView.NewItemPlaceholder;
                            }

                            // 遅そう
                            if (!AssociatedObject.SelectedItems.Contains(insertingItem))
                            {
                                AssociatedObject.SelectedItems.Add(insertingItem);
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        foreach (var item in e.OldItems)
                        {
                            AssociatedObject.SelectedItems.Remove(item);
                        }
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        AssociatedObject.SelectedItems.Clear();
                        break;
                    default:
                        break;
                }
            }
            finally
            {
                IsUpdating = false;
            }
        }

        // V側からの変更通知
        private void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var vmList = MultiSelectorAtt.GetSelectedItems(AssociatedObject);
            if (vmList == null) return;

            if (IsUpdating) return;
            try
            {
                IsUpdating = true;

                System.Diagnostics.Debug.WriteLine("MultiSelectorSelectedItemsHandler.SelectionChanged start");

                foreach (var item in e.RemovedItems)
                {
                    vmList.Remove(item);
                }
                foreach (var item in e.AddedItems)
                {
                    object insertingItem = item;

                    // うーん...
                    if (insertingItem == CollectionView.NewItemPlaceholder)
                    {
                        var ilist = vmList.GetType().GetInterface(typeof(IList<>).FullName);
                        if (ilist == null)
                        {
                            // IList<T> を実装していないので何でも突っ込める
                        }
                        else
                        {
                            // IList<T> を実装しているのでその型しか突っ込めない
                            var elementType = ilist.GenericTypeArguments[0];
                            if (elementType.IsAssignableFrom(item.GetType()))
                            {
                                // 互換性があるっぽい なんで？
                            }
                            else
                            {
                                // 型が合わないのでitemの代わりにnullを突っ込む
                                insertingItem = null;
                            }
                        }
                    }

                    // 遅そう
                    if (!vmList.Contains(insertingItem))
                    {
                        vmList.Add(insertingItem);
                    }
                }

                System.Diagnostics.Debug.WriteLine("MultiSelectorSelectedItemsHandler.SelectionChanged finish");
            }
            finally
            {
                IsUpdating = false;
            }

        }

    }
}
