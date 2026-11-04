using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Toltech.App.Behaviors
{
    #region OLD
    /// <summary>
    /// Comportement attaché pour les TextBox qui gère la touche Enter.
    /// Lorsqu'Enter est pressé :
    /// - si <see cref="TextBox.AcceptsReturn"/> est false, déclenche un LostFocus
    ///   sans déplacer le focus vers un autre contrôle,
    /// - évite le bip sonore et l’insertion de saut de ligne.
    /// </summary>
    public static class TextBoxEnterBehavior
    {
        public static readonly DependencyProperty MoveFocusOnEnterProperty =
            DependencyProperty.RegisterAttached(
                "MoveFocusOnEnter",
                typeof(bool),
                typeof(TextBoxEnterBehavior),
                new PropertyMetadata(false, OnMoveFocusOnEnterChanged));

        public static readonly DependencyProperty AllowEnterKeyProperty =
    DependencyProperty.RegisterAttached(
        "AllowEnterKey",
        typeof(bool),
        typeof(TextBoxEnterBehavior),
        new PropertyMetadata(false));

        public static bool GetAllowEnterKey(DependencyObject obj) =>
            (bool)obj.GetValue(AllowEnterKeyProperty);

        public static void SetAllowEnterKey(DependencyObject obj, bool value) =>
            obj.SetValue(AllowEnterKeyProperty, value);


        public static bool GetMoveFocusOnEnter(DependencyObject obj) => (bool)obj.GetValue(MoveFocusOnEnterProperty);
        public static void SetMoveFocusOnEnter(DependencyObject obj, bool value) => obj.SetValue(MoveFocusOnEnterProperty, value);

        private static void OnMoveFocusOnEnterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox tb)
            {
                if ((bool)e.NewValue)
                    tb.PreviewKeyDown += Tb_PreviewKeyDown;
                else
                    tb.PreviewKeyDown -= Tb_PreviewKeyDown;
            }
        }

        private static void Tb_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (sender is not TextBox tb)
                return;

            // Laisser passer Enter pour les TextBox qui en ont besoin (rename, édition inline, etc.)
            if (GetAllowEnterKey(tb))
                return;

            if (e.Key == Key.Enter && !tb.AcceptsReturn)
            {
                Keyboard.ClearFocus();
                e.Handled = true;
            }
        }

    }
    
    #endregion

    public static class TextBoxEnterBehavior2
    {
        public static readonly DependencyProperty EnableProperty =
            DependencyProperty.RegisterAttached(
                "Enable",
                typeof(bool),
                typeof(TextBoxEnterBehavior2),
                new PropertyMetadata(false, OnEnableChanged));

        public static bool GetEnable(DependencyObject obj) =>
            (bool)obj.GetValue(EnableProperty);

        public static void SetEnable(DependencyObject obj, bool value) =>
            obj.SetValue(EnableProperty, value);

        private static void OnEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TextBox textBox)
                return;

            if ((bool)e.NewValue)
                textBox.PreviewKeyDown += TextBox_PreviewKeyDown;
            else
                textBox.PreviewKeyDown -= TextBox_PreviewKeyDown;
        }

        private static void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (sender is not TextBox textBox)
                return;

            // Laisser Enter fonctionner si le TextBox accepte les retours à la ligne.
            if (textBox.AcceptsReturn || e.Key != Key.Enter)
                return;

            // Empêcher le retour à la ligne et le bip sonore.
            e.Handled = true;

            // Déplacer le focus vers le contrôle suivant.
            textBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }
    }

}
