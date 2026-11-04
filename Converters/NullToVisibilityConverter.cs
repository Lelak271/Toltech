using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Toltech.App.Converters
{
    /// <summary>
    /// Convertit une valeur nulle (ou vide) en Visibility.
    /// Null ou vide → Visible
    /// Non null → Collapsed
    /// </summary>
    public sealed class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Cas null
            if (value == null)
                return Visibility.Visible;

            // Cas tableau vide (ex : byte[])
            if (value is Array array && array.Length == 0)
                return Visibility.Visible;

            // Cas chaîne vide
            if (value is string str && string.IsNullOrWhiteSpace(str))
                return Visibility.Visible;

            // Valeur valide
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("NullToVisibilityConverter ne supporte pas ConvertBack.");
        }
    }

    /// <summary>
    /// Convertit une valeur vide en Visibility.
    /// Vide ou null → Collapsed
    /// Non vide → Visible
    /// </summary>
    public sealed class EmptyToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isEmpty =
                value == null ||
                (value is string str && string.IsNullOrWhiteSpace(str)) ||
                (value is Array array && array.Length == 0);

            return isEmpty ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// Convertit une valeur nulle ou vide en Visibility.
    /// Par défaut : null ou vide → Visible, valeur renseignée → Collapsed.
    /// Invert = true : comportement inverse.
    /// </summary>
    public sealed class NullOrEmptyToVisibilityConverter : IValueConverter
    {
        public bool Invert { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Vérifie si la valeur est nulle ou vide.
            bool isEmpty =
                value == null ||
                (value is string str && string.IsNullOrWhiteSpace(str)) ||
                (value is Array array && array.Length == 0);

            // Applique éventuellement le comportement inverse.
            bool isVisible = Invert ? !isEmpty : isEmpty;

            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }


}
