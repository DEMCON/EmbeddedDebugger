using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace EmbeddedDebugger.View.Converters
{
    /// <summary>
    /// Converts objects that can represent "boolean" values.
    /// </summary>
    [ValueConversion(typeof(object), typeof(object))]
    public class NullableBooleanConverter : IValueConverter, IMultiValueConverter
    {
        /// <summary>
        /// Converts a value into an object of type targetType.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter, used as default value.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// Returns Binding.DoNothing for unsupported targetType
        /// Returns DependencyProperty.UnsetValue if conversion fails.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType == typeof(bool))
            {
                return ConvertToBool(value, parameter);
            }
            else if (targetType == typeof(bool?))
            {
                return ConvertToNullableBool(value, parameter);
            }
            else if (typeof(IConvertible).IsAssignableFrom(targetType))
            {
                bool? boolValue = NullableBooleanConverter.ConvertToNullableBool(value, parameter);
                try
                {
                    return System.Convert.ChangeType(boolValue, targetType, culture);
                }
                catch (InvalidCastException)
                {
                    System.Diagnostics.Debug.WriteLine("{0}:ConvertBack failed '{1}' to '{2}', falling back to default '{3}'", this.GetType().Name, value, targetType, parameter);
                    if (parameter == null)
                    {
                        return DependencyProperty.UnsetValue;
                    }
                    else
                    {
                        return this.Convert(parameter, targetType, null, culture);
                    }
                }
            }
            else
            {
                return Binding.DoNothing;
            }
        }

        /// <summary>
        /// Converts a value into an object of type targetType.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter, used as default value.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// Returns Binding.DoNothing for unsupported targetType
        /// Returns DependencyProperty.UnsetValue if conversion fails.
        /// </returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return this.Convert(value, targetType, parameter, culture);
        }

        /// <summary>
        /// Converts source values to a value for the binding target. The data binding engine calls this method when it propagates the values from source bindings to the binding target.
        /// </summary>
        /// <param name="values">The array of values that the source bindings in the <see cref="T:System.Windows.Data.MultiBinding" /> produces. The value <see cref="F:System.Windows.DependencyProperty.UnsetValue" /> indicates that the source binding has no value to provide for conversion.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">
        ///     A string specifying the operator used in this conversion and the default value for invalid boolean values. "'operator'(,'default value')"
        ///     Supported operators are: "AND", "OR", default is "AND,false"</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value.If the method returns null, the valid null value is used.A return value of <see cref="T:System.Windows.DependencyProperty" />.<see cref="F:System.Windows.DependencyProperty.UnsetValue" /> indicates that the converter did not produce a value, and that the binding will use the <see cref="P:System.Windows.Data.BindingBase.FallbackValue" /> if it is available, or else will use the default value.A return value of <see cref="T:System.Windows.Data.Binding" />.<see cref="F:System.Windows.Data.Binding.DoNothing" /> indicates that the binding does not transfer the value or use the <see cref="P:System.Windows.Data.BindingBase.FallbackValue" /> or the default value.
        /// </returns>
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool defaultValue = parameter != null && parameter.ToString().TrimEnd().EndsWith("True", StringComparison.OrdinalIgnoreCase);
            bool useOrOperator = parameter != null && parameter.ToString().TrimStart().TrimStart().StartsWith("OR", StringComparison.OrdinalIgnoreCase);

            return this.Convert(Convert(values, defaultValue, useOrOperator), targetType, null, culture);
        }

        /// <summary>
        /// Converts the back.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="targetTypes">The target types.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>Always null</returns>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            System.Diagnostics.Debug.WriteLine(this.GetType().Name + ":ConvertBack() requested, but not implemented, returning null!");
            return null;
        }

        /// <summary>
        /// Converts value to nullable bool.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>
        /// The to nullable bool converted value.
        /// If conversion fails, the to nullable bool converted default value is returned.
        /// If no default value is available or the conversion of the default value fails, null is returned
        /// </returns>
        internal static bool? ConvertToNullableBool(object value, object defaultValue)
        {
            if (value is bool?)
            {
                bool? result = (bool?)value;
                if (result != null && result.HasValue)
                {
                    return result;
                }
            }
            else if (value is bool)
            {
                return new bool?((bool)value);
            }
            else
            {
                bool result;

                if (TryConvertToBool(value, out result))
                {
                    return new bool?(result);
                }
            }

            // if conversion fails return default value if available or false
            if (defaultValue == null)
            {
                return null;
            }
            else
            {
                return ConvertToNullableBool(defaultValue, null);
            }
        }

        /// <summary>
        /// Converts value to bool.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>
        /// The to bool converted value.
        /// If conversion fails, the to bool converted default value is returned.
        /// If no default value is available or the conversion of the default value fails, false is returned
        /// </returns>
        internal static bool ConvertToBool(object value, object defaultValue)
        {
            if (value is bool)
            {
                return (bool)value;
            }
            else if (value is bool?)
            {
                bool? nullableBool = (bool?)value;
                if (nullableBool.HasValue)
                {
                    return nullableBool.Value;
                }
            }
            else
            {
                bool result;

                if (TryConvertToBool(value, out result))
                {
                    return result;
                }
            }

            // if conversion fails return default value if available or false
            if (defaultValue == null)
            {
                return false;
            }
            else
            {
                return ConvertToBool(defaultValue, null);
            }
        }

        /// <summary>
        /// Tries the convert value to bool.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="result">The converted value. (only of method returns true, otherwise false).</param>
        /// <returns><c>true</c> if [conversion was successful]; otherwise, <c>false</c>.</returns>
        internal static bool TryConvertToBool(object value, out bool result)
        {
            if (value == null)
            {
                result = false;
                return false;
            }

            IConvertible convertible = value as IConvertible;
            if (convertible == null || value is string)
            {
                return bool.TryParse(value.ToString(), out result);
            }

            result = convertible.ToBoolean(null);
            return true;
        }

        /// <summary>
        /// Converts the specified values to a bool result, by applying a AND or OR operation on the bool representations of the values.
        /// DependencyProperty.UnsetValue values are ignored.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="defaultValue">the default value used when converting values to bool representation.</param>
        /// <param name="useOrOperator">if set to <c>true</c> [use OR operator]; otherwise [use AND operator].</param>
        /// <returns>The converted result</returns>
        internal static bool Convert(object[] values, bool defaultValue, bool useOrOperator)
        {
            if (values == null)
            {
                return defaultValue;
            }

            int i = 0;
            bool result = defaultValue;

            // set result to first Boolean
            while (i < values.Length)
            {
                if (values[i] != DependencyProperty.UnsetValue)
                {
                    result = ConvertToBool(values[i], defaultValue);
                    ++i;

                    // first set value found exit while loop
                    break;
                }

                ++i;
            }

            if (useOrOperator)
            {
                // OR the other value with result;
                while (result == false && i < values.Length)
                {
                    if (values[i] != DependencyProperty.UnsetValue)
                    {
                        result |= ConvertToBool(values[i], defaultValue);
                    }

                    ++i;
                }
            }
            else
            {
                // AND the other value with result;
                while (result && i < values.Length)
                {
                    if (values[i] != DependencyProperty.UnsetValue)
                    {
                        result &= ConvertToBool(values[i], defaultValue);
                    }

                    ++i;
                }
            }

            return result;
        }
    }
}