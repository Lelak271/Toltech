using System.Windows;
using Toltech.App.Resources.ColorTheme;
using Toltech.App.Resources.Lang;

namespace Toltech.App.Resources
{
    /// <summary>
    /// Projette un <see cref="AppSettings"/> vers <c>Application.Resources</c>
    /// pour les <c>DynamicResource</c> du XAML. Ne parse rien et ne définit aucun défaut.
    /// </summary>
    public static class AppResourceLoader
    {
        public static void ApplySettings(AppSettings s)
        {
            var res = Application.Current.Resources;
            
            LanguageManager.ApplyFromSettings();
            ThemeManager.ApplyFromSettings();

            // Noms d'enum (pour les converters / sélections).
            res["LanguageDisplay"] = s.Language.DisplayName();
            res["Theme"] = s.Theme.ToString();

            // Symboles affichables (en-têtes de colonnes, labels...).
            // Chaque affectation REMPLACE la ressource, ce qui déclenche
            // la mise à jour des DynamicResource.
            res["LengthUnitSymbol"] = s.LengthUnit.Symbol();
            res["AngleUnitSymbol"] = s.AngleUnit.Symbol();
            res["DensityUnitSymbol"] = s.DensityUnit.Symbol();

        }
    }
}