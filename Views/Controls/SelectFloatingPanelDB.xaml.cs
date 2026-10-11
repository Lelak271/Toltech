using System.Data;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Toltech.App.Models;
using Toltech.App.Services;
using Toltech.App.Services.Notification;
using Toltech.App.ViewModels;

namespace Toltech.App.Front
{
    public partial class SelectFloatingPanelDB : Window
    {
        private readonly MainViewModel _mainVM;
        public MainViewModel MainVM => _mainVM;

        private readonly INotificationService _notificationService;
        private readonly DomainService _domainService;

        public event EventHandler<ToleranceSelectedEventArgs> ToleranceSelected;
        private string currentContextTarget = string.Empty; // "Part1" ou "Part2"

        public SelectFloatingPanelDB(MainViewModel mainVM)
        {
            _mainVM = mainVM;
            _domainService = mainVM.DomainService;

            InitializeComponent();
            InitializeDataGridColumns();
            LoadTolerancesFromActiveModel();
        }


        public void UpdateUI()
        {
            InitializeDataGridColumns();
            LoadTolerancesFromActiveModel();
        }

        // Classe adaptée au tri et aux colonnes déclarées dynamiquement
        public class ToleranceRow
        {
            public int Id { get; set; }
            public string DescriptionTol { get; set; }
            public string NameTol { get; set; }
            public double tol { get; set; }
        }

        private void InitializeDataGridColumns()
        {
            ToleranceGrid.AutoGenerateColumns = false;
            ToleranceGrid.Columns.Clear();

            AddTextColumn("ID", "Id", 50);
            AddTextColumn("NameTolInt", "NameTolInt");
            AddTextColumn("DescriptionTolInt", "DescriptionTolInt");
            AddTextColumn("tolInt", "tolInt");
        }

        private void AddTextColumn(string header, string bindingPath, double width = double.NaN)
        {
            var column = new DataGridTextColumn
            {
                Header = header,
                Binding = new Binding(bindingPath),
                Width = double.IsNaN(width) ? DataGridLength.Auto : width
            };
            ToleranceGrid.Columns.Add(column);
        }

        private async Task LoadTolerancesFromActiveModel()
        {
            try
            {
                Debug.WriteLine($"[Tolérance] Chargement des données depuis le modèle actif : {ModelManager.ModelActif}");

                if (string.IsNullOrEmpty(ModelManager.ModelActif))
                {
                    MessageBox.Show("Aucun modèle actif sélectionné.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                var tolerances = await _domainService.GetTolerancesAsync();

                if (tolerances == null || !tolerances.Any())
                {
                    Debug.WriteLine("Aucune tolérance trouvée dans la base.");
                    ToleranceGrid.ItemsSource = null;
                    return;
                }

                var rows = tolerances.Select(tol => new ToleranceRow
                {
                    Id = tol.Id,
                    DescriptionTol = tol.DescriptionTol,
                    NameTol = tol.NameTol,
                    tol = tol.Tol,
                }).ToList();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    ToleranceGrid.Items.Clear();
                    ToleranceGrid.ItemsSource = rows;
                });

                Debug.WriteLine($"[Tolérance] {rows.Count} lignes chargées.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des tolérances : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task SaveTolerancesAsync()
        {
            try
            {
                foreach (var item in ToleranceGrid.Items)
                {
                    if (item is ToleranceRow tol)
                    {
                        var toleranceToSave = new Tolerance
                        {
                            Id = tol.Id,
                            DescriptionTol = tol.DescriptionTol,
                            NameTol = tol.NameTol.ToString(),
                            Tol = tol.tol,
                        };

                        if (toleranceToSave.Id == 0)
                        {
                            // Création d'une nouvelle tolérance
                            await _domainService.InsertToleranceAsync(toleranceToSave);
                            UpdateUI();
                        }
                        else
                        {
                            // Mise à jour d'une tolérance existante
                            await _domainService.UpdateToleranceAsync(toleranceToSave);
                        }
                    }
                }

                MessageBox.Show("Tolérances sauvegardées avec succès.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la sauvegarde : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            await SaveTolerancesAsync();
        }


        #region Clique-droit SELECT

        public class ToleranceSelectedEventArgs : EventArgs
        {
            public string ToleranceName { get; set; }
            public string ToleranceDescription { get; set; }
            public string ToleranceValue { get; set; }
            public int ToleranceID { get; set; }
        }


        // Méthode à appeler quand une tolérance est choisie dans votre UI
        private void OnToleranceChosen(int id, string name, string description, string tol)
        {
            ToleranceSelected?.Invoke(this, new ToleranceSelectedEventArgs
            {
                ToleranceID = id,
                ToleranceName = name,
                ToleranceDescription = description,
                ToleranceValue = tol
            });
            this.Close();
        }

        // Exemple : bouton ou double clic sur une tolérance dans la liste
        private void SelectToleranceButton_Click(object sender, RoutedEventArgs e)
        {
            // Vérifie qu'une ligne est bien sélectionnée
            if (ToleranceGrid.SelectedItem is ToleranceRow selectedRow)
            {
                int selectedId = selectedRow.Id;
                string selectedName = selectedRow.NameTol;
                string selectedDescription = selectedRow.DescriptionTol;
                string selectedTol = selectedRow.tol.ToString(); // Conversion double -> string

                OnToleranceChosen(selectedId, selectedName, selectedDescription, selectedTol);
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner une ligne de tolérance avant de valider.", "Aucune sélection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        #endregion


    }
}
