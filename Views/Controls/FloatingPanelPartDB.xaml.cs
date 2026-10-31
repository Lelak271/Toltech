using System.Windows;
using System.Windows.Controls;
using Toltech.App.ViewModels;

namespace Toltech.App.FrontEnd.Controls
{
    public partial class FloatingPanelPartDB : Wpf.Ui.Controls.FluentWindow
    {
        public FloatingPanelPartDB(MainViewModel mainVM)
        {
            DataContext = mainVM.PartVM;

            InitializeComponent();
        }

        private void PartsGrid_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var grid = (DataGrid)sender;

            // Y a-t-il une ligne sous le curseur ?
            var row = e.OriginalSource is DependencyObject source
                ? ItemsControl.ContainerFromElement(grid, source) as DataGridRow
                : null;

            grid.ContextMenu = (ContextMenu)FindResource(row != null ? "RowMenu" : "EmptyMenu");
        }

        private async void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                if (DataContext is PartDBViewModel vm && vm.LoadCommand.CanExecute(null))
                {
                    // On exécute la commande synchronously ou async selon ton implémentation
                    //vm.LoadCommand.Execute(null);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors du LoadAsync à la fermeture : {ex}");
            }
        }


    }
}
