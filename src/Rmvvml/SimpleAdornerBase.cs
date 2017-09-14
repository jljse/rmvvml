using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Rmvvml
{
    /// <summary>
    /// Adorner中に簡単にコントロールを表示するためのクラス
    /// </summary>
    public class SimpleAdornerBase : Adorner
    {
        public SimpleAdornerBase(UIElement adornedElement)
            : base(adornedElement)
        {
            Host = new Grid();
        }

        /// <summary>
        /// Adorner内のルート要素
        /// </summary>
        protected Grid Host { get; set; }

        protected override int VisualChildrenCount
        {
            get { return 1; }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            Host.Measure(constraint);
            return base.MeasureOverride(constraint);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            Host.Arrange(new Rect(finalSize));
            return base.ArrangeOverride(finalSize);
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index >= VisualChildrenCount)
            {
                throw new IndexOutOfRangeException();
            }
            return Host;
        }

        public void Show()
        {
            AdornerLayer layer = AdornerLayer.GetAdornerLayer(AdornedElement);
            layer.Add(this);
        }

        public void Hide()
        {
            AdornerLayer layer = AdornerLayer.GetAdornerLayer(AdornedElement);
            layer.Remove(this);
        }
    }
}
