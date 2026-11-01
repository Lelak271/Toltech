using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using Toltech.App.Resources;
using Toltech.App.Resources.Lang;

namespace Toltech.App.Views
{
    public partial class SettingsPage : Wpf.Ui.Controls.FluentWindow
    {
        // Brouillon : instance locale, jamais vue par le reste de l'application.
        private readonly AppSettings _draft = new AppSettings();   // le constructeur relit la config persistée

        public SettingsPage()
        {
            InitializeComponent();

            // Liste des densités générée depuis l'enum : à assigner AVANT le DataContext.
            DensityUnitComboBox.ItemsSource = UnitOptions.Of<SupportedDensityUnit>(u => u.Symbol());
            LanguageComboBox.ItemsSource = LanguageOptions.Of<SupportedLanguage>(u => u.DisplayName());

            // Les bindings lisent l'état initial ici.
            DataContext = _draft;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Le brouillon devient l'état validé, puis on persiste et on applique.
            App.Settings.CopyFrom(_draft);
            App.Settings.SaveToUserConfig();
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) => Close();

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // Les bindings modifient l'instance globale en direct : on la remet
            // sur l'état persisté (annulation, croix de fermeture, ou après sauvegarde).
            _draft.LoadFromUserConfig();
        }

        /// <summary>Dirty tracking : reçoit SelectionChanged et Checked de tous les contrôles.</summary>
        private void OnSettingChanged(object sender, RoutedEventArgs e)
        {
            // Ignore les événements déclenchés par l'initialisation des bindings.
            if (IsLoaded)
                SaveButton.IsEnabled = true;
        }

        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LanguageComboBox.SelectedValue is not SupportedLanguage lang || Flag == null)
                return;

            string code = lang.ToString(); // "fr", "en", "it"

            Flag.Source = new System.Windows.Media.Imaging.BitmapImage(
                new Uri($"pack://application:,,,/Asset/Flags/{code}.png", UriKind.Absolute));

            string tooltipText =
                AppResources.ResourceManager.GetString("ToolTipFlag", new CultureInfo(code))
                ?? "Language information";

            Flag.ToolTip = new ToolTip
            {
                Content = new TextBlock
                {
                    Text = tooltipText,
                    TextWrapping = TextWrapping.Wrap,
                    MaxWidth = 300
                }
            };
        }
    }
}