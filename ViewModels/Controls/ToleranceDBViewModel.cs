using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Toltech.App.Models;
using Toltech.App.Services;
using Toltech.App.Services.Notification;
using TtCore = Toltech.App.ViewModels;

namespace Toltech.App.ViewModels
{
    public class ToleranceDBViewModel : BaseViewModel
    {
        private readonly MainViewModel _mainVM;
        public MainViewModel MainVM => _mainVM;

        private readonly DomainService _domainService;

        private readonly INotificationService _notificationService;


        private ObservableCollection<ToleranceRow> _selectedTolerances
            = new ObservableCollection<ToleranceRow>();

        public ObservableCollection<ToleranceRow> SelectedTolerances
        {
            get => _selectedTolerances;
            set => SetProperty(ref _selectedTolerances, value);
        }


        public ToleranceDBViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM;
            _domainService = mainVM.DomainService;

            Tolerances = new ObservableCollection<ToleranceRow>();

            #region ICommand
            LoadCommand = new TtCore.AsyncRelayCommand(LoadTolerancesAsync);
            SaveCommand = new TtCore.AsyncRelayCommand(SaveTolerancesAsync);
            CreateCommand = new TtCore.AsyncRelayCommand(CreateToleranceAsync);
            DeleteCommand = new TtCore.AsyncRelayCommand(DeleteSelectedTolerancesAsync);
            #endregion

            _ = LoadTolerancesAsync();
        }

        // ----------------------------------------------------------------
        // Collections
        // ----------------------------------------------------------------
        public ObservableCollection<ToleranceRow> Tolerances { get; }

        // ----------------------------------------------------------------
        // Commands
        // ----------------------------------------------------------------
        public ICommand LoadCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CreateCommand { get; }
        public ICommand DeleteCommand { get; }

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
            await ExecuteBusyAsync("Chargement des tolérances...", async () =>
            {
                Tolerances.Clear();
                var tolerances = await _domainService.GetTolerancesAsync();
                if (tolerances == null || !tolerances.Any()) return;

                foreach (var tol in tolerances)
                {
                    Tolerances.Add(new ToleranceRow
                    {
                        Id = tol.Id,
                        NameTol = tol.NameTol,
                        DescriptionTol = tol.DescriptionTol,
                        Tol = tol.Tol
                    });
                }
            });
        }

        public async Task SaveTolerancesAsync()
        {
            await ExecuteBusyAsync("Sauvegarde des tolérances...", async () =>
            {
                foreach (var tol in Tolerances)
                {
                    var dbTol = new Tolerance
                    {
                        Id = tol.Id,
                        NameTol = tol.NameTol,
                        DescriptionTol = tol.DescriptionTol,
                        Tol = tol.Tol
                    };

                    if (dbTol.Id == 0)
                        await _domainService.InsertToleranceAsync(dbTol);
                    else
                        await _domainService.UpdateToleranceAsync(dbTol);
                }
            });

            MessageBox.Show("Tolérances sauvegardées avec succès.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public async Task CreateToleranceAsync()
        {
            await ExecuteBusyAsync("Création d'une tolérance...", async () =>
            {
                var newTol = new Tolerance();
                await _domainService.InsertToleranceAsync(newTol);
                Tolerances.Add(new ToleranceRow
                {
                    Id = newTol.Id,
                    NameTol = newTol.NameTol,
                    DescriptionTol = newTol.DescriptionTol,
                    Tol = newTol.Tol
                });
            });
            LoadTolerancesAsync();

        }

        public async Task DeleteToleranceAsync(ToleranceRow tol)
        {
            if (tol == null) return;

            var result = MessageBox.Show($"Confirmez-vous la suppression de {tol.NameTol} ?",
                                         "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;

            await ExecuteBusyAsync("Suppression...", async () =>
            {
                var dbTol = new Tolerance
                {
                    Id = tol.Id,
                    NameTol = tol.NameTol,
                    DescriptionTol = tol.DescriptionTol,
                    Tol = tol.Tol
                };

                await _domainService.DeleteToleranceAsync(dbTol);
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
                    var dbTol = new Tolerance
                    {
                        Id = tol.Id,
                        NameTol = tol.NameTol,
                        DescriptionTol = tol.DescriptionTol,
                        Tol = tol.Tol
                    };

                    await _domainService.DeleteToleranceAsync(dbTol);
                    Tolerances.Remove(tol);
                }

                SelectedTolerances.Clear();
            });
        }


        // ----------------------------------------------------------------
        // Helper
        // ----------------------------------------------------------------
        private async Task ExecuteBusyAsync(string message, Func<Task> action)
        {
            IsBusy = true;
            StatusMessage = message;

            try
            {
                await action();
            }
            finally
            {
                IsBusy = false;
                StatusMessage = string.Empty;
            }
        }
    }

    // DTO pour DataGrid
    public class ToleranceRow 
    {
        public int Id { get; set; }
        public string NameTol { get; set; }
        public string DescriptionTol { get; set; }
        public double Tol { get; set; }
    }
}
