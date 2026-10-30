using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using DocumentFormat.OpenXml.Spreadsheet;
using Toltech.App.Services;
using Toltech.App.ViewModels;
using Toltech.App.Views.Controls.GrapheControl;
using Toltech.App.Views.Controls.GrapheControl.GraphData;
using Westermo.GraphX.Common.Enums;
using Westermo.GraphX.Controls;
using Westermo.GraphX.Logic.Algorithms.EdgeRouting;
using Westermo.GraphX.Logic.Algorithms.LayoutAlgorithms;

namespace Toltech.App.FrontEnd.GraphePage.Controls
{
    public partial class MainGraphe : Wpf.Ui.Controls.FluentWindow, IDisposable
    {
        private readonly MainViewModel _mainVM;
        public MainViewModel MainVM => _mainVM;

        private readonly DomainService _domainService;

        #region Types et constantes

        /// <summary>Options du graphe. Source unique : l'état des contrôles de l'UI.</summary>
        public class GraphLayoutOptions
        {
            public LayoutAlgorithmTypeEnum LayoutAlgorithm { get; set; } = LayoutAlgorithmTypeEnum.KK;
            public OverlapRemovalAlgorithmTypeEnum OverlapRemovalAlgorithm { get; set; } = OverlapRemovalAlgorithmTypeEnum.FSA;
            public EdgeRoutingAlgorithmTypeEnum EdgeRoutingAlgorithm { get; set; } = EdgeRoutingAlgorithmTypeEnum.SimpleER;
            public int MaxIterations { get; set; } = 4000;
            public float HorizontalGap { get; set; } = 50;
            public float VerticalGap { get; set; } = 50;
            public bool UseAsyncCompute { get; set; } = false;
            public bool EnableParallelEdges { get; set; } = true;
            public int ParallelEdgeDistance { get; set; } = 10;

            public static GraphLayoutOptions Default => new();
        }

        /// <summary>Représentation neutre d'une liaison (ModelData ou Requirement).</summary>
        private sealed record GraphLink(string Kind, object Id, int From, int To, string Text, bool IsRequirement);

        private const double EdgeLabelFontSize = 3;

        private static readonly Uri DefaultVertexImageUri =
            new("pack://application:,,,/Asset/add_model.png");

        private static readonly BitmapImage DefaultVertexImage =
            CreateBitmapImage(DefaultVertexImageUri);

        // Dictionnaire associant l'ID d'une pièce à son image.
        private Dictionary<int, BitmapImage> VertexImages = new();

        private static BitmapImage CreateBitmapImage(Uri uri)
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = uri;
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.EndInit();
            image.Freeze();

            return image;
        }

        #endregion

        private readonly Random _rand = new();
        private GXLogicCoreExample _logicCore;
        private bool _isUpdatingUi;
        private bool _hasLoaded;

        private readonly DatabaseService _databaseService;

        public MainGraphe(MainViewModel mainVM)
        {
            InitializeComponent();

            _mainVM = mainVM;
            _databaseService = _mainVM.DatabaseService;
            _domainService = mainVM.DomainService;

            var defaults = GraphLayoutOptions.Default;
            FillCombo(LayoutCombo, defaults.LayoutAlgorithm);
            FillCombo(OverlapCombo, defaults.OverlapRemovalAlgorithm);
            FillCombo(EdgeRoutingCombo, defaults.EdgeRoutingAlgorithm);

            ZoomControl.SetViewFinderVisibility(zoomctrl, Visibility.Visible);
            Loaded += OnLoaded;
        }

        private static void FillCombo<T>(ComboBox box, T selected) where T : struct, Enum
        {
            box.ItemsSource = Enum.GetValues<T>();
            box.SelectedItem = selected;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (_hasLoaded) return; // évite de tout recharger à chaque retour sur la page
            _hasLoaded = true;
            await LoadGraphAsync();
        }

        #region Chargement du graphe

