using System;
using System.Diagnostics;
using System.IO;

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

        // Initialise le chemin des données avec le chemin par défaut.
        private static string _appDataPath = AppDataPathDefault();

        private static string _filepathResx = GetResultsPath();
        private static string _temporaryToltechPath = GetTolTechTemporaryPath();

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
        /// Retourne le chemin du répertoire temporaire utilisé par Toltech.
        /// Le répertoire est créé automatiquement s'il n'existe pas.
        /// </summary>
        /// <returns>
        /// Le chemin complet du répertoire temporaire Toltech.
        /// </returns>
        public static string GetTolTechTemporaryPath()
        {
            // Récupération du répertoire temporaire de l'utilisateur.
            string tempPath = Path.GetTempPath();

            // Combinaison avec le nom du sous-dossier de l'application.
            string usertempPath = Path.Combine(
                tempPath,
                "TolTech_Temp");

            // Création du répertoire s'il n'existe pas.
            if (!Directory.Exists(usertempPath))
            {
                Directory.CreateDirectory(usertempPath);
            }

            return usertempPath;
        }


        /// <summary>
        /// Retourne le chemin du répertoire de stockage des résultats Toltech
        /// dans le dossier Documents de l'utilisateur.
        /// Le répertoire est créé automatiquement s'il n'existe pas.
        /// </summary>
        /// <returns>
        /// Le chemin complet du répertoire des résultats.
        /// </returns>
        public static string GetResultsPath()
        {
            string path = Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.MyDocuments),
                "TolTech",
                "Results");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }


        /// <summary>
        /// Retourne le chemin du répertoire contenant les métadonnées des modèles
        /// Toltech dans le dossier Documents de l'utilisateur.
        /// Le répertoire est créé automatiquement s'il n'existe pas.
        /// </summary>
        /// <returns>
        /// Le chemin complet du répertoire des métadonnées des modèles.
        /// </returns>
        public static string GetModelMetaPath()
        {
            string path = Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.MyDocuments),
                "TolTech",
                "ModelMeta");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }


        /// <summary>
        /// Retourne le chemin par défaut utilisé pour le stockage des données
        /// de la base de données Toltech.
        /// Le répertoire est créé automatiquement s'il n'existe pas.
        /// </summary>
        /// <returns>
        /// Le chemin complet du répertoire de données par défaut.
        /// </returns>
        public static string AppDataPathDefault()
        {
            string path = @"C:\Toltech\DataBase_Default";

            // Crée le dossier s'il n'existe pas.
            Directory.CreateDirectory(path);

            return path;
        }

        #endregion
    }
}