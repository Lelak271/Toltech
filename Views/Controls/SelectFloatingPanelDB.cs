using Toltech.App.FrontEnd.Controls;
using Toltech.App.ViewModels;
using static Toltech.App.Services.EventsManager;

namespace Toltech.App.Front
{
    /// <summary>
    /// Fenêtre de sélection d'une tolérance de la base de données.
    /// Surcouche de <see cref="FloatingPanelDB"/> en mode sélection : aucun XAML propre,
    /// l'API utilisée par les appelants (constructeur, Owner, ToleranceSelected, ShowDialog) est conservée.
    /// </summary>
    public sealed class SelectFloatingPanelDB : FloatingPanelDB
    {
        public MainViewModel MainVM { get; }

        /// <summary>Levé quand l'utilisateur lie une tolérance ; la fenêtre se ferme ensuite.</summary>
        public event EventHandler<ToleranceSelectedEventArgs>? ToleranceSelected;

        public SelectFloatingPanelDB(MainViewModel mainVM)
            : base(CreateViewModel(mainVM))
        {
            MainVM = mainVM;

            Title = "Sélection d'une tolérance";

            ViewModel.ToleranceSelected += OnViewModelToleranceSelected;
            Closed += (_, _) => ViewModel.ToleranceSelected -= OnViewModelToleranceSelected;
        }

        private static ToleranceDBViewModel CreateViewModel(MainViewModel mainVM)
        {
            ArgumentNullException.ThrowIfNull(mainVM);

            return new ToleranceDBViewModel(mainVM)
            {
                IsSelectionMode = true
            };
        }

        private void OnViewModelToleranceSelected(object? sender, ToleranceSelectedEventArgs e)
        {
            ToleranceSelected?.Invoke(this, e);
            Close();
        }
    }
}
