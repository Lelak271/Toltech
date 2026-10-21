using System.ComponentModel;
using System.Linq.Expressions;
using Toltech.App.Models;
using Toltech.App.Services;
using static Toltech.App.Services.EventsManager;

namespace Toltech.App.ViewModels
{
    /// <summary>
    /// Représente une tolérance à une position donnée
    /// (Origin, Intermediate ou Extremity).
    ///
    /// Peut être associé à ModelData ou Requirements.
    /// Expose les propriétés de tolérance via des PropertyAdapter
    /// et relaie les notifications vers les bindings WPF.
    /// </summary>
    public sealed class ToleranceSlotViewModel : BaseViewModel
    {
        private DomainService _domainService;

        /// <summary>
        /// Tolérance actuellement récupérée depuis la base de données.
        /// Null si aucune tolérance n'est chargée.
        /// </summary>
        private Tolerance? _databaseTolerance;

        /// <summary>
        /// Empêche plusieurs chargements concurrents pour un même slot.
        /// </summary>
        private CancellationTokenSource? _databaseLoadCancellation;


        public event PropertyChangedEventHandler? PropertyChanged;

        private readonly INotifyPropertyChanged _source;
       
        private readonly IPropertyAdapter<bool> _database;
        private readonly IPropertyAdapter<string> _name;
        private readonly IPropertyAdapter<string> _description;
        private readonly IPropertyAdapter<double> _value;
        private readonly IPropertyAdapter<int> _id;

        public ToleranceKind Kind { get; }

        public TolerancePosition Position { get; }

        public string DisplayName => $"{Kind}.{Position}";

        /// <summary>
        /// Modèle associé, si le slot utilise ModelData.
        /// </summary>
        public ModelData? ModelData { get; }

        /// <summary>
        /// Exigences associées, si le slot utilise Requirements.
        /// </summary>
        public Requirements? Requirements { get; }

        #region Constructeur ModelData

        public ToleranceSlotViewModel(
            ModelData model,
            ToleranceKind kind,
            TolerancePosition position,
            Expression<Func<ModelData, string>> name,
            Expression<Func<ModelData, string>> description,
            Expression<Func<ModelData, double>> value,
            Expression<Func<ModelData, int>> id,
            Expression<Func<ModelData, bool>> database)
        {
            ArgumentNullException.ThrowIfNull(model);

            _domainService = App.MainVM.DomainService; // TODO

            ModelData = model;
            _source = model;

            Kind = kind;
            Position = position;

            _database = new PropertyAdapter<ModelData, bool>(model, database);
            _name = new PropertyAdapter<ModelData, string>(model, name);
            _description = new PropertyAdapter<ModelData, string>(model, description);
            _value = new PropertyAdapter<ModelData, double>(model, value);
            _id = new PropertyAdapter<ModelData, int>(model, id);

            SubscribeToSource();
           
            // Si une référence existe déjà, la tolérance sera chargée.
            if (UseDatabase && Id > 0)
            {
                _ = LoadDatabaseToleranceAsync(Id);
            }
        }

        #endregion

        #region Constructeur Requirements

        public ToleranceSlotViewModel(
            Requirements requirements,
            ToleranceKind kind,
            TolerancePosition position,
            Expression<Func<Requirements, string>> name,
            Expression<Func<Requirements, string>> description,
            Expression<Func<Requirements, double>> value,
            Expression<Func<Requirements, int>> id,
            Expression<Func<Requirements, bool>> database)
        {
            ArgumentNullException.ThrowIfNull(requirements);

            _domainService = App.MainVM.DomainService; // TODO

            Requirements = requirements;
            _source = requirements;

            Kind = kind;
            Position = position;

            _name = new PropertyAdapter<Requirements, string>(requirements, name);
            _description = new PropertyAdapter<Requirements, string>(requirements, description);
            _value = new PropertyAdapter<Requirements, double>(requirements, value);
            _id = new PropertyAdapter<Requirements, int>(requirements, id);
            _database = new PropertyAdapter<Requirements, bool>(requirements, database);

            SubscribeToSource();

            // Si une référence existe déjà, la tolérance sera chargée.
            if (UseDatabase && Id > 0)
            {
                _ = LoadDatabaseToleranceAsync(Id);
            }

        }

