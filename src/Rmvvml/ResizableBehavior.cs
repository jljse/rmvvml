using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Interactivity;

namespace Rmvvml
{
    /// <summary>
    /// ドラッグで拡大縮小を可能にします
    /// </summary>
    public class ResizableBehavior : Behavior<Thumb>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.DragDelta += OnDragDelta;
        }

        #region Target

        /// <summary>
        /// 拡大縮小する対象のコントロール
        /// </summary>
        public FrameworkElement Target
        {
            get { return (FrameworkElement)GetValue(TargetProperty); }
            set { SetValue(TargetProperty, value); }
        }

        public static readonly DependencyProperty TargetProperty =
            DependencyProperty.Register("Target", typeof(FrameworkElement), typeof(ResizableBehavior), new PropertyMetadata(null));

        #endregion

        private void OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            if (Target == null) return;

            Target.Width = Math.Max(30, Target.ActualWidth + e.HorizontalChange);
            Target.Height = Math.Max(30, Target.ActualHeight + e.VerticalChange);
        }
    }
}
