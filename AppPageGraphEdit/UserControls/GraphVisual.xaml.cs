using AppPageGraphEdit.CORE;
using IEL.UserElementsControl.Base;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace AppPageGraphEdit.UserControls
{
    /// <summary>
    /// Логика взаимодействия для GraphVisual.xaml
    /// </summary>
    public partial class GraphVisual : IELObjectBase
    {
        /// <summary>
        /// Граф, который визуализируется данным элементом
        /// </summary>
        internal readonly Graph Source;

        #region ContextMenu
        /// <summary>
        /// Контекстное меню управления графами
        /// </summary>
        private ContextMenu ContextMenuGraph;

        /// <summary>
        /// Кнопка отвечающая за добавление новой точки подключения входа
        /// </summary>
        private MenuItem MenuButtonAddNewInputConnection;

        /// <summary>
        /// Кнопка отвечающая за добавление новой точки подключения выхода
        /// </summary>
        private MenuItem MenuButtonAddNewOutputConnection;
        #endregion

        #region Points Control Move Object
        /// <summary>
        /// Стартовая позиция перемещаемого графа
        /// </summary>
        private Point DraggingGraphStartPosition;

        /// <summary>
        /// Текущая позиция перемещаемого графа
        /// </summary>
        private Point DraggingGraphCurrentPosition;
        #endregion

        /// <summary>
        /// Состояние перемещения графа
        /// </summary>
        public bool IsMove { get; private set; }

        /// <summary>
        /// Событие создания новой точки подключения для графа
        /// </summary>
        internal event EventHandler<GraphPoint>? NewGraphPointCreated;

        internal GraphVisual(Graph SourceGraph) : base()
        {
            InitializeComponent();
            Source = SourceGraph;
            SourceBackground.SourceBrush.Opacity = 0.6d;
            MainBorderContainer.Background = SourceBackground.SourceBrush;

            MouseEnter += MouseEnterHandler;
            MouseLeave += MouseLeaveHandler;

            ContextMenuOpening += ContextMenuOpeningHandler;
            ContextMenuClosing += ContextMenuClosingHandler;

            #region ContextMenuGraph
            ContextMenuGraph = new();
            MenuButtonAddNewInputConnection = new()
            {
                Header = "Добавить точку подключения входа",
            };
            MenuButtonAddNewInputConnection.Click += ClickAddInputPointHandler;
            ContextMenuGraph.Items.Add(MenuButtonAddNewInputConnection);
            MenuButtonAddNewOutputConnection = new()
            {
                Header = "Добавить точку подключения выхода",
            };
            MenuButtonAddNewOutputConnection.Click += ClickAddOutputPointHandler;
            ContextMenuGraph.Items.Add(MenuButtonAddNewOutputConnection);
            #endregion

            ContextMenu = ContextMenuGraph;
            MouseLeftButtonDown += OnMouseLeftDown;
            MouseMove += OnMouseMove;
            MouseLeftButtonUp += OnMouseLeftUp;
        }

        internal GraphVisual() : this(new())
        {
            MouseLeftButtonDown -= OnMouseLeftDown;
            MouseMove -= OnMouseMove;
            MouseLeftButtonUp -= OnMouseLeftUp;
            ContextMenuOpening -= ContextMenuOpeningHandler;
            ContextMenuClosing -= ContextMenuClosingHandler;
            ContextMenu = null;
        }

        #region AddNewGraphPoint
        /// <summary>
        /// Добавить новую точку подключения в граф
        /// </summary>
        /// <param name="NewPoint">Добавляемая точка</param>
        /// <param name="InputPosition">
        /// Код позиции точки<br/>
        /// <c>true - Вход</c><br/>
        /// <c>false - Выход</c>
        /// </param>
        private GraphPoint AddNewGraphPoint(bool InputPosition)
        {
            GraphPoint Result = new();
            if (ContextMenu == null) Result.Visual.ContextMenu = null;
            (InputPosition ? ContainerInput : ContainerOutput).Children.Add(Result.Visual);
            (InputPosition ? Source.InputConnections : Source.OutputConnections).Add(Result);
            UpdateLayout();
            MainPage.RenderConnectionsGraph.SetGraphDoEdgesRender(
                (InputPosition ? Source.InputConnections : Source.OutputConnections).Select(i => i.Connections).SelectMany(i => i));
            return Result;
        }

        /// <summary>
        /// Добавить точку подключения к графу во вход
        /// </summary>
        public GraphPoint AddNewInputGraphPoint() => AddNewGraphPoint(true);

        /// <summary>
        /// Добавить точку подключения к графу в выход
        /// </summary>
        public GraphPoint AddNewOutputGraphPoint() => AddNewGraphPoint(false);
        #endregion

        /// <summary>
        /// Принудительно активировать изменение позиции графа
        /// </summary>
        internal void ActivateMovePosition()
        {
            DraggingGraphStartPosition = Mouse.GetPosition(MainPage.CanvasGraph);
            DraggingGraphCurrentPosition = new Point(Canvas.GetLeft(this), Canvas.GetTop(this));
            CaptureMouse();
            IsMove = true;
        }

        #region Events
        /// <summary>
        /// Обработка события наведения мыши на визуальный элемент графа
        /// </summary>
        private void MouseEnterHandler(object sender, MouseEventArgs e)
        {
            SetActiveSpecrum(IEL.CORE.Enums.StateSpectrum.Select);
        }

        /// <summary>
        /// Обработка события снятия мыши с визуального элемента графа
        /// </summary>
        private void MouseLeaveHandler(object sender, MouseEventArgs e)
        {
            SetActiveSpecrum(IEL.CORE.Enums.StateSpectrum.Default);
        }

        /// <summary>
        /// Обработчик события зажатия кнопки мыши на элемент
        /// </summary>
        private void OnMouseLeftDown(object sender, MouseButtonEventArgs e)
        {
            ActivateMovePosition();
            List<GraphPoint> AllPointsGraph = [];
            AllPointsGraph.AddRange(Source.InputConnections);
            AllPointsGraph.AddRange(Source.OutputConnections);
            MainPage.RenderConnectionsGraph.SetGraphEdgesRender(AllPointsGraph.Select((i) => i.Connections).SelectMany(i => i));
            e.Handled = true;
        }

        /// <summary>
        /// Обработка события перемещения мышки на элементе
        /// </summary>
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!IsMove) return;
            Point MouseCurrentPoint = e.GetPosition(MainPage.CanvasGraph);
            Vector Delta = MouseCurrentPoint - DraggingGraphStartPosition;
            Point NewPoint = DraggingGraphCurrentPosition + Delta;
            //Vector CurrentPositionEditContainerNewPoint - new Point(Canvas.GetLeft(MainPage.CanvasGraph), Canvas.GetTop(MainPage.CanvasGraph));

            //// Ограничиваем перемещение в пределах Canvas
            //NewPoint.X = Math.Max(0, Math.Min(NewPoint.X, MainPage.EditContainerGraphs.ActualWidth - ActualWidth));
            //NewPoint.Y = Math.Max(0, Math.Min(NewPoint.Y, MainPage.EditContainerGraphs.ActualHeight - ActualHeight));

            Canvas.SetLeft(this, NewPoint.X);
            Canvas.SetTop(this, NewPoint.Y);
        }

        /// <summary>
        /// Обработчик события отпускания левой мыши от элемента
        /// </summary>
        private void OnMouseLeftUp(object sender, MouseButtonEventArgs e)
        {
            if (!IsMove) return;
            Source.UpdateVisualConnections();
            ReleaseMouseCapture();
            MainPage.RenderConnectionsGraph.EndGraphEdgesRender();
            IsMove = false;
        }

        #region ContextMenu
        private void ContextMenuClosingHandler(object sender, ContextMenuEventArgs e)
        {
        }

        private void ContextMenuOpeningHandler(object sender, ContextMenuEventArgs e)
        {
            if (e.OriginalSource is Border)
            {
            }
        }

        /// <summary>
        /// Обработчик события выделения графа с которого будет начало соединения
        /// </summary>
        private void ClickAddInputPointHandler(object sender, RoutedEventArgs e)
        {
            GraphPoint NewPoint = AddNewInputGraphPoint();
            NewGraphPointCreated?.Invoke(this, NewPoint);
        }

        /// <summary>
        /// Обработчик события выделения графа на который будет нацелено соединение
        /// </summary>
        private void ClickAddOutputPointHandler(object sender, RoutedEventArgs e)
        {
            GraphPoint NewPoint = AddNewOutputGraphPoint();
            NewGraphPointCreated?.Invoke(this, NewPoint);
        }
        #endregion
        #endregion
    }
}