        #endregion

        #region Properties

        /// <summary>
        /// Nom de la tolérance affichée.
        /// La source dépend de UseDatabase.
        /// </summary>
        public string Name
        {
            get => UseDatabase
                ? _databaseTolerance?.Name ?? string.Empty
                : _name.Value;

            set
            {
                // Une tolérance liée à la base n'est pas modifiée
                // directement par le binding de l'éditeur.
                if (UseDatabase)
                    return;

                _name.Value = value;
            }
        }

        /// <summary>
        /// Description de la tolérance affichée.
        /// </summary>
        public string Description
        {
            get => UseDatabase
                ? _databaseTolerance?.Description ?? string.Empty
                : _description.Value;

            set
            {
                if (UseDatabase)
                    return;

                _description.Value = value;
            }
        }

        /// <summary>
        /// Valeur de la tolérance affichée.
        /// </summary>
        public double Value
        {
            get => UseDatabase
                ? _databaseTolerance?.Tol ?? 0.0
                : _value.Value;

            set
            {
                if (UseDatabase)
                    return;

                _value.Value = value;
            }
        }

        /// <summary>
        /// Identifiant de la tolérance associée.
        /// </summary>
        public int Id
        {
            get => _id.Value;

            set
            {
                if (_id.Value == value)
                    return;

                _id.Value = value;
                OnPropertyChanged(nameof(Id));

                RefreshDatabaseTolerance();
            }
        }

        /// <summary>
        /// Recharge la tolérance depuis la base de données si nécessaire.
        /// </summary>
        private async void RefreshDatabaseTolerance()
        {
            if (!UseDatabase || Id <= 0)
            {
                _databaseTolerance = null;
                NotifyTolerancePropertiesChanged();
                return;
            }

            await LoadDatabaseToleranceAsync(Id);
        }

        /// <summary>
        /// Indique si les valeurs proviennent de la base de données.
        /// </summary>
        public bool UseDatabase
        {
            get => _database.Value;

            set
            {
                if (_database.Value == value)
                    return;

                _database.Value = value;

                OnPropertyChanged(nameof(UseDatabase));

                if (value)
                {
                    RefreshDatabaseTolerance();
                }
                else
                {
                    CancelDatabaseToleranceLoad();

                    _databaseTolerance = null;

                    NotifyTolerancePropertiesChanged();
                }
            }
        }

        #endregion

        /// <summary>
        /// Charge une tolérance depuis la base de données.
        /// </summary>
        public async Task LoadDatabaseToleranceAsync(int toleranceId)
        {
            // Si aucune tolérance n'est associée, il n'y a rien à charger.
            if (toleranceId <= 0 || !UseDatabase)
            {
                _databaseTolerance = null;

                NotifyTolerancePropertiesChanged();

                return;
            }

            // Annule le chargement précédent pour ce slot.
            CancelDatabaseToleranceLoad();

            var cancellation = new CancellationTokenSource();

            _databaseLoadCancellation = cancellation;

            try
            {
                var tolerance = await _domainService.GetToleranceByIdAsync(
                    toleranceId,
                    cancellation.Token);

                // Une requête plus récente a pu remplacer celle-ci.
                if (cancellation.IsCancellationRequested)
                    return;

                // Vérifie que la référence n'a pas changé pendant le chargement.
                if (!UseDatabase || Id != toleranceId)
                    return;

                // Conserve la tolérance chargée.
                _databaseTolerance = tolerance;

                // Actualise les bindings WPF.
                NotifyTolerancePropertiesChanged();
            }
            catch (OperationCanceledException)
            {
                // L'annulation est normale lorsqu'une nouvelle tolérance
                // est sélectionnée ou que UseDatabase est désactivé.
            }
            catch (Exception ex)
            {
                // Évite de conserver une ancienne tolérance si le chargement échoue.
                _databaseTolerance = null;

                NotifyTolerancePropertiesChanged();

            }
            finally
            {
                // Ne libère que la ressource correspondant à ce chargement.
                if (ReferenceEquals(_databaseLoadCancellation, cancellation))
                {
                    _databaseLoadCancellation = null;
                }

                cancellation.Dispose();
            }
        }


