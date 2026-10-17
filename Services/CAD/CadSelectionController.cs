using System.Net.Http;
using System.Windows;

using Toltech.App.Views.Controls.CAD;
using Toltech.Cad.Abstractions;
using Toltech.Cad.Model;

namespace Toltech.App.Services.CAD
{
    /// <summary>
    /// Centralise la gestion des sélections dans le logiciel de CAO.
    /// Une instance par UserControl : seul le contrôleur qui a lancé une demande
    /// relaie le résultat à son propriétaire, les autres l'ignorent.
    /// </summary>
    public sealed class CadSelectionController : IDisposable
    {
        private readonly ICadSelectionService _cadSelectionService;

        private CadPointSelectionWindow? _selectionWindow;

        private bool _isDisposed;

        // Vrai entre le lancement de la demande et la réception du résultat
        // (ou la fermeture de la fenêtre / une erreur).
        private volatile bool _isSelecting;

        /// <summary>
        /// Notifie le composant propriétaire lorsqu'une sélection est reçue.
        /// Toujours levé sur le thread UI.
        /// </summary>
        public event EventHandler<CadSelection>? SelectionChanged;

        /// <summary>Une sélection est en cours pour ce contrôleur.</summary>
        public bool IsSelecting => _isSelecting;

        public CadSelectionController(ICadSelectionService cadSelectionService)
        {
            ArgumentNullException.ThrowIfNull(cadSelectionService);

            _cadSelectionService = cadSelectionService;
            _cadSelectionService.SelectionChanged += OnServiceSelectionChanged;
        }

        /// <summary>
        /// Résultat reçu du service CAO (peut venir d'un thread autre que l'UI).
        /// </summary>
        private void OnServiceSelectionChanged(object? sender, CadSelection selection)
        {
            // Ce contrôleur n'attend rien : le résultat est destiné à un autre contrôle.
            if (!_isSelecting)
                return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                if (_isDisposed || !_isSelecting)
                    return;

                // La sélection est terminée : on libère l'état et on ferme la fenêtre
                // avant de prévenir le propriétaire.
                _isSelecting = false;
                CloseSelectionWindow();

                SelectionChanged?.Invoke(this, selection);
            });
        }

        /// <summary>
        /// Démarre une sélection CAO et affiche la fenêtre associée.
        /// La méthode rend la main dès que la demande est envoyée au logiciel CAO :
        /// la fenêtre reste ouverte jusqu'à la réception du résultat.
        /// </summary>
        public async Task StartSelectionAsync(Window? owner, CadSelectionType selectionType)
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);

            // Évite plusieurs sélections simultanées avec ce contrôleur.
            if (_isSelecting)
                return;

            _isSelecting = true;

            try
            {
                var window = new CadPointSelectionWindow(_cadSelectionService)
                {
                    Owner = owner
                };
                window.Closed += OnSelectionWindowClosed;

                _selectionWindow = window;
                window.Show();

                // Demande au logiciel de CAO actif de commencer la sélection.
                await _cadSelectionService.StartSelectionAsync(selectionType);
            }
            catch (HttpRequestException)
            {
                Abort();

                MessageBox.Show(
                    "Aucun logiciel de CAO n'est actuellement connecté à Toltech.",
                    "CAO",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            catch (NotSupportedException error)
            {
                Abort();

                MessageBox.Show(
                    error.Message,
                    "CAO",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception error)
            {
                Abort();

                MessageBox.Show(
                    $"Erreur lors de la sélection CAO :\n{error.Message}",
                    "CAO",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>Annule la sélection en cours (erreur) et ferme la fenêtre.</summary>
        private void Abort()
        {
            _isSelecting = false;
            CloseSelectionWindow();
        }

        /// <summary>
        /// L'utilisateur a fermé la fenêtre à la main : la demande est abandonnée.
        /// </summary>
        private void OnSelectionWindowClosed(object? sender, EventArgs e)
        {
            if (sender is CadPointSelectionWindow window)
                window.Closed -= OnSelectionWindowClosed;

            if (ReferenceEquals(_selectionWindow, sender))
            {
                _selectionWindow = null;
                _isSelecting = false;
            }
        }

        /// <summary>
        /// Ferme la fenêtre de sélection si elle existe.
        /// </summary>
        public void CloseSelectionWindow()
        {
            var window = _selectionWindow;
            if (window is null)
                return;

            _selectionWindow = null;
            window.Closed -= OnSelectionWindowClosed;

            // Close() doit être appelé sur le thread UI.
            if (!window.Dispatcher.CheckAccess())
            {
                window.Dispatcher.Invoke(window.Close);
                return;
            }

            window.Close();
        }

        /// <summary>
        /// Libère l'abonnement au service de sélection CAO.
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            _isSelecting = false;

            _cadSelectionService.SelectionChanged -= OnServiceSelectionChanged;

            CloseSelectionWindow();

            SelectionChanged = null;
        }
    }
}