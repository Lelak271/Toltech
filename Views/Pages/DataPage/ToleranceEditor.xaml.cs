using System.Windows;
using System.Windows.Controls;
using Toltech.App.Front;
using Toltech.App.Models;
using Toltech.App.ViewModels;

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

        public ToleranceEditor()
        {
            _mainVM = App.MainVM; 

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
        private void OnToleranceSelected(
            ToleranceSlotViewModel toleranceSlotViewModel,
            SelectFloatingPanelDB.ToleranceSelectedEventArgs args)
        {
            if (toleranceSlotViewModel == null)
                return;

            // Récupère le ModelData associé au slot.
            var modelData = toleranceSlotViewModel.ModelData;

            if (modelData == null) 
                return;

            // TODO 
            // Mettre en place la logique pour mettre a jour le bon Id tolerance 
            // Mettre la logique dans Domainservice

            // Affecte l'identifiant de la tolérance sélectionnée en base.
            modelData.Id = args.ToleranceID;
        }
    }

}
