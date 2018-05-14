using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interactivity;

namespace Rmvvml
{
    public class InteractionAtt
    {
        #region StyleBehaviors

        [AttachedPropertyBrowsableForType(typeof(FrameworkElement))]
        public static StyleBehaviorCollection GetStyleBehaviors(DependencyObject obj)
        {
            return (StyleBehaviorCollection)obj.GetValue(StyleBehaviorsProperty);
        }

        [AttachedPropertyBrowsableForType(typeof(FrameworkElement))]
        public static void SetStyleBehaviors(DependencyObject obj, StyleBehaviorCollection value)
        {
            obj.SetValue(StyleBehaviorsProperty, value);
        }

        /// <summary>
        /// Behaviors from Style Setter
        /// </summary>
        public static readonly DependencyProperty StyleBehaviorsProperty =
            DependencyProperty.RegisterAttached("StyleBehaviors", typeof(StyleBehaviorCollection), typeof(InteractionAtt), new PropertyMetadata(null, OnStyleBehaviorsChanged));

        private static void OnStyleBehaviorsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behaviors = Interaction.GetBehaviors(d);
            foreach(var removing in behaviors.Where(b => GetStyleBehaviors_IsStyleBehavior(b)))
            {
                behaviors.Remove(removing);
            }

            var styleBehaviors = GetStyleBehaviors(d);
            foreach(var sb in styleBehaviors)
            {
                var newBehavior = sb.Clone() as Behavior;
                SetStyleBehaviors_IsStyleBehavior(newBehavior, true);
                behaviors.Add(newBehavior);
            }
        }

        #region StyleBehaviors_IsStyleBehavior

        static bool GetStyleBehaviors_IsStyleBehavior(DependencyObject obj)
        {
            return (bool)obj.GetValue(StyleBehaviors_IsStyleBehaviorProperty);
        }

        static void SetStyleBehaviors_IsStyleBehavior(DependencyObject obj, bool value)
        {
            obj.SetValue(StyleBehaviors_IsStyleBehaviorProperty, value);
        }

        /// <summary>
        /// Wheather Behavior come from StyleBehaviors or not
        /// </summary>
        static readonly DependencyProperty StyleBehaviors_IsStyleBehaviorProperty =
            DependencyProperty.RegisterAttached("StyleBehaviors_IsStyleBehavior", typeof(bool), typeof(InteractionAtt), new PropertyMetadata(false));

        #endregion

        #endregion
    }

    public class StyleBehaviorCollection : List<Behavior>
    {
    }
}
