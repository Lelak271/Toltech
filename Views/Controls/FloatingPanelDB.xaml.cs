using System.Windows;
using System.Windows.Input;
using Toltech.App.Services;
using Toltech.App.ViewModels;

namespace Toltech.App.FrontEnd.Controls
{
    /// <summary>
    /// Fenêtre des tolérances de la base de données.
    /// Deux modes selon <see cref="TolerancesViewModel.IsSelectionMode"/> :
    /// - false : visualisation / édition (boutons CRUD),
    /// - true  : sélection d'une seule valeur (menu contextuel "Link DB").
    /// </summary>
    public partial class TolerancePanelWindow : Wpf.Ui.Controls.FluentWindow
    {
        protected TolerancesViewModel ViewModel { get; }

        public TolerancePanelWindow(TolerancesViewModel viewModel)
        {
            DataContext = viewModel;

            InitializeComponent();

            ViewModel = viewModel;
        }

        private void Tolerances_MouseDoubleClick(object sender,MouseButtonEventArgs e)
        {
            if (DataContext is not TolerancesViewModel vm)
                return;

            if (vm.LinkedCommand.CanExecute(null))
            {
                vm.LinkedCommand.Execute(null);
            }
        }

    }



}
