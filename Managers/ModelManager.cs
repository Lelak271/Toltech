using System;
using System.Diagnostics;
using System.IO;
using Toltech.App.Properties;

namespace Toltech.App.Services
{
    /// <summary>
    /// Gestionnaire statique centralisant les informations globales liées au modèle
    /// actif de l'application Toltech ainsi que les chemins de fichiers et de dossiers
    /// utilisés par l'application.
    ///
    /// <para>
    /// Cette classe permet notamment de :
    /// <list type="bullet">
    /// <item>gérer le chemin du modèle actif ;</item>
    /// <item>gérer le chemin de stockage des données de l'application ;</item>
    /// <item>gérer le chemin des fichiers de résultats ;</item>
    /// <item>notifier les composants de l'application lors d'un changement de ces valeurs ;</item>
    /// <item>centraliser la création et l'accès aux différents dossiers utilisés par Toltech.</item>
    /// </list>
    /// </para>
    /// </summary>
    public static class ModelManager
    {
        #region Champs privés

        private static string _modelActif;

        // Chemin de stockage des données de l'application.
        // Initialisé avec le répertoire de la base de données par défaut.
        private static string _appDataPath = AppDataPathDefault;

        // Chemin du fichier de ressources.
        private static string _filepathResx = ResultsPath;

        // Chemin du répertoire temporaire Toltech.
        private static string _temporaryToltechPath = TemporaryPath;

        #endregion

        #region Événements

        /// <summary>
        /// Se produit lorsque le modèle actif est modifié.
        /// </summary>
        public static event Action<object> OnModelChanged;

        /// <summary>
        /// Se produit lorsqu'une pièce du modèle est modifiée.
        /// </summary>
        public static event Action<object> OnPartChanged;

        /// <summary>
        /// Se produit lorsque le chemin des données de l'application est modifié.
        /// </summary>
        public static event Action<string> OnAppDataPathChanged;

        /// <summary>
        /// Se produit lorsque le chemin du fichier de résultats est modifié.
        /// </summary>
        public static event Action<string> FilePathResxChanged;

        #endregion

        #region Propriétés

        /// <summary>
        /// Obtient ou définit le modèle actuellement actif dans la base de données.
        /// </summary>
        public static string ModelActif
        {
            get => _modelActif;
            set
            {
                if (_modelActif != value)
                {
                    _modelActif = value;

                    Debug.WriteLine(
                        $"ModelActif Changed : Path - {_modelActif}");

                    // Notifie les abonnés du changement, y compris lorsque
                    // la nouvelle valeur est null.
                    OnModelChanged?.Invoke(_modelActif);
                }
            }
        }

        public static string NameModelActif => string.IsNullOrEmpty(ModelActif) ? string.Empty : Path.GetFileNameWithoutExtension(ModelActif);

        /// <summary>
        /// Obtient ou définit le chemin utilisé pour le stockage des données
        /// de l'application.
        /// </summary>
        public static string AppDataPath
        {
            get => _appDataPath;
            set
            {
                Debug.WriteLine(
                    "[ModelManager] - AppDataPath Mettre en variable App");

                if (_appDataPath != value)
                {
                    _appDataPath = value;
                    OnAppDataPathChanged?.Invoke(_appDataPath);
                }
            }
        }


        /// <summary>
        /// Obtient ou définit le chemin utilisé pour l'affichage et l'export
        /// des résultats.
        /// </summary>
        public static string FilePathResx
        {
            get => _filepathResx;
            set
            {
                if (_filepathResx != value)
                {
                    _filepathResx = value;
                    FilePathResxChanged?.Invoke(_filepathResx);
                }
            }
        }
        
        /// <summary>
        /// Obtient ou définit le chemin utilisé pour l'affichage et l'export
        /// des résultats.
        /// </summary>
        public static string TemporaryToltechPath
        {
            get => _temporaryToltechPath;
            set
            {
                if (_temporaryToltechPath != value)
                {
                    _temporaryToltechPath = value;
                }
            }
        }

        #endregion

        #region Gestion des dossiers

        /// <summary>
        /// Répertoire racine utilisé pour le stockage des données Toltech.
        /// </summary>
        public static string ToltechPath
        {
            get
            {
                string path = @"C:\Toltech";

                // Création automatique du répertoire racine.
                Directory.CreateDirectory(path);

                return path;
            }
        }

        /// <summary>
        /// Répertoire utilisé pour la base de données par défaut.
        /// </summary>
        public static string AppDataPathDefault =>
            GetOrCreateDirectory("BD_Default");

        /// <summary>
        /// Répertoire temporaire utilisé par Toltech.
        /// </summary>
        public static string TemporaryPath =>
            GetOrCreateDirectory("Temp");

        /// <summary>
        /// Répertoire utilisé pour le stockage des résultats.
        /// </summary>
        public static string ResultsPath =>
            GetOrCreateDirectory("Results");

        /// <summary>
        /// Répertoire utilisé pour le stockage des métadonnées des modèles.
        /// </summary>
        public static string ModelMetaPath =>
            GetOrCreateDirectory("ModelMeta");

        /// <summary>
        /// Répertoire utilisé pour le stockage des Setting.
        /// </summary>
        public static string SettingsPath =>
            GetOrCreateDirectory("Settings");


        /// <summary>
        /// Retourne le chemin d'un sous-répertoire Toltech et le crée s'il n'existe pas.
        /// </summary>
        /// <param name="directoryName">
        /// Nom du sous-répertoire à créer.
        /// </param>
        /// <returns>
        /// Chemin complet du sous-répertoire.
        /// </returns>
        private static string GetOrCreateDirectory(string directoryName)
        {
            string path = Path.Combine(ToltechPath, directoryName);

            Directory.CreateDirectory(path);

            return path;
        }

        #endregion
    }
}