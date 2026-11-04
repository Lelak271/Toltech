using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Toltech.App.Properties;
using Toltech.App.Resources.ColorTheme;
using Toltech.App.Resources.Lang;

namespace Toltech.App.Resources
{
    /// <summary>
    /// Anciennes et nouvelles unités, transmises à l'événement
    /// <see cref="AppSettings.UnitsChanged"/> après une sauvegarde réussie.
    /// </summary>
    public sealed class UnitsChangedEventArgs : EventArgs
    {
        public SupportedLengthUnit OldLength { get; init; }
        public SupportedLengthUnit NewLength { get; init; }
        public SupportedAngleUnit OldAngle { get; init; }
        public SupportedAngleUnit NewAngle { get; init; }
        public SupportedDensityUnit OldDensity { get; init; }
        public SupportedDensityUnit NewDensity { get; init; }

        public bool LengthChanged => OldLength != NewLength;
        public bool AngleChanged => OldAngle != NewAngle;
        public bool DensityChanged => OldDensity != NewDensity;
    }

    /// <summary>
    /// Modèle typé et éditable des paramètres utilisateur.
    /// Charge depuis <c>Settings.Default</c>, sauvegarde dedans, puis demande
    /// à <see cref="AppResourceLoader"/> de projeter les valeurs vers le XAML.
    /// </summary>
    public class AppSettings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private void Notify([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        /// <summary>
        /// Levé après une sauvegarde réussie qui modifie au moins une unité.
        /// Permet aux écrans ouverts de reconvertir les valeurs affichées.
        /// </summary>
        public static event EventHandler<UnitsChangedEventArgs>? UnitsChanged;

        // General settings
        public SupportedLanguage Language { get; set; } = SupportedLanguage.fr;
        private AppTheme _theme = AppTheme.System;
        public AppTheme Theme
        {
            get => _theme;
            set { if (_theme == value) return; _theme = value; Notify(); Notify(nameof(IsDark)); }
        }

        // Units
        public SupportedLengthUnit LengthUnit { get; set; } = SupportedLengthUnit.Millimeter;
        public SupportedAngleUnit AngleUnit { get; set; } = SupportedAngleUnit.Degre;
        public SupportedDensityUnit DensityUnit { get; set; } = SupportedDensityUnit.KgPerCubicMeter;

        // Display
        public int DisplayPrecision { get; set; } = 3;

        /// <summary>
        /// Lecture : thème effectivement affiché (System inclus).
        /// Écriture : choix explicite de l'utilisateur (Dark ou Light).
        /// </summary>
        public bool IsDark
        {
            get
            {
                bool value = Theme switch
                {
                    AppTheme.Dark => true,
                    AppTheme.Light => false,
                    _ => SystemThemeHelper.IsDark()
                };
                return value;
            }
            set => Theme = value ? AppTheme.Dark : AppTheme.Light;
        }

        /// <summary>À appeler quand Windows change de thème alors que Theme = System.</summary>
        public void RefreshEffectiveTheme() => Notify(nameof(IsDark));

        public AppSettings()
        {
            // Prend en compte les valeurs utilisateur dès l'initialisation.
            LoadFromUserConfig();
        }

        /// <summary>
        /// Charge l'état persisté. Une valeur absente ou invalide conserve
        /// la valeur par défaut de la propriété (source unique des défauts).
        /// </summary>
        public void LoadFromUserConfig()
        {
            if (Enum.TryParse(Settings.Default.Language, out SupportedLanguage lang))
                Language = lang;

            // NB : la clé de Settings s'appelle "LenghtUnit" (faute de frappe conservée
            // pour ne pas perdre les valeurs déjà enregistrées chez les utilisateurs).
            if (Enum.TryParse(Settings.Default.LenghtUnit, out SupportedLengthUnit lengthUnit))
                LengthUnit = lengthUnit;

            if (Enum.TryParse(Settings.Default.AngleUnit, out SupportedAngleUnit angleUnit))
                AngleUnit = angleUnit;

            if (Enum.TryParse(Settings.Default.DensityUnit, out SupportedDensityUnit densityUnit))
                DensityUnit = densityUnit;

            if (Enum.TryParse(Settings.Default.Theme, out AppTheme theme))
                Theme = theme;
        }

        /// <summary>
        /// Persiste les paramètres, applique les ressources, puis signale
        /// les changements d'unité éventuels.
        /// </summary>
        public void SaveToUserConfig()
        {
            // 1. Relit l'état actuellement persisté AVANT d'écrire :
            var previous = new AppSettings();

            // 2. Écriture dans Settings (en mémoire).
            Settings.Default.Language = Language.ToString();
            Settings.Default.LenghtUnit = LengthUnit.ToString();
            Settings.Default.AngleUnit = AngleUnit.ToString();
            Settings.Default.DensityUnit = DensityUnit.ToString();
            Settings.Default.Theme = Theme.ToString();
            Settings.Default.Save();

            // 4. Projection vers les ressources XAML (symboles inclus) :
            AppResourceLoader.ApplySettings(this);

            // 5. Notification : uniquement si une unité a réellement changé,
            var args = new UnitsChangedEventArgs
            {
                OldLength = previous.LengthUnit,
                NewLength = LengthUnit,
                OldAngle = previous.AngleUnit,
                NewAngle = AngleUnit,
                OldDensity = previous.DensityUnit,
                NewDensity = DensityUnit
            };

            if (args.LengthChanged || args.AngleChanged || args.DensityChanged)
                UnitsChanged?.Invoke(this, args);

        }

 
        /// <summary>Copie les valeurs d'un autre AppSettings (brouillon -> état validé).</summary>
        public void CopyFrom(AppSettings other)
        {
            Language = other.Language;
            Theme = other.Theme;

            LengthUnit = other.LengthUnit;
            AngleUnit = other.AngleUnit;
            DensityUnit = other.DensityUnit;

            DisplayPrecision = other.DisplayPrecision;
        }
    }
}