using AppPageGraphEdit.CORE;
using AppPageGraphEdit.CORE.Render;
using AppPageGraphEdit.UserControls;
using OPLAPI.CORE.Animation;
using OPLAPI.OIEL.CORE.Browser;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace AppPageGraphEdit
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : PageBrowser
    {
        /// <summary>
        /// Контейнер редактора графов
        /// </summary>
        internal static readonly Canvas EditContainerGraphs = new();

        /// <summary>
        /// Контейнер в котором редактируются графы
        /// </summary>
        internal static readonly Canvas CanvasGraph = new();

        /// <summary>
        /// Объект управляющий рендером графов
        /// </summary>
        internal static readonly RenderConnectionsGraph RenderConnectionsGraph = new();

        #region Points Control Move Container Graphs
        /// <summary>
        /// Стартовая точка перемещения контейнера редактора
        /// </summary>
        private Point StartPointMove;

        /// <summary>
        /// Текущая точка перемещения контейнера редактора
        /// </summary>
        private Point CurrentPointMove;
        #endregion

        /// <summary>
        /// Менеджер анимаций страницы
        /// </summary>
        public override OPLAnimationManager? ManagerAnimation
        {
            get => base.ManagerAnimation;
            set
            {
                base.ManagerAnimation = value;
            }
        }

        /// <summary>
        /// Состояние перемещения контейнера редактора
        /// </summary>
        private bool IsMove = false;

        public MainPage()
        {
            InitializeComponent();
            EditContainerGraphs.Background = new SolidColorBrush(Color.FromRgb(240, 240, 240));
            CanvasGraph.Background = new SolidColorBrush(Colors.Blue);
            BorderContainerWindow.Child = EditContainerGraphs;
            Canvas.SetLeft(CanvasGraph, 0);
            Canvas.SetTop(CanvasGraph, 0);
            EditContainerGraphs.Children.Add(CanvasGraph);

            ManagerAnimation = new();
            SourcePanelAction = new();

            #region AddNewGraph
            AddNewGraph.OnActivateMouseLeft += (sender, e) =>
            {
                Graph graph = new();
                CanvasGraph.Children.Add(graph.Visual);
                Canvas.SetLeft(graph.Visual, 0);
                Canvas.SetTop(graph.Visual, 0);
            };
            #endregion

            #region Library
            CreateLibraryElement(0, 1);
            CreateLibraryElement(0, 2);
            CreateLibraryElement(1, 0);
            CreateLibraryElement(2, 0);
            CreateLibraryElement(1, 2);
            CreateLibraryElement(2, 1);
            CreateLibraryElement(2, 2);
            #endregion

            EditContainerGraphs.MouseLeftButtonDown += BorderGraphWindowStartMoveHandler;
            EditContainerGraphs.MouseMove += BorderGraphWindowMouseMoveHandler;
            EditContainerGraphs.MouseLeftButtonUp += BorderGraphWindowEndMoveHandler;

            RenderConnectionsGraph.RenderStart();
        }

        /// <summary>
        /// Создать элемент библиотеки графов
        /// </summary>
        /// <param name="InputCount">Количество точек подключения входа</param>
        /// <param name="OutputCount">Количество точек подключения выхода</param>
        private GraphVisual CreateLibraryElement(int InputCount, int OutputCount)
        {
            int i;
            GraphVisual ItemGraph = new();
            ItemGraph.PreviewMouseLeftButtonDown += ManagerItemGraphInicializeHandler;
            for (i = 0; i < InputCount; i++)
                ItemGraph.AddNewInputGraphPoint();
            for (i = 0; i < OutputCount; i++)
                ItemGraph.AddNewOutputGraphPoint();
            StackPanelManagerElements.Children.Add(ItemGraph);
            return ItemGraph;
        }

        #region ContainerGraphs Events
        /// <summary>
        /// Обработчик события начала передвижения области редактора
        /// </summary>
        private void BorderGraphWindowStartMoveHandler(object sender, MouseButtonEventArgs e)
        {
            StartPointMove = e.GetPosition(CanvasGraph);
            CurrentPointMove = new Point(
                Canvas.GetLeft(CanvasGraph),
                Canvas.GetTop(CanvasGraph)
            );
            CanvasGraph.CaptureMouse();
            IsMove = true;
        }

        /// <summary>
        /// Обработчик события передвижения области редактора
        /// </summary>
        private void BorderGraphWindowMouseMoveHandler(object sender, MouseEventArgs e)
        {
            if (!IsMove) return;
            Point MouseCurrentPoint = e.GetPosition(CanvasGraph);
            Vector Delta = MouseCurrentPoint - StartPointMove;
            CurrentPointMove += Delta;

            Canvas.SetLeft(CanvasGraph, CurrentPointMove.X);
            Canvas.SetTop(CanvasGraph, CurrentPointMove.Y);
        }

        /// <summary>
        /// Обработчик события конца передвижения области редактора
        /// </summary>
        private void BorderGraphWindowEndMoveHandler(object sender, MouseButtonEventArgs e)
        {
            IsMove = false;
            CanvasGraph.ReleaseMouseCapture();
        }
        #endregion

        #region ManagerItemInicialize Events
        private void ManagerItemGraphInicializeHandler(object sender, MouseButtonEventArgs e)
        {
            int i;
            GraphVisual Source = (GraphVisual)sender;
            Graph InitGraph = new();
            InitGraph.Visual.MouseUp += InicializeGraphMouseUpHandler;
            for (i = 0; i < Source.Source.InputConnections.Count; i++)
                InitGraph.Visual.AddNewInputGraphPoint();
            for (i = 0; i < Source.Source.OutputConnections.Count; i++)
                InitGraph.Visual.AddNewOutputGraphPoint();
            ScaleTransform Scale = new()
            {
                ScaleX = 0,
                ScaleY = 0
            };
            InitGraph.Visual.RenderTransform = Scale;
            OPLAnimationManager.AnimateTakingZeroTo(ManagerAnimation, Scale, ScaleTransform.ScaleXProperty,
                1d, TimeSpan.FromMilliseconds(400d));
            OPLAnimationManager.AnimateTakingZeroTo(ManagerAnimation, Scale, ScaleTransform.ScaleYProperty,
                1d, TimeSpan.FromMilliseconds(400d));
            Point Current = Source.TranslatePoint(default, CanvasVisualInitItem);
            CanvasVisualInitItem.Children.Add(InitGraph.Visual);
            Canvas.SetLeft(InitGraph.Visual, Current.X);
            Canvas.SetTop(InitGraph.Visual, Current.Y);


            InitGraph.Visual.ActivateMovePosition();
            e.Handled = true;
        }

        private void InicializeGraphMouseUpHandler(object sender, MouseButtonEventArgs e)
        {
            UIElement Source = (UIElement)sender;
            Source.MouseUp -= InicializeGraphMouseUpHandler;
            Point Current = Source.PointToScreen(default);
            Point PointContainer = BorderContainerWindow.PointToScreen(default);
            Vector SourcePoint = Current - PointContainer;
            if (SourcePoint.X >= 0 && SourcePoint.Y >= 0)
            {
                Source.RenderTransform = Transform.Identity;
                CanvasVisualInitItem.Children.Remove(Source);
                CanvasGraph.Children.Add(Source);
                Canvas.SetLeft(Source, SourcePoint.X - Canvas.GetLeft(CanvasGraph));
                Canvas.SetTop(Source, SourcePoint.Y - Canvas.GetTop(CanvasGraph));
            }
            else
            {
                OPLAnimationManager.AnimateTakingZeroTo(ManagerAnimation, (ScaleTransform)Source.RenderTransform, ScaleTransform.ScaleXProperty,
                    0d, TimeSpan.FromMilliseconds(400d));
                OPLAnimationManager.AnimateTakingZeroTo(ManagerAnimation, (ScaleTransform)Source.RenderTransform, ScaleTransform.ScaleYProperty,
                    0d, TimeSpan.FromMilliseconds(400d));
                DoubleAnimation? Animation = ManagerAnimation?.GetCloneAnimationElementFromType<DoubleAnimation>();
                if (Animation != null)
                {
                    Animation.To = 0d;
                    Animation.FillBehavior = FillBehavior.Stop;
                    Animation.Completed += (sender, e) =>
                    {
                        CanvasVisualInitItem.Children.Remove(Source);
                    };
                    Source.BeginAnimation(OpacityProperty, Animation);
                }
                else
                {
                    CanvasVisualInitItem.Children.Remove(Source);
                }
            }
        }
        #endregion

        /// <summary>
        /// Обработчик события установки веса ребру
        /// </summary>
        private void ClickSetWeightGraphHandler(object sender, RoutedEventArgs e)
        {
            //if (GraphOpenedContextMenu?.Source != null)
            //{
            //    GraphConnection Connection = GraphOpenedContextMenu.Source.Connections[int.Parse(TextBoxWeightGraphIndex.Text)];
            //    Connection.Weight = byte.Parse(TextBoxWeightGraph.Text);
            //    RenderConnectionsGraph.TryDoRenderGraphConnection(in Connection);
            //}
        }
    }
}