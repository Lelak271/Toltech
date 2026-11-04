using System.Collections.ObjectModel;
using System.Globalization;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows;
using System.Windows.Controls;
using Toltech.App.Models;
using Toltech.App.Resources;
using Toltech.App.Services.CAD;
using Toltech.App.ViewModels;
using Toltech.Cad.Model;
using System.Windows.Data;
namespace Toltech.App.FrontEnd.Controls
{
    public sealed partial class PanelReqs : UserControl
    {
        private string currentContextTarget = string.Empty; // "Part1" ou "Part2"

        private CadSelectionController? _cadSelectionController;

        #region Constructeur
        public PanelReqs()
        {
            InitializeComponent();

            AppSettings.UnitsChanged -= OnUnitsChanged;
            AppSettings.UnitsChanged += OnUnitsChanged;
        }

        public RequirementsViewModel ParentViewModel
        {
            get => (RequirementsViewModel)GetValue(ParentViewModelProperty);
            set => SetValue(ParentViewModelProperty, value);
        }

        public static readonly DependencyProperty ParentViewModelProperty =
            DependencyProperty.Register(
                nameof(ParentViewModel),
                typeof(RequirementsViewModel),
                typeof(PanelReqs),
               new PropertyMetadata(null, OnParentViewModelChanged));


        // Création d'une DependencyProperty pour faciliter le binding
        public static readonly DependencyProperty RequirementProperty =
            DependencyProperty.Register(
                nameof(Requirement),
                typeof(Requirements),
                typeof(PanelReqs),
                 new PropertyMetadata(null));

        public Requirements Requirement
        {
            get => (Requirements)GetValue(RequirementProperty);
            set => SetValue(RequirementProperty, value);
        }

        #endregion

        #region ComboBox Gestion
        private static void OnParentViewModelChanged(
                        DependencyObject d,
                        DependencyPropertyChangedEventArgs e)
        {
            if (d is PanelReqs control)
                control.UpdateAvailableParts();
        }

        private bool _listsBuilt;

        private void UpdateAvailableParts()
        {
            if (ParentViewModel?.Parts == null)
                return;

            // Reconstruit les listes seulement si l'ensemble des pièces a changé.
            if (!_listsBuilt || AvailableParts1.Count != ParentViewModel.Parts.Count)
            {
                AvailableParts1.Clear();
                AvailableParts2.Clear();

                foreach (var part in ParentViewModel.Parts)
                {
                    AvailableParts1.Add(new PartChoice(part));
                    AvailableParts2.Add(new PartChoice(part));
                }
                _listsBuilt = true;
            }

            // Les sélections sont lues sur les combobox : fiable quel que soit l'UpdateSourceTrigger.
            var selected1 = Part1Text.SelectedValue as int? ?? Requirement?.PartReq1Id;
            var selected2 = Part2Text.SelectedValue as int? ?? Requirement?.PartReq2Id;

            foreach (var c in AvailableParts1) c.IsEnabled = selected2 == null || c.Id != selected2;
            foreach (var c in AvailableParts2) c.IsEnabled = selected1 == null || c.Id != selected1;
        }


        private void Part_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
                UpdateAvailableParts();
        }

        public ObservableCollection<PartChoice> AvailableParts1 { get; } = new();
        public ObservableCollection<PartChoice> AvailableParts2 { get; } = new();

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

            PointX.Text = point.X.ToString(CultureInfo.InvariantCulture);
            PointY.Text = point.Y.ToString(CultureInfo.InvariantCulture);
            PointZ.Text = point.Z.ToString(CultureInfo.InvariantCulture);
        }

        private void HandleDirectionSelection(CadSelection selection)
        {
            if (selection.Direction is not CadDirection direction)
                return;

            DirU.Text = direction.U.ToString(CultureInfo.InvariantCulture);
            DirV.Text = direction.V.ToString(CultureInfo.InvariantCulture);
            DirW.Text = direction.W.ToString(CultureInfo.InvariantCulture);
        }

        #endregion

        private void OnUnitsChanged(object? sender, UnitsChangedEventArgs e)
        {
            if (!e.LengthChanged) return;

            Dispatcher.Invoke(() =>
            {
                foreach (var box in new[] { PointX, PointY, PointZ, DirU, DirV, DirW })
                    BindingOperations.GetBindingExpression(box, Wpf.Ui.Controls.NumberBox.ValueProperty)?.UpdateTarget();
            });
        }

    }

    /// <summary>
    /// Represents a selectable part with properties for identification, name, and enabled state. Notifies listeners
    /// when property values change.
    /// </summary>
    public sealed class PartChoice : INotifyPropertyChanged
    {
        public PartChoice(Part part) => Part = part;

        public Part Part { get; }
        public int Id => Part.Id;
        public string NamePart => Part.NamePart;

        private bool _isEnabled = true;
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled == value) return;
                _isEnabled = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsEnabled)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

}
