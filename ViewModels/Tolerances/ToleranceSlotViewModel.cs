using System.ComponentModel;
using System.Linq.Expressions;
using Toltech.App.Models;

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
    public sealed class ToleranceSlotViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private readonly INotifyPropertyChanged _source;

        private readonly IPropertyAdapter<string> _name;
        private readonly IPropertyAdapter<string> _description;
        private readonly IPropertyAdapter<double> _value;
        private readonly IPropertyAdapter<int> _id;
        private readonly IPropertyAdapter<bool> _database;

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

            ModelData = model;
            _source = model;

            Kind = kind;
            Position = position;

            _name = new PropertyAdapter<ModelData, string>(model, name);
            _description = new PropertyAdapter<ModelData, string>(model, description);
            _value = new PropertyAdapter<ModelData, double>(model, value);
            _id = new PropertyAdapter<ModelData, int>(model, id);
            _database = new PropertyAdapter<ModelData, bool>(model, database);

            SubscribeToSource();
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
        }

        #endregion

        #region Properties

        public string Name
        {
            get => _name.Value;
            set => _name.Value = value;
        }

        public string Description
        {
            get => _description.Value;
            set => _description.Value = value;
        }

        public double Value
        {
            get => _value.Value;
            set => _value.Value = value;
        }

        public int Id
        {
            get => _id.Value;
            set => _id.Value = value;
        }

        public bool UseDatabase
        {
            get => _database.Value;
            set => _database.Value = value;
        }

        #endregion

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
        private void Source_PropertyChanged(
            object? sender,
            PropertyChangedEventArgs e)
        {
            if (e.PropertyName == _name.PropertyName)
                OnPropertyChanged(nameof(Name));

            if (e.PropertyName == _description.PropertyName)
                OnPropertyChanged(nameof(Description));

            if (e.PropertyName == _value.PropertyName)
                OnPropertyChanged(nameof(Value));

            if (e.PropertyName == _id.PropertyName)
                OnPropertyChanged(nameof(Id));

            if (e.PropertyName == _database.PropertyName)
                OnPropertyChanged(nameof(UseDatabase));
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}