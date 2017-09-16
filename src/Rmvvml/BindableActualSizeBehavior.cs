using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interactivity;

namespace demo3
{
    /// <summary>
    /// ActualWidthとActualHeightにバインドするためのビヘイビア
    /// OneWayToSource相当の動作です
    /// </summary>
    class BindableActualSizeBehavior : Behavior<FrameworkElement>
    {
        #region ActualWidth
        public double ActualWidth
        {
            get { return (double)GetValue(ActualWidthProperty); }
            set { SetValue(ActualWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ActualWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ActualWidthProperty =
            DependencyProperty.Register("ActualWidth", typeof(double), typeof(BindableActualSizeBehavior), new PropertyMetadata(double.NaN));
        #endregion

        #region ActualHeight
        public double ActualHeight
        {
            get { return (double)GetValue(ActualHeightProperty); }
            set { SetValue(ActualHeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ActualHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ActualHeightProperty =
            DependencyProperty.Register("ActualHeight", typeof(double), typeof(BindableActualSizeBehavior), new PropertyMetadata(double.NaN));
        #endregion

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += OnLoaded;
            AssociatedObject.SizeChanged += OnSizeChanged;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateSavedSize();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateSavedSize();
        }

        private void UpdateSavedSize()
        {
            ActualWidth = AssociatedObject.ActualWidth;
            ActualHeight = AssociatedObject.ActualHeight;
        }
    }
}
