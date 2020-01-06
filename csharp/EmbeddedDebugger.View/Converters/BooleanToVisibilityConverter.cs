using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace EmbeddedDebugger.View.Converters
{
    /// <summary>
    /// Convert a boolean to Visibility
    /// </summary>
    [ValueConversion(typeof(object), typeof(Visibility))]
    public class BooleanToVisibilityConverter : IValueConverter, IMultiValueConverter
    {
        /// <summary>
        /// Converts a value with boolean representation to Visibility.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property. Only Visibility allowed.</param>
        /// <param name="parameter">The Visibility used if value has no boolean representation.</param>
        /// <param name="culture">This parameter is ignored.</param>
        /// <returns>The value converted to Visibility.</returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Debug.Assert(typeof(Visibility).IsAssignableFrom(targetType), this.GetType().Name + " Convert() supports only 'Visibility' as target type");
            bool? result = NullableBooleanConverter.ConvertToNullableBool(value, null);
            if (result != null && result.HasValue)
            {
                return result.Value ? Visibility.Visible : InvisibleVisibility(ParseVisibility(parameter));
            }
            else
            {
                return ParseVisibility(parameter);
            }
        }

        /// <summary>
        /// One way only converter, so not implemented.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>Always Binding.DoNothing because converting back is not implemented</returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            System.Diagnostics.Debug.WriteLine(this.GetType().Name + " ConvertBack() requested, but not implemented, returning Binding.DoNothing!");
            return Binding.DoNothing;
        }

        /// <summary>
        /// Converts multiple values with boolean representation to Visibility.
        /// </summary>
        /// <param name="values">The values to convert</param>
        /// <param name="targetType">The type of the binding target property. Only Visibility allowed.</param>
        /// <param name="parameter">
        ///     A string specifying the operator used in this conversion and the default value for invalid boolean values. "'operator'(,'default value')"
        ///     Supported operators are: "AND", "OR", default is "AND,Hidden"</param>
        /// <param name="culture">This parameter is ignored.</param>
        /// <returns>The Visibility value calculated from values</returns>
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Debug.Assert(typeof(Visibility).IsAssignableFrom(targetType), "Only Visibility allowed as type of the binding target property");

            Visibility defaultVisibility = ParseVisibility(parameter);

            if (NullableBooleanConverter.Convert(values, defaultVisibility == Visibility.Visible, ParseOrOperator(parameter)))
            {
                return Visibility.Visible;
            }
            else
            {
                return InvisibleVisibility(ParseVisibility(parameter));
            }
        }

        /// <summary>
        /// One way only converter, so not implemented.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetTypes">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>Always null because converting back is not implemented</returns>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            System.Diagnostics.Debug.WriteLine(this.GetType().Name + " ConvertBack() requested, but not implemented, returning null!");
            return null;
        }

        /// <summary>
        /// Get the Visibility used to make things invisible. Its the same as the default visibility, except for Visibility.Visible.
        /// </summary>
        /// <param name="defaultVisibility">The default visibility.</param>
        /// <returns>the defaultVisibility, except if the defaultVisibility is Visibility.Visible it returns Visibility.Hidden</returns>
        private static Visibility InvisibleVisibility(Visibility defaultVisibility)
        {
            if (defaultVisibility == Visibility.Collapsed)
            {
                return Visibility.Collapsed;
            }
            else
            {
                return Visibility.Hidden;
            }
        }

        /// <summary>
        /// Parses the string representation of parameter from the end for a visibility value.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>the parsed visibility. If parsing fails Visibility.Hidden is returned</returns>
        private static Visibility ParseVisibility(object parameter)
        {
            if (parameter == null)
            {
                return Visibility.Hidden;
            }
            else
            {
                string param = parameter.ToString().TrimEnd();
                if (param.EndsWith("Visible", StringComparison.OrdinalIgnoreCase))
                {
                    return Visibility.Visible;
                }
                else
                {
                    if (param.EndsWith("Collapsed", StringComparison.OrdinalIgnoreCase))
                    {
                        return Visibility.Collapsed;
                    }
                    else
                    {
                        return Visibility.Hidden;
                    }
                }
            }
        }

        /// <summary>
        /// Parses the string representation of parameter from the beginning for operator value.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>true if a OR operator is found, otherwise false.</returns>
        private static bool ParseOrOperator(object parameter)
        {
            return parameter != null && parameter.ToString().TrimStart().StartsWith("OR", StringComparison.OrdinalIgnoreCase);
        }
    }
}