        /// <summary>
        /// Cœur commun : affecte la tolérance de la base à ce slot,
        /// persiste la modification puis recharge la tolérance.
        /// </summary>
        private async Task AssignDatabaseToleranceAsync(
            int toleranceId,
            Func<Task> persistAsync)
        {
            if (toleranceId <= 0)
                return;

            // Écriture via les adapters : la bonne propriété est ciblée
            // automatiquement, quelle que soit la source.
            _id.Value = toleranceId;
            _database.Value = true;

            OnPropertyChanged(nameof(Id));
            OnPropertyChanged(nameof(UseDatabase));

            // Persistance propre à chaque type de source.
            await persistAsync();

            await LoadDatabaseToleranceAsync(toleranceId);
        }

        /// <summary>
        /// Affecte une tolérance de la base au ModelData de ce slot.
        /// </summary>
        public Task UpdateModelDataToleranceAsync(ToleranceSelectedEventArgs tolerance)
        {
            if (ModelData is null || tolerance is null)
                return Task.CompletedTask;

            return AssignDatabaseToleranceAsync(
                tolerance.ToleranceID,
                () => _domainService.UpdateModelDataAsync(new List<ModelData> { ModelData }));
        }

        /// <summary>
        /// Affecte une tolérance de la base aux Requirements de ce slot.
        /// </summary>
        public Task UpdateRequirementsToleranceAsync(ToleranceSelectedEventArgs tolerance)
        {
            if (Requirements is null || tolerance is null)
                return Task.CompletedTask;

            return AssignDatabaseToleranceAsync(
                tolerance.ToleranceID,
                () => _domainService.SaveRequirementsAsync(new List<Requirements> { Requirements }));
        }


        /// <summary>
        /// Annule le chargement en cours pour ce slot.
        /// </summary>
        private void CancelDatabaseToleranceLoad()
        {
            _databaseLoadCancellation?.Cancel();
        }

        /// <summary>
        /// Notifie tous les bindings qui dépendent de la tolérance.
        /// </summary>
        public void NotifyTolerancePropertiesChanged()
        {
            OnPropertyChanged(nameof(UseDatabase));
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(Description));
            OnPropertyChanged(nameof(Value));
            OnPropertyChanged(nameof(Id));
        }

        #region Notifications

        /// <summary>
        /// Abonne la ViewModel aux modifications de sa source.
        /// </summary>
        private void SubscribeToSource()
        {
            _source.PropertyChanged += Source_PropertyChanged;
        }

        /// <summary>
        /// Relaye les notifications de la source vers les propriétés
        /// exposées par cette ViewModel.
        /// </summary>
        private void Source_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == _database.PropertyName)
            {
                OnPropertyChanged(nameof(UseDatabase));
                RefreshDatabaseTolerance();
            }

            if (e.PropertyName == _id.PropertyName)
            {
                OnPropertyChanged(nameof(Id));
                if (UseDatabase)
                    RefreshDatabaseTolerance();
            }

            if (e.PropertyName == _name.PropertyName)
                OnPropertyChanged(nameof(Name));
            if (e.PropertyName == _description.PropertyName)
                OnPropertyChanged(nameof(Description));
            if (e.PropertyName == _value.PropertyName)
                OnPropertyChanged(nameof(Value));
        }



        #endregion
    }
}