using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interactivity;

namespace Rmvvml
{
    /// <summary>
    /// ThumbをドラッグすることでCanvasの中を移動できるようにします
    /// </summary>
    class MovableInCanvasBehavior : Behavior<Thumb>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.DragDelta += OnDragDelta;
        }


        #region Target
        /// <summary>
        /// 移動する対象のコントロール
        /// 実際には、このコントロールの祖先でCanvas直下のコントロールを探し、そのCanvas.LeftとCanvas.Topを設定します
        /// </summary>
        public FrameworkElement Target
        {
            get { return (FrameworkElement)GetValue(TargetProperty); }
            set { SetValue(TargetProperty, value); }
        }

        public static readonly DependencyProperty TargetProperty =
            DependencyProperty.Register("Target", typeof(FrameworkElement), typeof(MovableInCanvasBehavior), new PropertyMetadata(null));
        #endregion


        private void OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            if (Target == null) return;

            // itemscontrolのitemscontainerを探す方法がよくわからない
            var container = VisualTreeHelperExtension
                .Ancestors(AssociatedObject)
                .OfType<UIElement>()
                .TakeUntilButNotIncluding(x => x is Canvas)
                .LastOrDefault();
            if (container == null) return;

            Canvas.SetLeft(container, Math.Max(0, Canvas.GetLeft(container) + e.HorizontalChange));
            Canvas.SetTop(container, Math.Max(0, Canvas.GetTop(container) + e.VerticalChange));
        }

    }
}
