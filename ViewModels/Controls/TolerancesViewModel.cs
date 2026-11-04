using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using Toltech.App.Models;
using Toltech.App.Services;
using Toltech.App.Services.Notification;
using static Toltech.App.Services.EventsManager;
namespace Toltech.App.ViewModels
{
    public class TolerancesViewModel : BaseViewModel
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

        public TolerancesViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM;
            _domainService = mainVM.DomainService;
            _notificationService = App.NotificationService;

            Tolerances = new ObservableCollection<Tolerance>();

            #region ICommand
            LoadCommand = new AsyncRelayCommand(LoadAsync);
            SaveCommand = new AsyncRelayCommand(SaveTolerancesAsync);
            CreateCommand = new AsyncRelayCommand(CreateToleranceAsync);
            DeleteCommand = new AsyncRelayCommand(DeleteSelectedTolerancesAsync);

            LinkedCommand = new RelayCommand(Select, () => IsSelectionMode && SelectedTolerance is not null);
            #endregion

            #region Events

            // Gestion centrale du Chargement des Parts du modèle actif
            EventsManager.ModelOpened += OnOpenModel;
            #endregion
        }

        /// <summary>
        /// Gets the collection of tolerances.
        /// </summary>
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

        public async Task LoadAsync()
        {
            if (string.IsNullOrEmpty(ModelManager.ModelActif))
            {
                _dialog.Info("Aucun modèle actif sélectionné.");
                return;
            }

            await ExecuteBusyAsync("Chargement des tolérances...", async () =>
            {
                Tolerances.Clear();
                var tolerances = await _domainService.GetTolerancesAsync();
                if (tolerances == null || !tolerances.Any()) return;

                foreach (var dbTol in tolerances)
                {
                    // Create a new Tolerance instance.
                    var tol = new Tolerance();

                    // Load database values and initialize IsDirty.
                    tol.LoadFromDb(dbTol);

                    Tolerances.Add(tol);

                }
            });
        }

        private async Task OnOpenModel(ModelOpenedEvent e)
        {
            try
            {
                await LoadAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PartVM] Erreur lors du rechargement : {ex}");
            }
        }

        public async Task SaveTolerancesAsync()
        {
            var toSave = Tolerances.Where(p => p.IsDirty).ToList();
            if (toSave.Count == 0)
                return;

            bool saved = await ExecuteBusyAsync("Sauvegarde des tolérances...", async () =>
            {
                await _domainService.UpdateTolerancesAsync(toSave);
            });

            if (saved)
                _ = _notificationService.ShowNotifAsync("Tolérances sauvegardées avec succès.");
        }

        public async Task CreateToleranceAsync()
        {
            bool created = await ExecuteBusyAsync("Création d'une tolérance...", async () =>
            {
                string nameTol = Loc("NewTolerance");
                var newTol = new Tolerance
                { 
                 Name= nameTol,
                };
                await _domainService.InsertToleranceAsync(newTol);
                Tolerances.Add(newTol);
                _ = _notificationService.ShowNotifAsync($"Tolérances ajoutée {newTol.Name}.");

            });

        }

        public async Task DeleteToleranceAsync(Tolerance tol)
        {
            if (tol == null) return;

            var result = _dialog.Confirm($"Confirmez-vous la suppression de {tol.Name} ?",
                                         "Confirmation");
            if (result != true) return;

            await ExecuteBusyAsync("Suppression...", async () =>
            {
                await _domainService.DeleteToleranceAsync(tol);
                Tolerances.Remove(tol);
            });
        }

        /// <summary>
        /// Deletes the selected tolerances after user confirmation.
        /// </summary>
        /// <remarks>If no tolerances are selected, the operation is skipped.</remarks>
        /// <returns>A task that represents the asynchronous delete operation.</returns>
        public async Task DeleteSelectedTolerancesAsync()
        {
            if (SelectedTolerances == null || SelectedTolerances.Count == 0)
                return;

            var result = _dialog.Confirm(
                $"Confirmez-vous la suppression de {SelectedTolerances.Count} tolérance(s) ?");

            if (result != true)
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
                _dialog.Error($"Erreur : {ex.Message}");
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