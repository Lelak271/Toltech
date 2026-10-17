using System.Windows;
using System.Globalization;
using System.Windows.Controls;
using Toltech.App.Models;
using Toltech.App.ViewModels;
using Toltech.Cad.Model;
using Toltech.App.Services.CAD;
using System.Windows.Input;

namespace Toltech.App.FrontEnd.Controls
{
    public partial class PanelReqs : UserControl
    {
        private string currentContextTarget = string.Empty; // "Part1" ou "Part2"

        private CadSelectionController? _cadSelectionController;

        #region Constructeur
        public PanelReqs()
        {
            InitializeComponent();
            //Debug.WriteLine("PANEL CREATED -> " + Guid.NewGuid());


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
                new PropertyMetadata(null));


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

    }
}
