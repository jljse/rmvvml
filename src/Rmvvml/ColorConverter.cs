using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Rmvvml
{
    /// <summary>
    /// Convert Color to another Color by operations list of
    /// - set A/R/G/B  (#AARRGGBB)
    /// - get complemental color  (!)
    /// - add or sub A/R/G/B  (+#AARRGGBB or -#AARRGGBB)
    /// For example, "! -#333" will return darker complemental color.
    /// </summary>
    public class ColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return value;
            if (parameter == null) return value;
            if (!(parameter is string))
            {
                return value;
            }
            var paramStr = parameter as string;

            var operations = ParseOperations(paramStr);

            if (value is Color)
            {
                var color = (Color)value;
                var newColor = ApplyOperations(operations, color);
                return newColor;
            }
            if (value is SolidColorBrush)
            {
                var brush = value as SolidColorBrush;
                var darkColor = ApplyOperations(operations, brush.Color);
                var newBrush = new SolidColorBrush(darkColor);
                return newBrush;
            }
            if (value is LinearGradientBrush)
            {
                var brush = value as LinearGradientBrush;
                var newBrush = brush.Clone();
                newBrush.GradientStops = new GradientStopCollection(brush.GradientStops.Select(stop =>
                {
                    var newStop = stop.Clone();
                    newStop.Color = ApplyOperations(operations, stop.Color);
                    return newStop;
                }));
                return newBrush;
            }

            return value;
        }

        byte ToColorByte(string str)
        {
            if (str.Length == 1)
            {
                return (byte)(System.Convert.ToByte(str, 16) * 0x11);
            }
            if (str.Length == 2)
            {
                return System.Convert.ToByte(str, 16);
            }

            throw new NotImplementedException();
        }

        byte? ToNullableByte(string str)
        {
            if (str.StartsWith("X") || str.StartsWith("x"))
            {
                return null;
            }
            else
            {
                return ToColorByte(str);
            }
        }

        Color ParseColorValue(string colorStr)
        {
            Color ret;

            switch (colorStr.Length)
            {
                case 3: // RGB
                    ret = Color.FromArgb(
                        0x00,
                        ToColorByte(colorStr.Substring(0, 1)),
                        ToColorByte(colorStr.Substring(1, 1)),
                        ToColorByte(colorStr.Substring(2, 1))
                        );
                    break;
                case 4: // ARGB
                    ret = Color.FromArgb(
                        ToColorByte(colorStr.Substring(0, 1)),
                        ToColorByte(colorStr.Substring(1, 1)),
                        ToColorByte(colorStr.Substring(2, 1)),
                        ToColorByte(colorStr.Substring(3, 1))
                        );
                    break;
                case 6: // RRGGBB
                    ret = Color.FromArgb(
                        0x00,
                        ToColorByte(colorStr.Substring(0, 2)),
                        ToColorByte(colorStr.Substring(2, 2)),
                        ToColorByte(colorStr.Substring(4, 2))
                        );
                    break;
                case 8: // AARRGGBB
                    ret = Color.FromArgb(
                        ToColorByte(colorStr.Substring(0, 2)),
                        ToColorByte(colorStr.Substring(2, 2)),
                        ToColorByte(colorStr.Substring(4, 2)),
                        ToColorByte(colorStr.Substring(6, 2))
                        );
                    break;
                default:
                    throw new NotImplementedException();
            }

            return ret;
        }

        ColorModOperationFix ParseFixColorValue(string colorStr)
        {
            ColorModOperationFix ret;

            switch (colorStr.Length)
            {
                case 3: // RGB
                    ret = new ColorModOperationFix(
                        null,
                        ToNullableByte(colorStr.Substring(0, 1)),
                        ToNullableByte(colorStr.Substring(1, 1)),
                        ToNullableByte(colorStr.Substring(2, 1))
                        );
                    break;
                case 4: // ARGB
                    ret = new ColorModOperationFix(
                        ToNullableByte(colorStr.Substring(0, 1)),
                        ToNullableByte(colorStr.Substring(1, 1)),
                        ToNullableByte(colorStr.Substring(2, 1)),
                        ToNullableByte(colorStr.Substring(3, 1))
                        );
                    break;
                case 6: // RRGGBB
                    ret = new ColorModOperationFix(
                        null,
                        ToNullableByte(colorStr.Substring(0, 2)),
                        ToNullableByte(colorStr.Substring(2, 2)),
                        ToNullableByte(colorStr.Substring(4, 2))
                        );
                    break;
                case 8: // AARRGGBB
                    ret = new ColorModOperationFix(
                        ToNullableByte(colorStr.Substring(0, 2)),
                        ToNullableByte(colorStr.Substring(2, 2)),
                        ToNullableByte(colorStr.Substring(4, 2)),
                        ToNullableByte(colorStr.Substring(6, 2))
                        );
                    break;
                default:
                    throw new NotImplementedException();
            }

            return ret;
        }

        List<IColorModOperation> ParseOperations(string opeStr)
        {
            if (string.IsNullOrEmpty(opeStr)) return new List<IColorModOperation>();

            return opeStr
                .Split(' ')
                .Select(ope =>
                {
                    if (string.IsNullOrEmpty(ope))
                    {
                        return new ColorModOperationKeep();
                    }
                    IColorModOperation ret;
                    switch (ope[0])
                    {
                        case '#':
                            ret = ParseFixColorValue(ope.Substring(1));
                            break;
                        case '!':
                            ret = new ColorModOperationComplement();
                            break;
                        case '+':
                            ret = new ColorModOperationAddSub(ParseColorValue(ope.Substring(2)), true);
                            break;
                        case '-':
                            ret = new ColorModOperationAddSub(ParseColorValue(ope.Substring(2)), false);
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                    return ret;
                }).ToList();
        }

        Color ApplyOperations(List<IColorModOperation> operations, Color baseColor)
        {
            var temp = baseColor;
            foreach (var ope in operations)
            {
                temp = ope.Modify(temp);
            }
            return temp;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    interface IColorModOperation
    {
        Color Modify(Color baseColor);
    }

    class ColorModOperationKeep : IColorModOperation
    {
        public Color Modify(Color baseColor)
        {
            return baseColor;
        }
    }

    class ColorModOperationComplement : IColorModOperation
    {
        public Color Modify(Color baseColor)
        {
            var max = new[] { baseColor.R, baseColor.G, baseColor.B }.Max();
            return Color.FromArgb(
                baseColor.A,
                (byte)(max - baseColor.R),
                (byte)(max - baseColor.G),
                (byte)(max - baseColor.B)
                );
        }
    }

    class ColorModOperationAddSub : IColorModOperation
    {
        bool IsAdd { get; set; }
        Color Diff { get; set; }

        public ColorModOperationAddSub(Color diff, bool isAdd)
        {
            IsAdd = isAdd;
            Diff = diff;
        }

        public Color Modify(Color baseColor)
        {
            if (IsAdd)
            {
                return Color.FromArgb(
                    (byte)Math.Min(0xff, baseColor.A + Diff.A),
                    (byte)Math.Min(0xff, baseColor.R + Diff.R),
                    (byte)Math.Min(0xff, baseColor.G + Diff.G),
                    (byte)Math.Min(0xff, baseColor.B + Diff.B)
                    );
            }
            else
            {
                return Color.FromArgb(
                    (byte)Math.Max(0x00, baseColor.A - Diff.A),
                    (byte)Math.Max(0x00, baseColor.R - Diff.R),
                    (byte)Math.Max(0x00, baseColor.G - Diff.G),
                    (byte)Math.Max(0x00, baseColor.B - Diff.B)
                    );
            }
        }
    }

    class ColorModOperationFix : IColorModOperation
    {
        byte? A { get; set; }
        byte? R { get; set; }
        byte? G { get; set; }
        byte? B { get; set; }

        public ColorModOperationFix(byte? a, byte? r, byte? g, byte? b)
        {
            A = a;
            R = r;
            G = g;
            B = b;
        }

        public Color Modify(Color baseColor)
        {
            return Color.FromArgb(
                A != null ? A.Value : baseColor.A,
                R != null ? R.Value : baseColor.R,
                G != null ? G.Value : baseColor.G,
                B != null ? B.Value : baseColor.B
                );
        }
    }
}
