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
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

namespace Rmvvml
{

    public class MultiSelectorAtt
    {
        #region SelectedItems

        public static IList GetSelectedItems(DependencyObject obj)
        {
            return (IList)obj.GetValue(SelectedItemsProperty);
        }

        public static void SetSelectedItems(DependencyObject obj, IList value)
        {
            obj.SetValue(SelectedItemsProperty, value);
        }

        /// <summary>
        /// バインド可能なSelectedItems
        /// コントロールのSelectionChanged時にバインドされたリストへの反映を行う
        /// ObservableCollectionがバインドされた場合はVM=>Vの反映も行う
        /// </summary>
        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.RegisterAttached("SelectedItems", typeof(IList), typeof(MultiSelectorAtt), new PropertyMetadata(null, OnSelectedItemsChanged));

        private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dg = d as DataGrid;
            if (dg == null) return;

            var handler = GetSelectedItems_Handler(dg);
            if(handler == null)
            {
                handler = new MultiSelectorSelectedItemsHandler();
                SetSelectedItems_Handler(dg, handler);
            }

            handler.OnSelectedItemsChanged(d, e);
        }

        #region SelectedItems_Handler

        static MultiSelectorSelectedItemsHandler GetSelectedItems_Handler(DependencyObject obj)
        {
            return (MultiSelectorSelectedItemsHandler)obj.GetValue(SelectedItems_HandlerProperty);
        }

        static void SetSelectedItems_Handler(DependencyObject obj, MultiSelectorSelectedItemsHandler value)
        {
            obj.SetValue(SelectedItems_HandlerProperty, value);
        }

        // Using a DependencyProperty as the backing store for SelectedItems_Handler.  This enables animation, styling, binding, etc...
        static readonly DependencyProperty SelectedItems_HandlerProperty =
            DependencyProperty.RegisterAttached("SelectedItems_Handler", typeof(MultiSelectorSelectedItemsHandler), typeof(MultiSelectorAtt), new PropertyMetadata(null));

        #endregion

        #endregion
    }

}

