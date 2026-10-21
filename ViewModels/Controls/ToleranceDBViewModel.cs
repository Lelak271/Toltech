using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Toltech.App.Models;
using Toltech.App.Services;
using Toltech.App.Services.Notification;
using static Toltech.App.Services.EventsManager;

namespace Toltech.App.ViewModels
{
    public class ToleranceDBViewModel : BaseViewModel
    {
        private readonly MainViewModel _mainVM;
        public MainViewModel MainVM => _mainVM;

        private readonly DomainService _domainService;

        private readonly INotificationService _notificationService;

        private ObservableCollection<Tolerance> _selectedTolerances = new ObservableCollection<Tolerance>();

        /// <summary>Lignes sélectionnées (multi-sélection, alimentée par DataGridSelectedItemsBehavior).</summary>
        public ObservableCollection<Tolerance> SelectedTolerances
        {
            get => _selectedTolerances;
            set => SetProperty(ref _selectedTolerances, value);
        }

        public ToleranceDBViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM;
            _domainService = mainVM.DomainService;

            Tolerances = new ObservableCollection<Tolerance>();

            #region ICommand
            LoadCommand = new AsyncRelayCommand(LoadTolerancesAsync);
            SaveCommand = new AsyncRelayCommand(SaveTolerancesAsync);
            CreateCommand = new AsyncRelayCommand(CreateToleranceAsync);
            DeleteCommand = new AsyncRelayCommand(DeleteSelectedTolerancesAsync);

            LinkedCommand = new RelayCommand(Select, () => IsSelectionMode && SelectedTolerance is not null);
            #endregion

            _ = LoadTolerancesAsync();
        }

        // ----------------------------------------------------------------
        // Collections
        // ----------------------------------------------------------------
        public ObservableCollection<Tolerance> Tolerances { get; }

        // ----------------------------------------------------------------
        // Commands
        // ----------------------------------------------------------------
        public ICommand LoadCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CreateCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand LinkedCommand { get; }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        // ----------------------------------------------------------------
        // Méthodes Async
        // ----------------------------------------------------------------

        public async Task LoadTolerancesAsync()
        {
            if (string.IsNullOrEmpty(ModelManager.ModelActif))
            {
                MessageBox.Show("Aucun modèle actif sélectionné.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await ExecuteBusyAsync("Chargement des tolérances...", async () =>
            {
                Tolerances.Clear();
                var tolerances = await _domainService.GetTolerancesAsync();
                if (tolerances == null || !tolerances.Any()) return;

                foreach (var tol in tolerances)
                    Tolerances.Add(tol);
            });
        }

        public async Task SaveTolerancesAsync()
        {
            bool saved = await ExecuteBusyAsync("Sauvegarde des tolérances...", async () =>
            {
                foreach (var tol in Tolerances)
                {
                    if (tol.Id == 0)
                        await _domainService.InsertToleranceAsync(tol);
                    else
                        await _domainService.UpdateToleranceAsync(tol);
                }
            });

            if (saved)
                MessageBox.Show("Tolérances sauvegardées avec succès.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public async Task CreateToleranceAsync()
        {
            bool created = await ExecuteBusyAsync("Création d'une tolérance...", async () =>
            {
                var newTol = new Tolerance();
                await _domainService.InsertToleranceAsync(newTol);
            });

            // Rechargement attendu pour récupérer l'Id attribué par la base.
            if (created)
                await LoadTolerancesAsync();
        }

        public async Task DeleteToleranceAsync(Tolerance tol)
        {
            if (tol == null) return;

            var result = MessageBox.Show($"Confirmez-vous la suppression de {tol.Name} ?",
                                         "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;

            await ExecuteBusyAsync("Suppression...", async () =>
            {
                await _domainService.DeleteToleranceAsync(tol);
                Tolerances.Remove(tol);
            });
        }

        public async Task DeleteSelectedTolerancesAsync()
        {
            if (SelectedTolerances == null || SelectedTolerances.Count == 0)
                return;

            var result = MessageBox.Show(
                $"Confirmez-vous la suppression de {SelectedTolerances.Count} tolérance(s) ?",
                "Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            await ExecuteBusyAsync("Suppression des tolérances...", async () =>
            {
                // Copie pour éviter modification pendant itération
                var toDelete = SelectedTolerances.ToList();

                foreach (var tol in toDelete)
                {
                    await _domainService.DeleteToleranceAsync(tol);
                    Tolerances.Remove(tol);
                }

                SelectedTolerances.Clear();
            });
        }

        // ----------------------------------------------------------------
        // Helper
        // ----------------------------------------------------------------

        /// <summary>
        /// Exécute l'action avec l'indicateur d'occupation.
        /// Les exceptions sont affichées au lieu de remonter dans une commande async void
        /// (ce qui ferait planter l'application). Renvoie true si l'action a réussi.
        /// </summary>
        private async Task<bool> ExecuteBusyAsync(string message, Func<Task> action)
        {
            IsBusy = true;
            StatusMessage = message;

            try
            {
                await action();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            finally
            {
                IsBusy = false;
                StatusMessage = string.Empty;
            }
        }

        #region Sélection (mode sélection de la fenêtre)

        /// <summary>
        /// true : la fenêtre sert à choisir une seule tolérance (menu "Link DB").
        /// false : visualisation / édition (boutons CRUD).
        /// </summary>
        public bool IsSelectionMode { get; init; }

        private Tolerance? _selectedTolerance;
        public Tolerance? SelectedTolerance
        {
            get => _selectedTolerance;
            set => SetProperty(ref _selectedTolerance, value);
        }

        public event EventHandler<ToleranceSelectedEventArgs>? ToleranceSelected;

        private void Select()
        {
            if (SelectedTolerance is null)
                return;

            ToleranceSelected?.Invoke(this, new ToleranceSelectedEventArgs
            {
                ToleranceID = SelectedTolerance.Id,
                ToleranceName = SelectedTolerance.Name,
                ToleranceDescription = SelectedTolerance.Description,
                ToleranceValue = SelectedTolerance.Tol.ToString()
            });
        }

        #endregion
    }
}