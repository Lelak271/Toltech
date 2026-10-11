using System.Runtime.CompilerServices;
using System.Windows.Input;
using SQLite;
using Toltech.App.ViewModels;

namespace Toltech.App.Models
{
    public partial class Tolerance : BaseViewModel
    {
        #region Backing fields

        private string _descriptionTol;
        private string _nameTol;
        private double _tol;

        #endregion

        #region Propriétés éditables (dirty-aware)

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; } // Identifiant unique de la tolérance

        public string DescriptionTol
        {
            get => _descriptionTol;
            set => SetAndDirty(ref _descriptionTol, value);
        }

        public string NameTol
        {
            get => _nameTol;
            set => SetAndDirty(ref _nameTol, value);
        }

        public double Tol
        {
            get => _tol;
            set => SetAndDirty(ref _tol, value);
        }

        #endregion

        #region Flags d’état

        private bool _isDirty;
        private bool _isSaving;
        private bool _isOutOfSync;
        private bool _isLoading;

        public bool IsDirty
        {
            get => _isDirty;
            private set
            {
                if (SetProperty(ref _isDirty, value))
                    CommandManager.InvalidateRequerySuggested();
            }
        }

        public bool IsSaving
        {
            get => _isSaving;
            private set => SetProperty(ref _isSaving, value);
        }

        public bool IsOutOfSync
        {
            get => _isOutOfSync;
            private set => SetProperty(ref _isOutOfSync, value);
        }

        #endregion

        #region Dirty helpers

        protected bool SetAndDirty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value))
                return false;

            field = value;

            OnPropertyChanged(propertyName);

            if (!_isLoading)
                MarkDirty();

            return true;
        }

        private void MarkDirty()
        {
            if (!IsDirty)
                IsDirty = true;

            IsOutOfSync = false;
        }


        // Méthodes publiques pour contrôler les flags
        public void ClearDirty() => IsDirty = false;
        public void MarkSaving() => IsSaving = true;
        public void ClearSaving() => IsSaving = false;
        public void MarkOutOfSync() => IsOutOfSync = true;

        #endregion

    }
}