using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Media;

namespace Rmvvml.Sample
{
    public class TextBoxAtt
    {
        #region FocusAction

        public static AttachedPropertyAction GetFocusAction(DependencyObject obj)
        {
            return (AttachedPropertyAction)obj.GetValue(FocusActionProperty);
        }

        public static void SetFocusAction(DependencyObject obj, AttachedPropertyAction value)
        {
            obj.SetValue(FocusActionProperty, value);
        }

        // Using a DependencyProperty as the backing store for FocusAction.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FocusActionProperty =
            DependencyProperty.RegisterAttached("FocusAction", typeof(AttachedPropertyAction), typeof(TextBoxAtt), new PropertyMetadata(null, OnFocusActionChanged));

        private static void OnFocusActionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AttachedPropertyActionHandler<TextBox, object>.Inject(FocusActionProperty, d, e, (control, param) =>
            {
                // ViewModel側からInvokeされた時に実行したい処理をここに書く
                control.Focus();
            });
        }

        #endregion
    }

}
