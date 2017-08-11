using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Rmvvml
{
    /// <summary>
    /// 空きスペースいっぱいに広がるようコントロールを配置します
    /// 空きスペース以上には広がらないよう制限されます
    /// </summary>
    public class RestrictDesiredSize : Decorator
    {
        #region IsRestrictWidth

        /// <summary>
        /// 幅が空きスペース以上に広がらないよう制限するかどうか
        /// </summary>
        public bool IsRestrictWidth
        {
            get { return (bool)GetValue(IsRestrictWidthProperty); }
            set { SetValue(IsRestrictWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsRestrictWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsRestrictWidthProperty =
            DependencyProperty.Register("IsRestrictWidth", typeof(bool), typeof(RestrictDesiredSize), new PropertyMetadata(true));

        #endregion

        #region IsRestrictHeight

        /// <summary>
        /// 高さが空きスペース以上に広がらないよう制限するかどうか
        /// </summary>
        public bool IsRestrictHeight
        {
            get { return (bool)GetValue(IsRestrictHeightProperty); }
            set { SetValue(IsRestrictHeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsRestrictHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsRestrictHeightProperty =
            DependencyProperty.Register("IsRestrictHeight", typeof(bool), typeof(RestrictDesiredSize), new PropertyMetadata(true));

        #endregion

        protected override Size MeasureOverride(Size constraint)
        {
            System.Diagnostics.Debug.WriteLine("--- Measure");

            //var actual = new Size(
            //    constraint.Width,
            //    constraint.Height
            //    );
            //Child.Measure(actual);

            if (LastArrangedSize != LastFixedMeasureSize)
            {
                LastFixedMeasureSize = LastArrangedSize;
                return base.MeasureOverride(LastArrangedSize);
            }

            base.MeasureOverride(constraint);

            //var ret = Child.DesiredSize;
            var ret = new Size(
                IsRestrictWidth ? 0 : Child.DesiredSize.Width,
                IsRestrictHeight ? 0 : Child.DesiredSize.Height
                );
            System.Diagnostics.Debug.WriteLine("Measure constraint=" + constraint + " desired=" + Child.DesiredSize + " ret=" + ret);
            return ret;
        }

        Size LastArrangedSize { get; set; } = new Size();
        Size LastFixedMeasureSize { get; set; } = new Size();

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            System.Diagnostics.Debug.WriteLine("--- Arrange");

            //Child.Measure(arrangeSize);
            //MeasureOverride(arrangeSize);
            //Child.Arrange(new Rect(arrangeSize));

            if (LastArrangedSize != arrangeSize)
            {
                LastArrangedSize = arrangeSize;
                System.Diagnostics.Debug.WriteLine("call Measure from Arrange");
                //Child.Measure(arrangeSize);
                InvalidateMeasure();
                return arrangeSize;
            }

            base.ArrangeOverride(arrangeSize);
            //Child.Arrange(new Rect(arrangeSize));

            var ret = arrangeSize;
            System.Diagnostics.Debug.WriteLine("Arrange arrangesize=" + arrangeSize + " ret=" + ret);
            return ret;
        }
    }
}
