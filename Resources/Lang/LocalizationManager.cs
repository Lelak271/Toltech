using System.ComponentModel;
using System.Globalization;
using System.Resources;

namespace Toltech.App.Resources.Lang
{
    /// <summary>
    /// Provides localization management and culture switching for the application.
    /// </summary>
    /// <remarks>Supports retrieving localized strings and notifies property changes when the culture is updated.
    /// Implements the singleton pattern for global access.</remarks>
    public class LocalizationManager : INotifyPropertyChanged
    {
        private static readonly LocalizationManager _instance = new();
        public static LocalizationManager Instance => _instance;

        private readonly ResourceManager _resourceManager = AppResources.ResourceManager;

        public CultureInfo CurrentCulture { get; private set; }


        public event PropertyChangedEventHandler? PropertyChanged;

        public string this[string key] => _resourceManager.GetString(key, CurrentCulture) ?? key;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cultureCode"></param>
        public void ChangeCulture(string cultureCode)
        {
            var culture = new CultureInfo(cultureCode);

            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;

            CurrentCulture = culture;

            // Force refresh bindings
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }
    }
}
