using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Rmvvml
{
    public class RadioButtonAtt
    {
        #region CheckFor

        enum CheckFor_UnsetValue
        {
            UnsetValue,
        }

        [AttachedPropertyBrowsableForType(typeof(RadioButton))]
        public static Enum GetCheckFor(DependencyObject obj)
        {
            return (Enum)obj.GetValue(CheckForProperty);
        }

        [AttachedPropertyBrowsableForType(typeof(RadioButton))]
        public static void SetCheckFor(DependencyObject obj, Enum value)
        {
            obj.SetValue(CheckForProperty, value);
        }

        /// <summary>
        /// Treat RadioButton's check state as Enum
        /// </summary>
        public static readonly DependencyProperty CheckForProperty =
            DependencyProperty.RegisterAttached(
                "CheckFor",
                typeof(Enum),
                typeof(RadioButtonAtt),
                new FrameworkPropertyMetadata(CheckFor_UnsetValue.UnsetValue, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnCheckForChanged));

        private static void OnCheckForChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RadioButton radio = d as RadioButton;
            if (radio == null) return;

            // first change as initialize
            if (CheckFor_UnsetValue.UnsetValue.Equals(e.OldValue))
            {
                radio.DataContextChanged += CheckFor_DataContextChanged;
                radio.Checked += CheckFor_Checked;

                CheckFor_UpdateSetting(radio);
            }

            if (e.NewValue == null)
            {
                radio.IsChecked = false;
            }
            else
            {
                if (GetSelectedValue(radio)?.ToString() == e.NewValue?.ToString())
                {
                    radio.IsChecked = true;
                }
            }
        }

        private static void CheckFor_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radio = sender as RadioButton;
            if (radio == null) return;

            // apply to VM
            SetCheckFor(radio, GetSelectedValue_Converted(radio));
        }

        private static void CheckFor_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            RadioButton radio = sender as RadioButton;
            if (radio == null) return;

            CheckFor_UpdateSetting(radio);
        }

        // convert SelectedValue to Enum and save to SelectedValue_Converted
        private static void CheckFor_UpdateSetting(RadioButton radio)
        {
            var be = BindingOperations.GetBindingExpression(radio, CheckForProperty);
            if (be == null) return;
            if (be.ResolvedSource == null) return;

            if (CheckFor_UnsetValue.UnsetValue.Equals(GetSelectedValue(radio))) return;

            // group RadioButton which have same binding source
            radio.GroupName = CheckFor_GenerateGroupName(be);

            var srcProp = be.ResolvedSource.GetType().GetProperty(be.ResolvedSourcePropertyName);
            if (srcProp.PropertyType.IsEnum)
            {
                // Enum.Parse throws exception for bad setting
                var enumVal = Enum.Parse(srcProp.PropertyType, GetSelectedValue(radio)?.ToString()) as Enum;
                SetSelectedValue_Converted(radio, enumVal);
                return;
            }

            if (srcProp.PropertyType.IsGenericType
                && srcProp.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var genericParamType = srcProp.PropertyType.GetGenericArguments()[0];
                if (genericParamType.IsEnum)
                {
                    var valueOnSelected = GetSelectedValue(radio);
                    if (valueOnSelected == null)
                    {
                        SetSelectedValue_Converted(radio, null);
                    }
                    else
                    {
                        // Enum.Parse throws exception for bad setting
                        var enumVal = Enum.Parse(genericParamType, valueOnSelected.ToString()) as Enum;
                        SetSelectedValue_Converted(radio, enumVal);
                    }
                    return;
                }
            }

            throw new ArgumentException(string.Format("{0}({1}) is not Enum", be.ResolvedSourcePropertyName, srcProp.PropertyType.Name));
        }

        #region CheckFor RadioButton.GroupName

        // Mapping from binding source object to Guid
        static ConditionalWeakTable<object, Box<Guid>> CheckFor_BoundObjectIds = new ConditionalWeakTable<object, Box<Guid>>();

        class Box<T>
        {
            public Box(T value)
            {
                Value = value;
            }

            public T Value { get; set; }
        }

        static Guid CheckFor_GetObjectId(object obj)
        {
            Box<Guid> savedId;
            if (CheckFor_BoundObjectIds.TryGetValue(obj, out savedId))
            {
                return savedId.Value;
            }
            else
            {
                var newId = Guid.NewGuid();
                CheckFor_BoundObjectIds.Add(obj, new Box<Guid>(newId));
                return newId;
            }
        }

        // generate unique GroupName for each binding source
        static string CheckFor_GenerateGroupName(BindingExpression be)
        {
            var objid = CheckFor_GetObjectId(be.ResolvedSource);
            var path = be.ResolvedSourcePropertyName;

            return string.Format("{0}.{1}", objid.ToString(), path);
        }

        #endregion

        #endregion

        #region SelectedValue

        [AttachedPropertyBrowsableForType(typeof(RadioButton))]
        public static object GetSelectedValue(DependencyObject obj)
        {
            return (object)obj.GetValue(SelectedValueProperty);
        }

        [AttachedPropertyBrowsableForType(typeof(RadioButton))]
        public static void SetSelectedValue(DependencyObject obj, object value)
        {
            obj.SetValue(SelectedValueProperty, value);
        }

        /// <summary>
        /// Enum value which this RadioButton correspond.
        /// Use with CheckFor
        /// </summary>
        public static readonly DependencyProperty SelectedValueProperty =
            DependencyProperty.RegisterAttached("SelectedValue", typeof(object), typeof(RadioButtonAtt), new PropertyMetadata(CheckFor_UnsetValue.UnsetValue, OnSelectedValueChanged));

        private static void OnSelectedValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RadioButton radio = d as RadioButton;
            if (radio == null) return;

            CheckFor_UpdateSetting(radio);
        }

        #region SelectedValue_Converted

        static Enum GetSelectedValue_Converted(DependencyObject obj)
        {
            return (Enum)obj.GetValue(SelectedValue_ConvertedProperty);
        }

        static void SetSelectedValue_Converted(DependencyObject obj, Enum value)
        {
            obj.SetValue(SelectedValue_ConvertedProperty, value);
        }

        /// <summary>
        /// SelectedValue as Enum
        /// </summary>
        static readonly DependencyProperty SelectedValue_ConvertedProperty =
            DependencyProperty.RegisterAttached("SelectedValue_Converted", typeof(Enum), typeof(RadioButtonAtt), new PropertyMetadata(null));

        #endregion

        #endregion
    }
}
