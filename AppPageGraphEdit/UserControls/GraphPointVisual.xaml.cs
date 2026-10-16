using AppPageGraphEdit.CORE;
using IEL.UserElementsControl.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AppPageGraphEdit.UserControls
{
    /// <summary>
    /// Логика взаимодействия для GraphPointVisual.xaml
    /// </summary>
    public partial class GraphPointVisual : IELObjectBase
    {
        #region StrokeThickness
        /// <summary>
        /// Данные свойства <see cref="StrokeThickness"/>
        /// </summary>
        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.Register(nameof(StrokeThickness), typeof(double), typeof(IELButtonBase),
                new(2d));

        /// <summary>
        /// Состояние отвечающее за возможность активации кнопки с помощью мыши
        /// </summary>
        [Description("Толщина границ точки подключения")]
        public double StrokeThickness
        {
            get => (double)GetValue(StrokeThicknessProperty);
            set => SetValue(StrokeThicknessProperty, value);
        }
        #endregion

        #region ContextMenu
        /// <summary>
        /// Контекстное меню управления
        /// </summary>
        private ContextMenu SourceContextMenu;

        /// <summary>
        /// Кнопка отвечающая за создание начальной точки ребра
        /// </summary>
        private MenuItem MenuButtonAddNewEdge;

        /// <summary>
        /// Кнопка отвечающая за присоеденение к конечной точке рёбра
        /// </summary>
        /// <remarks>
        /// Подразумевается что кнопка будет присоединять к текущей точке все рёбра без конечной точки
        /// </remarks>
        private MenuItem MenuButtonEndConnectEdge;

        /// <summary>
        /// Кнопка отвечающая за анимирование рёбер
        /// </summary>
        private MenuItem MenuButtonAnimationEdge;
        #endregion

        /// <summary>
        /// Визуальный элемент отображения точки подключения
        /// </summary>
        private readonly GraphPoint Source;

        public GraphPointVisual(GraphPoint SourcePoint)
        {
            InitializeComponent();
            SourceEllipse.Stroke = SourceBorderBrush.SourceBrush;
            SourceEllipse.Fill = SourceBackground.SourceBrush;

            #region ContextMenu
            SourceContextMenu = new();
            MenuButtonAddNewEdge = new()
            {
                Header = "Создать ребро",
            };
            MenuButtonAddNewEdge.Click += AddNewEdgeHandler;
            SourceContextMenu.Items.Add(MenuButtonAddNewEdge);

            MenuButtonEndConnectEdge = new()
            {
                Header = "Соеденить рёбра",
            };
            MenuButtonEndConnectEdge.Click += EndConnectEdgeHandler;
            SourceContextMenu.Items.Add(MenuButtonEndConnectEdge);

            MenuButtonAnimationEdge = new()
            {
                Header = "Изменить состояние анимирования рёбер",
            };
            MenuButtonAnimationEdge.Click += AnimationEdgeHandler;
            SourceContextMenu.Items.Add(MenuButtonAnimationEdge);
            #endregion
            ContextMenu = SourceContextMenu;
            ContextMenuOpening += ContextMenuOpeningHandler;
            ContextMenuClosing += ContextMenuClosingHandler;

            MouseEnter += MouseEnterHandler;
            MouseLeave += MouseLeaveHandler;
            MouseDown += MouseDownHandler;
            MouseUp += MouseUpHandler;
            MouseDoubleClick += MouseDoubleClickHandler;
            Source = SourcePoint;
        }

        /// <summary>
        /// Создать новое ребро без конечной точки с привязкой к рендеренгу
        /// </summary>
        private void CreatePreEdge()
        {
            GraphEdge NewEdge = GraphPoint.CreatePreEdge(Source);
            NewEdge.Visual.SetNewRestingPoint(GetPosition(), Mouse.GetPosition(Mouse.Captured));
            MainPage.CanvasGraph.Children.Add(NewEdge.Visual);
            MainPage.RenderConnectionsGraph.AddGraphEdgePreConnection(NewEdge);
        }

        #region Events
        #region ContextMenu
        /// <summary>
        /// Обработчик события включения/отключения анимирования ребра
        /// </summary>
        private void AnimationEdgeHandler(object sender, RoutedEventArgs e)
        {
            if (Source.Connections.Any((i) => i.Visual.IsAnimated))
                Source.Connections.ForEach((i) => i.Visual.OffAnimate());
            else
                Source.Connections.ForEach((i) => i.Visual.OnAnimate());
        }

        /// <summary>
        /// Обработчик события добавления нового ребра
        /// </summary>
        private void AddNewEdgeHandler(object sender, RoutedEventArgs e)
        {
            CreatePreEdge();
        }

        /// <summary>
        /// Обработчик события присоединения рёбер к точке подключения
        /// </summary>
        private void EndConnectEdgeHandler(object sender, RoutedEventArgs e)
        {
            GraphPoint.ConnectPreEdges(Source);
        }
        #endregion

        /// <summary>
        /// Обработчик события наведения курсора мыши
        /// </summary>
        private void MouseEnterHandler(object sender, MouseEventArgs e)
        {
            if (ContextMenu != null)
            {
                SetActiveSpecrum(IEL.CORE.Enums.StateSpectrum.Select);
            }
            e.Handled = true;
        }

        /// <summary>
        /// Обработчик события отведения курсора мыши
        /// </summary>
        private void MouseLeaveHandler(object sender, MouseEventArgs e)
        {
            if (ContextMenu != null)
            {
                SetActiveSpecrum(IEL.CORE.Enums.StateSpectrum.Default);
            }
            e.Handled = true;
        }

        /// <summary>
        /// Обработчик события нажатия мыши
        /// </summary>
        private void MouseDownHandler(object sender, MouseEventArgs e)
        {
            if (ContextMenu != null)
            {
                SetActiveSpecrum(IEL.CORE.Enums.StateSpectrum.Used);
            }
        }

        /// <summary>
        /// Обработчик события отпускания мыши
        /// </summary>
        private void MouseUpHandler(object sender, MouseEventArgs e)
        {
            if (ContextMenu != null)
            {
                SetActiveSpecrum(IEL.CORE.Enums.StateSpectrum.Select);
            }
        }

        /// <summary>
        /// Событие двойного нажатия на точку подключения
        /// </summary>
        private void MouseDoubleClickHandler(object sender, MouseButtonEventArgs e)
        {
            if (GraphPoint.CheckPreEdge(Source) is GraphEdge Element)
                GraphPoint.DeletePreEdge(Element);
            else CreatePreEdge();
        }

        /// <summary>
        /// Обработчик события открытия контекстного меню точки подключения
        /// </summary>
        private void ContextMenuOpeningHandler(object sender, ContextMenuEventArgs e)
        {
            if (e.OriginalSource is Ellipse)
            {
                MenuButtonAddNewEdge.IsEnabled = Source.IsCreateNewPreEdge;
                MenuButtonEndConnectEdge.IsEnabled = MenuButtonAddNewEdge.IsEnabled && Source.IsConnectPreEdge();
                //MenuButtonAnimationEdge.Header = Connections.Any((i) => i.Visual.IsAnimated) ? "Отключить анимирование" : "Включить анимирование";
                return;
            }
        }

        /// <summary>
        /// Обработчик события закрытия контекстного меню точки подключения
        /// </summary>
        private void ContextMenuClosingHandler(object sender, ContextMenuEventArgs e)
        {
        }
        #endregion

        /// <summary>
        /// Узнать текущую позицию точки подключения графа
        /// </summary>
        /// <returns>Структура позиции точки подключени графа внутри контейнера</returns>
        public Point GetPosition() => TranslatePoint(new Point(ActualWidth / 2d, ActualHeight / 2d), MainPage.CanvasGraph);
    }
}
