using System.Diagnostics;
using System.Windows;
using System.Windows.Controls.Primitives;
using Toltech.App.FrontEnd.Controls;
using Toltech.App.FrontEnd.GraphePage.Controls;
using Toltech.App.ToltechCalculation.Helpers;
using Toltech.App.Utilities;
using Toltech.App.ViewModels;

namespace Toltech.App.Views
{
    public partial class MainWindow : Wpf.Ui.Controls.FluentWindow
    {
        private readonly MainViewModel _mainVM;

        private MainGraphe? _maingraphe;
        private readonly ComputeValidationService _computeValidationService;

        #region Constructeur
        public MainWindow(MainViewModel mainVM)
        {
            InitializeComponent();

            _mainVM = mainVM;
            DataContext = _mainVM;

            _computeValidationService = _mainVM.ComputeValidationService;


            RestoreWindowPosition();
            //ThemeToggle.IsChecked = App.Settings.Theme == AppTheme.Dark;
        }

        #endregion

        #region Interface UI "Calculs"

        // Vérification de l'isostatisme de toutes les parts
        private async void CheckInverseButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ModelValidationHelper.CheckModelActif(true))
                return;

            Debug.WriteLine("[MainWindow] - CheckInverseButton_Click");
            await _computeValidationService.CheckIsoForAllPartAsync();
        }

        private async void ValidateGraphDataButton_Click(object sender, RoutedEventArgs e)
        {
            // Vérification du graphe avant d'afficher la matrice 
            // null paramètre pour verifier toutes les requirments de la DB
            bool isGraphValid = await _computeValidationService.ValidateGraphDataAsync(true, null);
        }

        private void GenerateGraph_Click(object sender, RoutedEventArgs e)
        {
            _maingraphe = new MainGraphe(_mainVM);
            _maingraphe.Show();
        }

        #endregion

        #region Database

        private void OpenFloatingPanelDB_Click(object sender, RoutedEventArgs e)
        {
            // Récupère le ViewModel principal.
            if (DataContext is not MainViewModel mainVM)
                return;

            // Récupère le ViewModel de la base de données.
            var dbViewModel = mainVM.ToleranceVM;

            // Crée la fenêtre en lui transmettant le ViewModel existant.
            var floatingWindow = new FloatingPanelDB(dbViewModel)
            {
                Owner = this
            };

            // Affiche la fenêtre sans mode modal.
            floatingWindow.Show();
        }

        private void OpenFloatingPartDB_Click(object sender, RoutedEventArgs e)
        {
            if (!ModelValidationHelper.CheckModelActif(true))
                return;
            var floatingWindow = new FloatingPanelPartDB(_mainVM);
            floatingWindow.Owner = this; // Associe à la fenêtre principale
            floatingWindow.Show(); // Non modal
        }

        #endregion

        #region Paramètre
        private void OpenFloatingParametre_Click(object sender, RoutedEventArgs e)
        {
            var floatingWindow = new SettingsPage();
            floatingWindow.Owner = this; // Associe à la fenêtre principale
            floatingWindow.Show(); // Non modal

        }
        #endregion

        #region Main Ribbon Custom

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            // Enregistre la géométrie uniquement lorsque la fenêtre
            // est dans son état normal.
            if (WindowState == WindowState.Normal)
            {
                Properties.Settings.Default.WindowLeft = Left;
                Properties.Settings.Default.WindowTop = Top;
                Properties.Settings.Default.WindowWidth = Width;
                Properties.Settings.Default.WindowHeight = Height;
            }

            // Conserve séparément l'état maximisé.
            Properties.Settings.Default.WindowMaximized =
                WindowState == WindowState.Maximized;

            // Persiste les paramètres utilisateur.
            Properties.Settings.Default.Save();

            base.OnClosing(e);
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            _mainVM.Logger.LogDebug("Main window minimized.");
        }

        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            _mainVM.Logger.LogDebug("Main window maximized.");
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            _mainVM.Logger.LogDebug("Main window closed.");
        }

        private void ThemeChanged_Click(object sender, RoutedEventArgs e)
        {
            _mainVM.ThemeChanged();
        }

        private void RestoreWindowPosition()
        {
            var settings = Properties.Settings.Default;

            // Restaure la position et les dimensions enregistrées.
            Left = settings.WindowLeft;
            Top = settings.WindowTop;
            Width = settings.WindowWidth;
            Height = settings.WindowHeight;

            // Restaure l'état de la fenêtre.
            WindowState = settings.WindowMaximized
                ? WindowState.Maximized
                : WindowState.Normal;
        }


        #endregion

        private const double ResizeThreshold = 2.0; // seuil en pixels
        private void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            // Ignore les petits mouvements
            if (Math.Abs(e.VerticalChange) < ResizeThreshold)
                return;

            // Calcule la nouvelle hauteur
            double newHeight = LoggerGrid.ActualHeight - (e.VerticalChange * 0.95);

            // Hauteur minimale
            if (newHeight < 40)
                newHeight = 40;

            // Limite max = 60% de la fenêtre
            var window = Window.GetWindow(this);
            if (window != null)
            {
                double maxHeight = window.ActualHeight * 0.6;
                if (newHeight > maxHeight)
                    newHeight = maxHeight;
            }

            // **Met à jour la propriété PanelHeight dans la VM**
            if (DataContext is LogViewerViewModel vm)
            {
                _mainVM.StatusBarVM.PanelHeight = newHeight;            // applique la nouvelle hauteur
                _mainVM.StatusBarVM._panelHeightExpanded = newHeight;  // mémorise comme hauteur "expanded"
            }

            // Appliquer au Grid pour le rendu immédiat
            LoggerGrid.Height = newHeight;
        }

        public void ReduceLoggerGrid()
        {
            LoggerGrid.Height = 40;
        }

    }

}
