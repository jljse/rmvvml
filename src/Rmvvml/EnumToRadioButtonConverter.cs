using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Rmvvml
{
    /// <summary>
    /// enum値がパラメータと等しいかどうかを返すコンバータです
    /// RadioButton.IsCheckedで使用することを想定しています
    /// </summary>
    public class EnumToRadioButtonConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(Nullable<bool>) && targetType != typeof(bool))
            {
                throw new InvalidCastException();
            }
            if (value == null)
            {
                return false;
            }
            if (parameter == null)
            {
                return Binding.DoNothing;
            }
            if (!value.GetType().IsEnum)
            {
                throw new InvalidCastException();
            }

            object param = null;
            if (parameter.GetType() == value.GetType())
            {
                param = parameter;
            }
            else if (parameter is string)
            {
                param = Enum.Parse(value.GetType(), parameter as string);
            }
            else
            {
                throw new InvalidCastException();
            }

            if (value.Equals(param))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool))
            {
                throw new InvalidCastException();
            }
            if (!targetType.IsEnum)
            {
                throw new InvalidCastException();
            }
            bool val = (bool)value;
            if (!val)
            {
                return Binding.DoNothing;
            }

            object param = null;
            if (parameter.GetType() == targetType)
            {
                param = parameter;
            }
            else if (parameter is string)
            {
                param = Enum.Parse(targetType, parameter as string);
            }
            else
            {
                throw new InvalidCastException();
            }

            return param;
        }
    }
}
