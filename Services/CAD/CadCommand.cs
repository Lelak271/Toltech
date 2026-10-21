using System.Windows;
using System.Windows.Input;

namespace Toltech.App.Services.CAD
{
    class CadCommand
    {

        // Commande à exécuter (PickPoint, PickEdge, ...)
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached(
                "Command", typeof(ICommand), typeof(CadCommand),
                new PropertyMetadata(null));

        public static ICommand GetCommand(DependencyObject d) => (ICommand)d.GetValue(CommandProperty);
        public static void SetCommand(DependencyObject d, ICommand v) => d.SetValue(CommandProperty, v);

        // Titre du menu ("Sélectionner un point dans la CAO", ...)
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.RegisterAttached(
                "Header", typeof(string), typeof(CadCommand),
                new PropertyMetadata(null));

        public static string GetHeader(DependencyObject d) => (string)d.GetValue(HeaderProperty);
        public static void SetHeader(DependencyObject d, string v) => d.SetValue(HeaderProperty, v);
    }
    
}
