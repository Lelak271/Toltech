using System.Windows;
using Toltech.App.ViewModels;

namespace Toltech.App.FrontEnd.Controls
{
    public partial class FloatingPanelDB : Window
    {
        public FloatingPanelDB(ToleranceDBViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
