using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Toltech.App.Models;
using Toltech.App.Services;
using Toltech.App.Services.CAD;
using Toltech.App.Utilities;
using Toltech.App.ViewModels;
using Toltech.Cad.Model;
using Toltech.Solver.Contracts;
using static Toltech.App.Models.NodesDefinition;
using static Toltech.App.Services.EventsManager;

namespace Toltech.App.FrontEnd.Controls
{
    /// <summary>
    /// Contrôle affichant une ligne ModelData dans la liste.
    /// </summary>
    public sealed partial class PanelData : UserControl
    {
        // Contrôleur de sélection CAO : créé au Loaded, libéré au Unloaded
        // (un contrôle WPF peut être déchargé puis réaffiché).
        private CadSelectionController? _cadSelectionController;

        public ObservableCollection<LinkageTypeItem> AvailableLinkageTypes { get; }
            = new ObservableCollection<LinkageTypeItem>();

        #region Constructeur

        public PanelData()
        {
            InitializeComponent();

            Loaded += OnLoaded;

            foreach (LinkageType type in Enum.GetValues<LinkageType>())
            {
                if (type == LinkageType.Requirement)
                    continue;

                AvailableLinkageTypes.Add(new LinkageTypeItem
                {
                    Value = type
                });
            }
        }

        #endregion

        #region Dependency properties

        public DatasViewModel ParentViewModel
        {
            get => (DatasViewModel)GetValue(ParentViewModelProperty);
            set => SetValue(ParentViewModelProperty, value);
        }

        public static readonly DependencyProperty ParentViewModelProperty =
            DependencyProperty.Register(
                nameof(ParentViewModel),
                typeof(DatasViewModel),
                typeof(PanelData),
                new PropertyMetadata(null));

        // Création d'une DependencyProperty pour faciliter le binding
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register(
                nameof(Data),
                typeof(ModelData),
                typeof(PanelData),
                new PropertyMetadata(null));

        public ModelData Data
        {
            get => (ModelData)GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }

        #endregion

        #region Cycle de vie

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // -= avant += : évite les doublons si Loaded est levé plusieurs fois.
            EventsManager.NodeChanged -= OnNodeChangedAsync;
            EventsManager.NodeChanged += OnNodeChangedAsync;

            EventsManager.PartCrud -= OnPartCrudAsync;
            EventsManager.PartCrud += OnPartCrudAsync;

            if (_cadSelectionController is null)
            {
                var cadSelectionService = ((App)Application.Current).CadSelectionService;

                _cadSelectionController = new CadSelectionController(cadSelectionService);
                _cadSelectionController.SelectionChanged += OnSelectionChanged;
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            // Un événement maintenu par un service global garde une référence
            // vers ce UserControl : on se désabonne au déchargement.
            EventsManager.NodeChanged -= OnNodeChangedAsync;
            EventsManager.PartCrud -= OnPartCrudAsync;

            if (_cadSelectionController is not null)
            {
                _cadSelectionController.SelectionChanged -= OnSelectionChanged;
                _cadSelectionController.Dispose();
                _cadSelectionController = null;
            }
        }

        #endregion

        #region Events

        private Task OnNodeChangedAsync(NodeChangedEvent e)
        {
            if (e.Type != NodeType.PartNode)
                return Task.CompletedTask;

            return OnPartCrudAsync(new PartCrudEvent
            {
                Operation = e.Operation,
                Source = EventSource.Tree,
                EntityId = e.LinkedOriginalId,
                Entity = e.Operation == CrudOperation.Updated
                    ? new Part { Id = e.LinkedOriginalId, NamePart = e.NewName }
                    : null
            });
        }

        private Task OnPartCrudAsync(PartCrudEvent e)
        {
            if (e.Operation != CrudOperation.Updated)
                return Task.CompletedTask;

            var updatedParts = e.Entities?.Any() == true
                ? e.Entities
                : e.Entity != null ? new List<Part> { e.Entity } : null;

            if (updatedParts == null)
                return Task.CompletedTask;

            if (DataContext is not ModelData data)
                return Task.CompletedTask;

            App.Current.Dispatcher.Invoke(() =>
            {
                var extremite = updatedParts.FirstOrDefault(p => p.Id == data.ExtremitePartId);
                if (extremite != null)
                    Part1Text.Text = extremite.NamePart;

                var origine = updatedParts.FirstOrDefault(p => p.Id == data.OriginePartId);
                if (origine != null)
                    Part2Text.Text = origine.NamePart;
            });

            return Task.CompletedTask;
        }

        #endregion

        #region Sélection CAO

        // Déclenché par la commande CadCommands.PickPoint (menu contextuel des NumberBox).
        private async void PickPointMenuItem_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is not TextBox)
                return;

            if (_cadSelectionController is null)
                return;

            await _cadSelectionController.StartSelectionAsync(
                Window.GetWindow(this),
                CadSelectionType.Point);
        }

        // Déclenché par la commande CadCommands.PickEdge.
        private async void PickEdgeMenuItem_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is not TextBox)
                return;

            if (_cadSelectionController is null)
                return;

            await _cadSelectionController.StartSelectionAsync(
                Window.GetWindow(this),
                CadSelectionType.Edge);
        }

        // Le contrôleur ne relaie le résultat qu'à celui qui a lancé la demande,
        private void OnSelectionChanged(object? sender, CadSelection selection)
        {
            switch (selection.Type)
            {
                case CadSelectionType.Point:
                    HandlePointSelection(selection);
                    break;

                case CadSelectionType.Direction:
                    HandleDirectionSelection(selection);
                    break;
            }
        }

        private void HandlePointSelection(CadSelection selection)
        {
            if (selection.Point is not CadPoint point)
                return;

            PointXText.Text = point.X.ToString(CultureInfo.InvariantCulture);
            PointYText.Text = point.Y.ToString(CultureInfo.InvariantCulture);
            PointZText.Text = point.Z.ToString(CultureInfo.InvariantCulture);
        }

        private void HandleDirectionSelection(CadSelection selection)
        {
            if (selection.Direction is not CadDirection direction)
                return;

            DirectionXText.Text = direction.U.ToString(CultureInfo.InvariantCulture);
            DirectionYText.Text = direction.V.ToString(CultureInfo.InvariantCulture);
            DirectionZText.Text = direction.W.ToString(CultureInfo.InvariantCulture);
        }

        #endregion
    }
}