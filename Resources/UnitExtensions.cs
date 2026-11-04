namespace Toltech.App.Resources
{
    #region Enums

    /// <summary>Unités de longueur. Unité de base : millimètre.</summary>
    public enum SupportedLengthUnit
    {
        Millimeter,  // mm
        Inch         // in
    }

    /// <summary>Unités d'angle. Unité de base : degré.</summary>
    public enum SupportedAngleUnit
    {
        Degre,       // °
        Radian       // rad
    }

    /// <summary>Unités de masse volumique. Unité de base : kg/m³.</summary>
    public enum SupportedDensityUnit
    {
        KgPerCubicMeter,   // kg/m³
        GramPerCm3,        // g/cm³
        PoundPerCubicFoot, // lb/ft³
        PoundPerCubicInch  // lb/in³
    }

    #endregion

    #region Symboles et facteurs

    /// <summary>
    /// Provides extension methods for supported units to retrieve their symbols and conversion factors to base units.
    /// </summary>
    public static class UnitExtensions
    {
        // ---------- Longueur ----------
        public static string Symbol(this SupportedLengthUnit u) => u switch
        {
            SupportedLengthUnit.Millimeter => "mm",
            SupportedLengthUnit.Inch => "in",
            _ => throw new ArgumentOutOfRangeException(nameof(u))
        };

        /// <summary>Valeur de 1 unité exprimée en mm.</summary>
        public static double ToBaseFactor(this SupportedLengthUnit u) => u switch
        {
            SupportedLengthUnit.Millimeter => 1.0,
            SupportedLengthUnit.Inch => 25.4,
            _ => throw new ArgumentOutOfRangeException(nameof(u))
        };

        // ---------- Angle ----------
        public static string Symbol(this SupportedAngleUnit u) => u switch
        {
            SupportedAngleUnit.Degre => "°",
            SupportedAngleUnit.Radian => "rad",
            _ => throw new ArgumentOutOfRangeException(nameof(u))
        };

        /// <summary>Valeur de 1 unité exprimée en degrés.</summary>
        public static double ToBaseFactor(this SupportedAngleUnit u) => u switch
        {
            SupportedAngleUnit.Degre => 1.0,
            SupportedAngleUnit.Radian => 180.0 / Math.PI,
            _ => throw new ArgumentOutOfRangeException(nameof(u))
        };

        // ---------- Masse volumique ----------
        public static string Symbol(this SupportedDensityUnit u) => u switch
        {
            SupportedDensityUnit.KgPerCubicMeter => "kg/m³",
            SupportedDensityUnit.GramPerCm3 => "g/cm³",
            SupportedDensityUnit.PoundPerCubicFoot => "lb/ft³",
            SupportedDensityUnit.PoundPerCubicInch => "lb/in³",
            _ => throw new ArgumentOutOfRangeException(nameof(u))
        };

       
        /// <summary>Valeur de 1 unité exprimée en kg/m³.</summary>
        public static double ToBaseFactor(this SupportedDensityUnit u) => u switch
        {
            SupportedDensityUnit.KgPerCubicMeter => 1.0,
            SupportedDensityUnit.GramPerCm3 => 1000.0,
            SupportedDensityUnit.PoundPerCubicFoot => 16.01846337,
            SupportedDensityUnit.PoundPerCubicInch => 27679.9047,
            _ => throw new ArgumentOutOfRangeException(nameof(u))
        };
    }

    /// <summary>Élément de liste : valeur de l'enum + symbole affiché.</summary>
    public sealed record UnitOption<T>(T Value, string Symbol) where T : struct, Enum;

    public static class UnitOptions
    {
        /// <summary>Construit la liste des choix à partir de l'enum et de sa fonction Symbol.</summary>
        public static IReadOnlyList<UnitOption<T>> Of<T>(Func<T, string> symbol) where T : struct, Enum =>
            Enum.GetValues<T>()
                .Select(u => new UnitOption<T>(u, symbol(u)))
                .ToList();
    }


    #endregion

    #region Conversions

    /// <summary>
    /// Conversions entre unités. Toujours via l'unité de base :
    /// valeur_cible = valeur_source × facteur_source / facteur_cible.
    /// </summary>
    public static class UnitConverter
    {
        // Conversion directe entre deux unités (ex. : l'utilisateur passe de mm à in).
        public static double Convert(double value, SupportedLengthUnit from, SupportedLengthUnit to) =>
            from == to ? value : value * from.ToBaseFactor() / to.ToBaseFactor();

        public static double Convert(double value, SupportedAngleUnit from, SupportedAngleUnit to) =>
            from == to ? value : value * from.ToBaseFactor() / to.ToBaseFactor();

        public static double Convert(double value, SupportedDensityUnit from, SupportedDensityUnit to) =>
            from == to ? value : value * from.ToBaseFactor() / to.ToBaseFactor();

        // Base -> affichage (valeur stockée -> unité choisie par l'utilisateur).
        public static double FromBase(double baseValue, SupportedLengthUnit unit) => baseValue / unit.ToBaseFactor();
        public static double FromBase(double baseValue, SupportedAngleUnit unit) => baseValue / unit.ToBaseFactor();
        public static double FromBase(double baseValue, SupportedDensityUnit unit) => baseValue / unit.ToBaseFactor();

        // Saisie -> base (valeur tapée dans l'unité courante -> valeur à stocker).
        public static double ToBase(double value, SupportedLengthUnit unit) => value * unit.ToBaseFactor();
        public static double ToBase(double value, SupportedAngleUnit unit) => value * unit.ToBaseFactor();
        public static double ToBase(double value, SupportedDensityUnit unit) => value * unit.ToBaseFactor();
    }

    #endregion
}