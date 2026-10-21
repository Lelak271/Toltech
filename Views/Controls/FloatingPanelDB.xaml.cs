using System.Windows;
using System.Windows.Input;
using Toltech.App.ViewModels;

namespace Toltech.App.FrontEnd.Controls
{
    /// <summary>
    /// Fenêtre des tolérances de la base de données.
    /// Deux modes selon <see cref="ToleranceDBViewModel.IsSelectionMode"/> :
    /// - false : visualisation / édition (boutons CRUD),
    /// - true  : sélection d'une seule valeur (menu contextuel "Link DB").
    /// </summary>
    public partial class FloatingPanelDB : Window
    {
        protected ToleranceDBViewModel ViewModel { get; }

        public FloatingPanelDB(ToleranceDBViewModel viewModel)
        {
            ArgumentNullException.ThrowIfNull(viewModel);

            InitializeComponent();

            ViewModel = viewModel;
            DataContext = viewModel;
        }

        private void Tolerances_MouseDoubleClick(
    object sender,
    MouseButtonEventArgs e)
        {
            if (DataContext is not ToleranceDBViewModel vm)
                return;

            if (vm.LinkedCommand.CanExecute(null))
            {
                vm.LinkedCommand.Execute(null);
            }
        }

    }



}
