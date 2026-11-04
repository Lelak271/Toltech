using System.IO;
using System.Windows;
using Microsoft.Win32;
using Toltech.App.Properties;
using Toltech.App.Resources;
using Toltech.App.Services.CAD;
using Toltech.App.Services.Dialog;
using Toltech.App.Services.Logging;
using Toltech.App.Services.Notification;
using Toltech.App.ViewModels;
using Toltech.App.Views;
using Toltech.Cad.Abstractions;
using Toltech.FreeCAD;
using Toltech.Solver.Contracts;
using Toltech.App.Resources.ColorTheme;

namespace Toltech.App
{
    /// Interaction logic for App.xaml
    public partial class App : Application
    {
        public static ILoggerService Logger { get; set; }
        public static INotificationService NotificationService { get; set; }
        public static IDialogService DialogService { get; set; }

        public static UiSettingsService UiSettings { get; private set; }
        public static AppSettings Settings { get; private set; }
        public static MainViewModel MainVM { get; set; }

        #region Test cad  TODO

        private ICadSelectionService? _cadSelectionService;
        public ICadSelectionService CadSelectionService
        {
            get
            {
                if (_cadSelectionService is null)
                {
                    throw new InvalidOperationException(
                        "Le service CAO n'a pas été initialisé.");
                }

                return _cadSelectionService;
            }
        }


        private FreeCadHttpServer? _freeCadHttpServer;


        #endregion

        public App()
        {
            Logger = new LoggerService();
            NotificationService = new NotificationService();
            DialogService = new DialogService();

            Settings = new AppSettings();

            UiSettings = new UiSettingsService();

            #region CAD INTEGRATION

            string freeCadPythonDirectory = Path.Combine(
                                                        AppContext.BaseDirectory,
                                                        "FreeCAD");

            // Création de l'installateur spécifique à FreeCAD.
            var installer = new FreeCadModuleInstaller(
                                                        freeCadPythonDirectory
                                                        );

            installer.Install();

            var cadProvider = new CadServiceProvider();

            var freeCad = new FreeCadApplication();

            cadProvider.Register(freeCad);

            ICadSelectionService freeCadSelectionService = freeCad.Selection;

            _cadSelectionService = freeCadSelectionService;

            _freeCadHttpServer = new FreeCadHttpServer(freeCadSelectionService);

            _freeCadHttpServer.Start();

            #endregion
        }

        /// <summary>
        /// Initializes the application, sets up logging, creates the main view model, performs asynchronous
        /// initialization tasks, loads UI settings, applies resource settings, and configures shutdown behavior.
        /// </summary>
        /// <param name="e">Event data for the application startup event.</param>
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Logger.LogInfo("Application démarrée");

            try
            {
                // Création du moteur de calcul.

                IComputeEngine engine = ToltechCalculation.ComputeEngineFactory.Create();

                // Assure le chargement initial des ressources.
                AppResourceLoader.ApplySettings(Settings);

                // Création du ViewModel principal.
                MainVM = new MainViewModel(engine);

                // Initialise les éléments nécessitant des opérations asynchrones,
                // notamment la base de données.
                await MainVM.InitializeAsync();


                // Create the main window with its ViewModel.
                var mainWindow = new MainWindow(MainVM);

                // Display the main window.
                mainWindow.Show();

                // Chargement des paramètres d'interface utilisateur.
                await UiSettings.LoadAsync();


                EventWindow(); //

                //if (!AccessControl.VerifyAccess())
                //{
                //    Shutdown();
                //    return;
                //}


                ShutdownMode = ShutdownMode.OnMainWindowClose;


                //AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                //{
                //    MessageBox.Show("Exception non gérée : " + e.ExceptionObject.ToString());
                //};
            }
            catch (Exception ex)
            {
                Logger.LogError(
                    $"Erreur lors du démarrage de l'application : {ex}",
                    nameof(App));

                Shutdown();
            }
        }

        private void EventWindow()
        {
            SystemEvents.UserPreferenceChanged += (_, e) =>
            {
                if (e.Category != UserPreferenceCategory.General || Settings.Theme != AppTheme.System)
                    return;

                Dispatcher.Invoke(() =>
                {
                    ThemeManager.ApplyFromSettings();   // réapplique le thème système à l'interface
                    Settings.RefreshEffectiveTheme();   // met le toggle à jour
                });
            };

        }
        protected override async void OnExit(ExitEventArgs e)
        {
            Logger.LogInfo("Application fermée");
            base.OnExit(e);
        }
     }



}
