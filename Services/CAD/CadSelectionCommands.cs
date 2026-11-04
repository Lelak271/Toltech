using System.Windows.Input;

namespace Toltech.App.Services.CAD
{
    public static class CadSelectionCommands
    {
        public static readonly RoutedUICommand PickPoint = new RoutedUICommand(
            "Pick a CAD point",
            nameof(PickPoint),
            typeof(CadSelectionCommands));


        public static readonly RoutedUICommand PickEdge = new RoutedUICommand(
            "Pick a CAD edge",
            nameof(PickEdge),
            typeof(CadSelectionCommands));
    }
}
