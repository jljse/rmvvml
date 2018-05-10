using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interactivity;

namespace Rmvvml
{
    public class InjectedViewActionTrigger : TriggerBase<FrameworkElement>
    {
        #region Trigger

        public InjectedViewAction Trigger
        {
            get { return (InjectedViewAction)GetValue(TriggerProperty); }
            set { SetValue(TriggerProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Trigger.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TriggerProperty =
            DependencyProperty.Register("Trigger", typeof(InjectedViewAction), typeof(InjectedViewActionTrigger), new PropertyMetadata(null, OnTriggerChanged));

        private static void OnTriggerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((InjectedViewActionTrigger)d).OnTriggerChanged(e);
        }

        private void OnTriggerChanged(DependencyPropertyChangedEventArgs e)
        {
            InjectedViewActionHandler<FrameworkElement, object>.Inject(TriggerProperty, this.AssociatedObject, e, (element, param) =>
            {
                this.InvokeActions(param);
            });
        }

        #endregion
    }
}
