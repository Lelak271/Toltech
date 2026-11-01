using System.Globalization;
using System.Windows.Data;
using Toltech.App.Resources;

namespace Toltech.App.Converters
{
    /// <summary>
    /// Base commune des converters d'unités.
    /// La valeur du modèle est toujours en unité de base (mm, °, kg/m³) ;
    /// le converter ne fait que l'affichage et la saisie dans l'unité choisie par l'utilisateur.
    /// Les classes dérivées indiquent seulement quelle unité lire et comment convertir.
    /// </summary>
    public abstract class UnitValueConverter<TUnit> : IValueConverter
        where TUnit : struct, Enum
    {
        /// <summary>Unité actuellement choisie par l'utilisateur (état validé).</summary>
        protected abstract TUnit CurrentUnit { get; }

        /// <summary>Base -> unité choisie.</summary>
        protected abstract double FromBase(double baseValue, TUnit unit);

        /// <summary>Unité choisie -> base.</summary>
        protected abstract double ToBase(double value, TUnit unit);

        // Stockage (unité de base) -> affichage (unité choisie)
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!TryGetDouble(value, out double baseValue))
                return value;

            return FromBase(baseValue, CurrentUnit);
        }

        // Saisie (unité choisie) -> stockage (unité de base)
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double typed;

            if (value is string raw)
            {
                // Cellule texte (DataGrid, TextBox) : la saisie arrive en chaîne.
                var text = raw.Trim();

                if (text.Length == 0)
                    return Binding.DoNothing;

                text = text.Replace('.', ',');

                if (!double.TryParse(text, NumberStyles.Float, CultureInfo.GetCultureInfo("fr-FR"), out typed))
                    return Binding.DoNothing;
            }
            else if (!TryGetDouble(value, out typed))
            {
                // Valeur numérique (NumberBox.Value) ou null.
                return value is null && Nullable.GetUnderlyingType(targetType) != null
                    ? null!
                    : Binding.DoNothing;
            }

            if (double.IsNaN(typed))
                return Binding.DoNothing;

            double result = ToBase(typed, CurrentUnit);

            var target = Nullable.GetUnderlyingType(targetType) ?? targetType;

            return target == typeof(double)
                ? result
                : System.Convert.ChangeType(result, target, CultureInfo.InvariantCulture);
        }

        private static bool TryGetDouble(object value, out double result)
        {
            switch (value)
            {
                case double d: result = d; return true;
                case float f: result = f; return true;
                case decimal m: result = (double)m; return true;
                default: result = 0; return false;
            }
        }
    }

    /// <summary>Longueur : stockage en mm.</summary>
    public sealed class LengthUnitConverter : UnitValueConverter<SupportedLengthUnit>
    {
        protected override SupportedLengthUnit CurrentUnit => App.Settings.LengthUnit;
        protected override double FromBase(double baseValue, SupportedLengthUnit unit) => UnitConverter.FromBase(baseValue, unit);
        protected override double ToBase(double value, SupportedLengthUnit unit) => UnitConverter.ToBase(value, unit);
    }

    /// <summary>Angle : stockage en degrés.</summary>
    public sealed class AngleUnitConverter : UnitValueConverter<SupportedAngleUnit>
    {
        protected override SupportedAngleUnit CurrentUnit => App.Settings.AngleUnit;
        protected override double FromBase(double baseValue, SupportedAngleUnit unit) => UnitConverter.FromBase(baseValue, unit);
        protected override double ToBase(double value, SupportedAngleUnit unit) => UnitConverter.ToBase(value, unit);
    }

    /// <summary>Masse volumique : stockage en kg/m³.</summary>
    public sealed class DensityUnitConverter : UnitValueConverter<SupportedDensityUnit>
    {
        protected override SupportedDensityUnit CurrentUnit => App.Settings.DensityUnit;
        protected override double FromBase(double baseValue, SupportedDensityUnit unit) => UnitConverter.FromBase(baseValue, unit);
        protected override double ToBase(double value, SupportedDensityUnit unit) => UnitConverter.ToBase(value, unit);
    }
}