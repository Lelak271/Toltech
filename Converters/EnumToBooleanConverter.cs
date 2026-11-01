using System.Globalization;
using System.Windows.Data;

namespace Toltech.App.Converters
{
    /// <summary>Lie un RadioButton à une valeur d'enum (ConverterParameter = valeur représentée).</summary>
    public sealed class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value?.Equals(parameter) == true;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            value is true ? parameter : Binding.DoNothing; // décocher ne doit rien écrire
    }

}