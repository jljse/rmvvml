using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace Rmvvml
{
    /// <summary>
    /// ドラッグ中にスクロールエリアの周囲へカーソルを移動することで自動的にスクロールするようにします
    /// DragOverイベントを使用するため、動作させるにはAllowDrop=Trueを設定している必要があります
    /// </summary>
    public class AutoScrollByDragBehavior : Behavior<FrameworkElement>
    {
        /// <summary>
        /// 反応する範囲の割合
        /// </summary>
        public double ScrollAreaRatio { get; set; } = 0.2;

        ScrollViewer Scroll { get; set; }
        DateTime LastMove { get; set; } = DateTime.Now;

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.Loaded += AssociatedObject_Loaded;
            AssociatedObject.PreviewDragOver += AssociatedObject_PreviewDragOver;
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            Scroll = VisualTreeHelperExtension.DescendantsOrSelf(AssociatedObject).OfType<ScrollViewer>().First();
        }

        private void AssociatedObject_PreviewDragOver(object sender, DragEventArgs e)
        {
            if (LastMove.AddMilliseconds(50) > DateTime.Now)
            {
                return;
            }
            LastMove = DateTime.Now;

            var moveAreaWidth = Scroll.ActualWidth * ScrollAreaRatio;
            var moveAreaHeight = Scroll.ActualHeight * ScrollAreaRatio;
            var bodyAreaLeft = moveAreaWidth;
            var bodyAreaTop = moveAreaHeight;
            var bodyAreaRight = Scroll.ActualWidth - moveAreaWidth;
            var bodyAreaBottom = Scroll.ActualHeight - moveAreaHeight;

            bool isScroll = false;
            var pos = e.GetPosition(Scroll);
            if (pos.X < bodyAreaLeft)
            {
                // ←へスクロール
                isScroll = true;
                var maxOffset = Math.Max(10, Scroll.ScrollableWidth * 0.1);
                var positionRatio = (bodyAreaLeft - pos.X) / moveAreaWidth;
                var offset = -maxOffset * positionRatio;
                Scroll.ScrollToHorizontalOffset(Math.Max(0, Scroll.HorizontalOffset + offset));
            }
            if (pos.Y < bodyAreaTop)
            {
                // ↑へスクロール
                isScroll = true;
                var maxOffset = Math.Max(10, Scroll.ScrollableHeight * 0.1);
                var positionRatio = (bodyAreaTop - pos.Y) / moveAreaHeight;
                var offset = -maxOffset * positionRatio;
                Scroll.ScrollToVerticalOffset(Math.Max(0, Scroll.VerticalOffset + offset));
            }
            if (pos.X > bodyAreaRight)
            {
                // →へスクロール
                isScroll = true;
                var maxOffset = Math.Max(10, Scroll.ScrollableWidth * 0.1);
                var positionRatio = (pos.X - bodyAreaRight) / moveAreaWidth;
                var offset = maxOffset * positionRatio;
                Scroll.ScrollToHorizontalOffset(Math.Min(Scroll.ScrollableWidth, Scroll.HorizontalOffset + offset));
            }
            if (pos.Y > bodyAreaBottom)
            {
                // ↓へスクロール
                isScroll = true;
                var maxOffset = Math.Max(10, Scroll.ScrollableHeight * 0.1);
                var positionRatio = (pos.Y - bodyAreaBottom) / moveAreaHeight;
                var offset = maxOffset * positionRatio;
                Scroll.ScrollToVerticalOffset(Math.Min(Scroll.ScrollableHeight, Scroll.VerticalOffset + offset));
            }
        }
    }

}