        private async Task LoadGraphAsync()
        {
            ReloadButton.IsEnabled = false;
            try
            {
                var modelData = await _databaseService.GetAllModelDataAsync();
                var requirements = await _databaseService.GetAllRequirementsAsync();

                var links = modelData
                    .Select(m => new GraphLink("La liaison", m.Id, m.OriginePartId, m.ExtremitePartId.Value, m.Model, false))
                    .Concat(requirements
                    .Select(r => new GraphLink("L'exigence", r.Id_req, r.PartReq1Id.Value, r.PartReq2Id.Value, r.NameReq, true)));

                var (graph, errors) = await BuildGraph(links);
                if (errors.Count > 0)
                {
                    MessageBox.Show(
                        "Certaines liaisons sont nulles, vides ou référencent des pièces manquantes. " +
                        "Elles n'apparaîtront pas dans le graphe." + Environment.NewLine +
                        string.Join(Environment.NewLine, errors.Select(err => "  - " + err)),
                        "Erreur de liaison", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                _logicCore = new GXLogicCoreExample { Graph = graph };
                ApplyOptions(_logicCore, ReadOptions());
                Area.LogicCore = _logicCore;

                Area.GenerateGraph(true);
                ApplyGraphStyle();
                zoomctrl.ZoomToFill();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de la génération du graphe : " + ex.Message,
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                ReloadButton.IsEnabled = true;
            }
        }

        private async Task<(GraphExample Graph, List<string> Errors)> BuildGraph(
            IEnumerable<GraphLink> links)
        {
            var graph = new GraphExample();
            var vertices = new Dictionary<int, DataVertex>();
            var errors = new List<string>();

            VertexImages = await _domainService.GetPartImagesAsync();

            DataVertex GetOrAdd(int id)
            {
                if (vertices.TryGetValue(id, out var vertex))
                    return vertex;

                vertex = new DataVertex(id.ToString())
                {
                    // L'identifiant de la pièce est également utilisé comme ImageId.
                    ImageId = id,

                    // Utilise l'image de la base de données ou l'image par défaut.
                    ImageSource = VertexImages.TryGetValue(id, out var image)
                                ? image
                                : DefaultVertexImage,

                    ImageHeight = ImageSizeSlider.Value
                };

                vertices[id] = vertex;
                graph.AddVertex(vertex);

                return vertex;
            }

            foreach (var link in links)
            {
                // Vérifie que les identifiants des pièces sont valides.
                if (link.From <= 0 || link.To <= 0)
                {
                    errors.Add($"{link.Kind} ID : {link.Id} est non valide.");
                    continue;
                }

                // Crée l'arête entre les deux sommets du graphe.
                graph.AddEdge(new DataEdge(
                    GetOrAdd(link.From),
                    GetOrAdd(link.To))
                {
                    // Définit le libellé de l'arête.
                    Text = link.Text ?? $"{link.From} → {link.To}",

                    // Indique s'il s'agit d'une exigence.
                    IsRequirement = link.IsRequirement
                });
            }

            return (graph, errors);
        }

        #endregion

        #region Options (UI -> LogicCore)

        private GraphLayoutOptions ReadOptions() => new()
        {
            LayoutAlgorithm = (LayoutAlgorithmTypeEnum)LayoutCombo.SelectedItem,
            OverlapRemovalAlgorithm = (OverlapRemovalAlgorithmTypeEnum)OverlapCombo.SelectedItem,
            EdgeRoutingAlgorithm = (EdgeRoutingAlgorithmTypeEnum)EdgeRoutingCombo.SelectedItem,
            EnableParallelEdges = ParallelEdgesToggle.IsChecked == true,
            ParallelEdgeDistance = (int)EdgeDistanceSlider.Value
        };

        private static void ApplyOptions(GXLogicCoreExample core, GraphLayoutOptions o)
        {
            var isSugiyama = o.LayoutAlgorithm == LayoutAlgorithmTypeEnum.EfficientSugiyama;

            // Layout
            core.DefaultLayoutAlgorithm = o.LayoutAlgorithm;
            var layoutParams = core.AlgorithmFactory.CreateLayoutParameters(o.LayoutAlgorithm);
            switch (layoutParams)
            {
                case KKLayoutParameters kk:
                    kk.MaxIterations = o.MaxIterations;
                    break;
                case EfficientSugiyamaLayoutParameters sugiyama:
                    sugiyama.EdgeRouting = SugiyamaEdgeRoutings.Orthogonal;
                    sugiyama.LayerDistance = sugiyama.VertexDistance = 100;
                    break;
            }
            core.DefaultLayoutAlgorithmParams = layoutParams;

            // Overlap
            core.DefaultOverlapRemovalAlgorithm = o.OverlapRemovalAlgorithm;
            var overlapParams = core.AlgorithmFactory.CreateOverlapRemovalParameters(o.OverlapRemovalAlgorithm);
            if (overlapParams != null)
            {
                overlapParams.HorizontalGap = o.HorizontalGap;
                overlapParams.VerticalGap = o.VerticalGap;
            }
            core.DefaultOverlapRemovalAlgorithmParams = overlapParams;

            // Edge routing
            core.DefaultEdgeRoutingAlgorithm = o.EdgeRoutingAlgorithm;
            if (o.EdgeRoutingAlgorithm == EdgeRoutingAlgorithmTypeEnum.Bundling)
            {
                core.DefaultEdgeRoutingAlgorithmParams = new BundleEdgeRoutingParameters
                {
                    Iterations = 200,
                    SpringConstant = 5,
                    Threshold = .1f
                };
            }

            // Divers
            core.EdgeCurvingEnabled = !isSugiyama;
            core.EnableParallelEdges = o.EnableParallelEdges;
            core.ParallelEdgeDistance = o.ParallelEdgeDistance;
            core.AsyncAlgorithmCompute = o.UseAsyncCompute;
        }

        private void Relayout()
        {
            Area.RelayoutGraph();
            zoomctrl.ZoomToFill();
        }

        #endregion

        #region Style du graphe

        private void ApplyGraphStyle()
        {
            Area.SetVerticesDrag(true);
            Area.SetEdgesDashStyle(EdgeDashStyle.Dot);
            Area.ShowAllEdgesArrows(false);
            Area.ShowAllVerticesLabels(true);
            ApplyEdgeLabelOptions();

            foreach (var ec in Area.EdgesList.Values)
            {
                if (ec.Edge is DataEdge { IsRequirement: true })
                {
                    ec.ShowArrows = true;
                    ec.Foreground = Brushes.Red;
                }
            }

            // Les templates des labels ne sont appliqués qu'après le layout
            Dispatcher.InvokeAsync(ShrinkEdgeLabels, DispatcherPriority.Loaded);
        }

        private void ApplyEdgeLabelOptions()
        {
            Area.ShowAllEdgesLabels(EdgeLabelsToggle.IsChecked == true);
            Area.AlignAllEdgesLabels(AlignLabelsToggle.IsChecked == true);
        }

        private void ShrinkEdgeLabels()
        {
            foreach (var ec in Area.EdgesList.Values)
            {
                if (FindChildOfType<TextBlock>(ec) is { } label)
                    label.FontSize = EdgeLabelFontSize;
            }
        }

        private static T FindChildOfType<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;

            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T match) return match;

                var nested = FindChildOfType<T>(child);
                if (nested != null) return nested;
            }
            return null;
        }

        #endregion

        #region Handlers UI

        private async void OnReloadClick(object sender, RoutedEventArgs e) => await LoadGraphAsync();

        private void OnRelayoutClick(object sender, RoutedEventArgs e) => Relayout();

        private void OnLayoutAlgorithmChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_logicCore is null || _isUpdatingUi) return;

            // Sugiyama gère lui-même le routage orthogonal : on désactive l'edge routing
            if ((LayoutAlgorithmTypeEnum)LayoutCombo.SelectedItem == LayoutAlgorithmTypeEnum.EfficientSugiyama)
            {
                _isUpdatingUi = true;
                EdgeRoutingCombo.SelectedItem = EdgeRoutingAlgorithmTypeEnum.None;
                _isUpdatingUi = false;
            }

            ApplyAlgorithmOptions();
        }

        private void OnAlgorithmOptionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_logicCore is null || _isUpdatingUi) return;
            ApplyAlgorithmOptions();
        }

        private void ApplyAlgorithmOptions()
        {
            ApplyOptions(_logicCore, ReadOptions());
            Relayout();
        }

        private void OnParallelEdgesChanged(object sender, RoutedEventArgs e)
        {
            if (_logicCore is null) return;

            var options = ReadOptions();
            _logicCore.EnableParallelEdges = options.EnableParallelEdges;
            _logicCore.ParallelEdgeDistance = options.ParallelEdgeDistance;
            Area.UpdateAllEdges(true);
        }

        private void OnEdgeLabelOptionsChanged(object sender, RoutedEventArgs e)
        {
            if (_logicCore is null) return;

            ApplyEdgeLabelOptions();
            Area.InvalidateVisual();
        }

        private void OnImageSizeChanged(object sender, RoutedEventArgs e)
        {
            if (Area is null) return;

            foreach (var vertexControl in Area.VertexList.Values)
            {
                if (vertexControl.Vertex is DataVertex dataVertex)
                    dataVertex.ImageHeight = ImageSizeSlider.Value;
            }
        }

        #endregion

        public void Dispose() => Area.Dispose();
    }
}
