using System.Collections.ObjectModel;
using System.Windows.Data;
using Toltech.App.Models;
using Toltech.App.Services;
using Toltech.App.Services.Logging;
using System.Windows;

namespace Toltech.App.ViewModels
{
    public class ResultsViewModel : BaseViewModel
    {
        #region Fields
        private readonly MainViewModel _mainVM;
        public MainViewModel MainVM => _mainVM;

        private readonly RequirementsViewModel _requirementsVM;

        private readonly ILoggerService _logger;

        #endregion

        #region Collections

        public ObservableCollection<Requirements> Requirements => _requirementsVM.Requirements;

        public ListCollectionView AllRequirements => _requirementsVM.AllRequirements;


        private string? _filePathResx;
        public string? FilePathResx
        {
            get => _filePathResx;
            private set
            {
                if (_filePathResx == value)
                    return;

                _filePathResx = value;
                OnPropertyChanged(nameof(FilePathResx));
            }
        }


        #endregion

        public ResultsViewModel(MainViewModel mainVM, RequirementsViewModel requirementsVM)
        {
            _mainVM = mainVM;
            _requirementsVM = requirementsVM;

        }

        public void SetFileResx(string filePathResx)
        {
            ModelManager.FilePathResx = filePathResx; 
            FilePathResx = filePathResx;

        }
    }
}
