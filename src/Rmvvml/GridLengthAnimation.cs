using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace Rmvvml
{
    /// <summary>
    /// Animation for GridLength.
    /// </summary>
    public class GridLengthAnimation : AnimationTimeline
    {
        public override Type TargetPropertyType
        {
            get
            {
                return typeof(GridLength);
            }
        }

        protected override Freezable CreateInstanceCore()
        {
            return new GridLengthAnimation();
        }

        public override object GetCurrentValue(object defaultOriginValue, object defaultDestinationValue, AnimationClock animationClock)
        {
            var from = From;
            if (from == GridLength.Auto)
            {
                from = (GridLength)defaultOriginValue;
            }
            var to = To;
            if (to == GridLength.Auto)
            {
                to = (GridLength)defaultDestinationValue;
            }

            if (from.GridUnitType != to.GridUnitType)
            {
                // cannot animate
                return to;
            }

            if (from.IsAuto)
            {
                // cannot animate
                return to;
            }

            double ratio = animationClock.CurrentProgress.Value;
            if (EasingFunction != null)
            {
                ratio = EasingFunction.Ease(ratio);
            }

            var diff = to.Value - from.Value;
            var result = from.Value + diff * ratio;

            return new GridLength(result, from.GridUnitType);
        }

        #region From
        /// <summary>
        /// Start value of animation.
        /// Use current value if not specified.
        /// </summary>
        public GridLength From
        {
            get { return (GridLength)GetValue(FromProperty); }
            set { SetValue(FromProperty, value); }
        }

        public static readonly DependencyProperty FromProperty =
            DependencyProperty.Register("From", typeof(GridLength), typeof(GridLengthAnimation), new PropertyMetadata(GridLength.Auto));
        #endregion

        #region To
        /// <summary>
        /// End value of animation
        /// </summary>
        public GridLength To
        {
            get { return (GridLength)GetValue(ToProperty); }
            set { SetValue(ToProperty, value); }
        }

        public static readonly DependencyProperty ToProperty =
            DependencyProperty.Register("To", typeof(GridLength), typeof(GridLengthAnimation), new PropertyMetadata(GridLength.Auto));
        #endregion

        #region EasingFunction
        /// <summary>
        /// Easing function
        /// </summary>
        public IEasingFunction EasingFunction
        {
            get { return (IEasingFunction)GetValue(EasingFunctionProperty); }
            set { SetValue(EasingFunctionProperty, value); }
        }

        public static readonly DependencyProperty EasingFunctionProperty =
            DependencyProperty.Register("EasingFunction", typeof(IEasingFunction), typeof(GridLengthAnimation), new PropertyMetadata(null));
        #endregion

    }
}
