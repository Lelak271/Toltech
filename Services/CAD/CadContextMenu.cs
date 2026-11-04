using System.Windows;
using System.Windows.Input;

namespace Toltech.App.Services.CAD
{
    public static class CadContextMenu
    {
        // Commande associée au contrôle (PickPoint, PickEdge...).
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached(
                "Command",
                typeof(ICommand),
                typeof(CadContextMenu),
                new PropertyMetadata(null));

        public static ICommand? GetCommand(DependencyObject obj) =>
            (ICommand?)obj.GetValue(CommandProperty);

        public static void SetCommand(DependencyObject obj, ICommand? value) =>
            obj.SetValue(CommandProperty, value);


        // Titre affiché dans le menu contextuel.
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.RegisterAttached(
                "Header",
                typeof(string),
                typeof(CadContextMenu),
                new PropertyMetadata(null));

        public static string? GetHeader(DependencyObject obj) =>
            (string?)obj.GetValue(HeaderProperty);

        public static void SetHeader(DependencyObject obj, string? value) =>
            obj.SetValue(HeaderProperty, value);
    }
}