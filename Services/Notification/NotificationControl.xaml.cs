using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Wpf.Ui.Controls;

namespace Toltech.App.Services.Notification
{
    public enum NotificationSeverity
    {
        Info,
        Success,
        Warning,
        Error
    }

    public partial class NotificationControl : UserControl
    {
        private static readonly TimeSpan EnterDuration = TimeSpan.FromMilliseconds(300);
        private static readonly TimeSpan ExitDuration = TimeSpan.FromMilliseconds(250);
        private static readonly TimeSpan DefaultDisplayDuration = TimeSpan.FromSeconds(3);

        // Complété par un clic (ou par Dismiss()) : ShowAsync se réveille immédiatement
        private readonly TaskCompletionSource _dismissTcs =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public NotificationControl()
        {
            InitializeComponent();

            Opacity = 0; // invisible tant que ShowAsync n'a pas démarré
            ClipHost.SizeChanged += (_, _) => UpdateClip();
        }

        public NotificationControl(string message, string? title = null) : this()
        {
            MessageText.Text = message;

            if (!string.IsNullOrWhiteSpace(title))
                TitleText.Text = title;
        }

        // ------------------------------------------------------------------
        //  API publique
        // ------------------------------------------------------------------

        /// <summary>Compatibilité avec l'ancien appel : ShowAsync(true) = erreur.</summary>
        public Task ShowAsync(bool isError) =>
            ShowAsync(isError ? NotificationSeverity.Error : NotificationSeverity.Info);

        public async Task ShowAsync(NotificationSeverity severity = NotificationSeverity.Info,
                                    TimeSpan? duration = null)
        {
            var display = duration ?? DefaultDisplayDuration;

            // À faire ici (et non dans le constructeur) : le contrôle est normalement déjà dans
            // l'arbre visuel, donc TryFindResource voit aussi les ressources de la fenêtre.
            ApplyTheme(severity);

            // Entrée : fondu + léger glissement vers le haut
            await Task.WhenAll(
                AnimateAsync(this, OpacityProperty, 0, 1, EnterDuration),
                AnimateAsync(CardTranslate, TranslateTransform.YProperty, 12, 0, EnterDuration,
                             new CubicEase { EasingMode = EasingMode.EaseOut }));

            // Remplissage progressif pendant la durée d'affichage
            FillScale.BeginAnimation(ScaleTransform.ScaleXProperty,
                new DoubleAnimation(0, 1, display)
                {
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                });

            // Attend la fin du délai OU un clic, selon ce qui arrive en premier
            await Task.WhenAny(Task.Delay(display), _dismissTcs.Task);

            await CloseAsync();
        }

        /// <summary>Ferme la notification depuis l'extérieur.</summary>
        public void Dismiss() => _dismissTcs.TrySetResult();

        // ------------------------------------------------------------------
        //  Thème
        // ------------------------------------------------------------------

        private readonly record struct Palette(string AccentKey, Brush AccentFallback, string Glyph);

        private static Palette GetPalette(NotificationSeverity severity) => severity switch
        {
            NotificationSeverity.Success => new("SystemFillColorSuccessBrush", Frozen("#0F7B0F"), "\uE73E"),
            NotificationSeverity.Warning => new("SystemFillColorCautionBrush", Frozen("#9D5D00"), "\uE7BA"),
            NotificationSeverity.Error => new("SystemFillColorCriticalBrush", Frozen("#C42B1C"), "\uEA39"),
            _ => new("SystemAccentColorPrimaryBrush", SystemColors.HighlightBrush, "\uE946")
        };

        private void ApplyTheme(NotificationSeverity severity)
        {
            var palette = GetPalette(severity);

            // Surface
            ApplyBrush(Card, Border.BackgroundProperty,
                       "ControlSolidFillColorDefaultBrush", SystemColors.WindowBrush);
            ApplyBrush(Card, Border.BorderBrushProperty,
                       "ControlStrokeColorDefaultBrush", SystemColors.ControlDarkBrush);

            // Textes
            ApplyBrush(MessageText, System.Windows.Controls.TextBlock.ForegroundProperty,
                       "TextFillColorPrimaryBrush", SystemColors.ControlTextBrush);
            ApplyBrush(TitleText, System.Windows.Controls.TextBlock.ForegroundProperty,
                       "TextFillColorSecondaryBrush", SystemColors.GrayTextBrush);
            ApplyBrush(CloseText, System.Windows.Controls.TextBlock.ForegroundProperty,
                       "TextFillColorSecondaryBrush", SystemColors.GrayTextBrush);

            // Accent (dépend de la sévérité)
            ApplyBrush(IconText, System.Windows.Controls.TextBlock.ForegroundProperty, palette.AccentKey, palette.AccentFallback);
            ApplyBrush(Fill, Border.BackgroundProperty, palette.AccentKey, palette.AccentFallback);

            IconText.Text = palette.Glyph;
        }

        /// <summary>
        /// Si la clé existe (WPF-UI chargé), on crée une référence dynamique : le brush suit les
        /// changements de thème en direct. Sinon, on applique le repli SystemColors.
        /// </summary>
        private void ApplyBrush(FrameworkElement target, DependencyProperty property, string key, Brush fallback)
        {
            if (TryFindResource(key) is not null)
                target.SetResourceReference(property, key);
            else
                target.SetValue(property, fallback);
        }

        private static SolidColorBrush Frozen(string hex)
        {
            var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
            brush.Freeze();
            return brush;
        }

        // ------------------------------------------------------------------
        //  Interactions / animations
        // ------------------------------------------------------------------

        private void OnNotificationClicked(object sender, System.Windows.Input.MouseButtonEventArgs e) =>
            Dismiss();

        private async Task CloseAsync()
        {
            // Fige le remplissage à sa valeur actuelle (sinon il retomberait à 0 pendant le fondu)
            var current = FillScale.ScaleX;
            FillScale.BeginAnimation(ScaleTransform.ScaleXProperty, null);
            FillScale.ScaleX = current;

            await Task.WhenAll(
                AnimateAsync(this, OpacityProperty, null, 0, ExitDuration),
                AnimateAsync(CardTranslate, TranslateTransform.YProperty, null, 8, ExitDuration,
                             new CubicEase { EasingMode = EasingMode.EaseIn }));
        }

        private static Task AnimateAsync(IAnimatable target, DependencyProperty property,
                                         double? from, double to, TimeSpan duration,
                                         IEasingFunction? easing = null)
        {
            var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

            var animation = new DoubleAnimation { From = from, To = to, Duration = duration, EasingFunction = easing };
            animation.Completed += (_, _) => tcs.TrySetResult();

            target.BeginAnimation(property, animation);
            return tcs.Task;
        }

        /// <summary>Clip arrondi : les coins du remplissage suivent ceux de la carte.</summary>
        private void UpdateClip()
        {
            var radius = Math.Max(0, Card.CornerRadius.TopLeft - Card.BorderThickness.Left);

            ClipHost.Clip = new RectangleGeometry(
                new Rect(0, 0, ClipHost.ActualWidth, ClipHost.ActualHeight), radius, radius);
        }
    }
}