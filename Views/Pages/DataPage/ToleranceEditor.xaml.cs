using System.Windows;
using System.Windows.Controls;
using Toltech.App.Front;
using Toltech.App.Services;
using Toltech.App.ViewModels;
using static Toltech.App.Services.EventsManager;

namespace Toltech.App.Views.Pages.DataPage
{
    /// <summary>
    /// Éditeur d'une tolérance unique (nom, description, valeur).
    /// Se bind sur un ToleranceSlotViewModel (ex: N.Intermediate).
    /// </summary>
    public partial class ToleranceEditor : UserControl
    {

        private readonly MainViewModel _mainVM;
        public MainViewModel MainVM => _mainVM;
        private ToleranceSlotViewModel? VM =>
    DataContext as ToleranceSlotViewModel;

        private readonly DomainService _domainService;

        public ToleranceEditor()
        {
            _mainVM = App.MainVM; // TODO Pas super jolie
            _domainService = _mainVM.DomainService;

            InitializeComponent();
        }

        #region Properties Control
        public string PlaceholderName
        {
            get => (string)GetValue(PlaceholderNameProperty);
            set => SetValue(PlaceholderNameProperty, value);
        }

        public static readonly DependencyProperty PlaceholderNameProperty =
            DependencyProperty.Register(
                nameof(PlaceholderName),
                typeof(string),
                typeof(ToleranceEditor));

        public string PlaceholderDescription
        {
            get => (string)GetValue(PlaceholderDescriptionProperty);
            set => SetValue(PlaceholderDescriptionProperty, value);
        }

        public static readonly DependencyProperty PlaceholderDescriptionProperty =
            DependencyProperty.Register(
                nameof(PlaceholderDescription),
                typeof(string),
                typeof(ToleranceEditor));

        public string PlaceholderTolerance
        {
            get => (string)GetValue(PlaceholderToleranceProperty);
            set => SetValue(PlaceholderToleranceProperty, value);
        }

        public static readonly DependencyProperty PlaceholderToleranceProperty =
            DependencyProperty.Register(
                nameof(PlaceholderTolerance),
                typeof(string),
                typeof(ToleranceEditor));

        public bool Active
        {
            get => (bool)GetValue(ActiveProperty);
            set => SetValue(ActiveProperty, value);
        }

        public static readonly DependencyProperty ActiveProperty =
            DependencyProperty.Register(
                nameof(Active),
                typeof(bool),
                typeof(ToleranceEditor),
                new PropertyMetadata(false));

        #endregion

        /// <summary>
        /// Ouvre la fenêtre de sélection d'une tolérance issue de la base de données.
        /// </summary>
        private void SelectClosingTolerance_Click(
            object sender,
            RoutedEventArgs e)
        {
            // Récupère le ModelData associé à ce composant.
            // Cette approche suppose que le DataContext du composant
            // est directement un objet ModelData.
            if (DataContext is not ToleranceSlotViewModel toleranceSlotViewModel)
                return;

            // Crée la fenêtre de sélection existante.
            var floatingDbWindow = new SelectFloatingPanelDB(_mainVM);

            // Récupère la fenêtre propriétaire.
            var ownerWindow = Window.GetWindow(this);

            if (ownerWindow != null)
                floatingDbWindow.Owner = ownerWindow;

            // Abonne l'événement qui renvoie la tolérance sélectionnée.
            floatingDbWindow.ToleranceSelected +=
                (s, args) => OnToleranceSelected(toleranceSlotViewModel, args);

            // Affiche la fenêtre de sélection de manière modale.
            floatingDbWindow.ShowDialog();
        }

        /// <summary>
        /// Traite la tolérance sélectionnée dans la fenêtre de la base de données.
        /// </summary>
        public async Task OnToleranceSelected(
            ToleranceSlotViewModel toleranceSlotViewModel,
            ToleranceSelectedEventArgs Tolerance)
        {
            if (toleranceSlotViewModel == null)
                return;


            if (toleranceSlotViewModel.Kind == ToleranceKind.R)
            {
                var requirement = toleranceSlotViewModel.Requirements;

                if (requirement == null)
                    return;

                await VM.UpdateRequirementsToleranceAsync(
                    Tolerance);
            }
            else
            {
                // Récupère le ModelData associé au slot.
                var modelData = toleranceSlotViewModel.ModelData;

                if (modelData == null)
                    return;

                await VM.UpdateModelDataToleranceAsync(
                    Tolerance);
            }





        }

    }

}
