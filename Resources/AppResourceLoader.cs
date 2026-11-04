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
        public static void ApplySettings(AppSettings appSetting)
        {
            var ressource = Application.Current.Resources;
            
            LanguageManager.ApplyFromSettings();
            ThemeManager.ApplyFromSettings();

            ressource["LanguageDisplay"] = appSetting.Language.DisplayName();
            ressource["Theme"] = appSetting.Theme.ToString();
            ressource["LengthUnitSymbol"] = appSetting.LengthUnit.Symbol();
            ressource["AngleUnitSymbol"] = appSetting.AngleUnit.Symbol();
            ressource["DensityUnitSymbol"] = appSetting.DensityUnit.Symbol();

        }
    }
}