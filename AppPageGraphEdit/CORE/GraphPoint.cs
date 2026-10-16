using AppPageGraphEdit.CORE.Render;
using AppPageGraphEdit.UserControls;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AppPageGraphEdit.CORE
{
    /// <summary>
    /// Класс точки подключения графа
    /// </summary>
    public class GraphPoint
    {
        /// <summary>
        /// Визуальный элемент отображения точки подключения
        /// </summary>
        internal readonly GraphPointVisual Visual;

        /// <summary>
        /// Динамический массив подключений к другим графам, исходящих из текущего графа
        /// </summary>
        internal readonly List<GraphEdge> Connections;

        /// <summary>
        /// Динамический массив подключений к другим графам, исходящих из текущего графа (не законченные)
        /// </summary>
        private static readonly List<GraphEdge> PreConnections = [];

        /// <summary>
        /// Пропускная способность
        /// </summary>
        public uint BandWidth { get; set; } = 1000u;

        /// <summary>
        /// Состояние: можно ли создать новое ребро от данной точки подключения
        /// </summary>
        internal bool IsCreateNewPreEdge => !PreConnections.Any((Element) => Element.Start == this);

        /// <summary>
        /// Состояние: можно ли подключить не законченные рёбра в данную точку подключения
        /// </summary>
        internal bool IsConnectPreEdge()
        {
            foreach (GraphEdge Element in PreConnections)
            {
                if (Connections.Any((i) => i.Start == Element.Start && i.End == this))
                    return false;
            }
            return IsUsePreEdge;
        }

        /// <summary>
        /// Состояние: Имеются ли текущие не подключенные рёбра
        /// </summary>
        internal static bool IsUsePreEdge => PreConnections.Count > 0;

        /// <summary>
        /// Инициализация точки подключения графа
        /// </summary>
        /// <param name="SourceBandWidth">Пропускная способность подключения</param>
        internal GraphPoint()
        {
            Connections = [];
            Visual = new(this);
        }

        #region PreEdge
        /// <summary>
        /// Создать новое ребро между точкой подключения (без конечной точки)
        /// </summary>
        internal static GraphEdge CreatePreEdge(GraphPoint Source)
        {
            GraphEdge Result = new()
            {
                Start = Source,
            };
            PreConnections.Add(Result);
            Source.Connections.Add(Result);
            return Result;
        }

        /// <summary>
        /// Проверить, существует ли ребро (без конечной точки) со стартовой точкой в текущем объекте
        /// </summary>
        internal static GraphEdge? CheckPreEdge(GraphPoint Source)
        {
            return PreConnections.FirstOrDefault(i => i.Start == Source);
        }

        /// <summary>
        /// Удалить ребро между точкой подключения (без конечной точки)
        /// </summary>
        internal static void DeletePreEdge(GraphEdge Source)
        {
            MainPage.CanvasGraph.Children.Remove(Source.Visual);
            MainPage.RenderConnectionsGraph.DeleteGraphEdgeRender(Source);
            PreConnections.Remove(Source);
        }

        /// <summary>
        /// Присоеденить все рёбра без конечной точки к текущей точке подключения
        /// </summary>
        internal static void ConnectPreEdges(GraphPoint Source)
        {
            PreConnections.ForEach((i) => i.End = Source);
            Source.Connections.AddRange(PreConnections.AsEnumerable());
            PreConnections.Clear();
        }
        #endregion

        /// <summary>
        /// Обновить визуальное отображение соединений
        /// </summary>
        public void UpdateVisualConnections(out bool Check)
        {
            Check = false;
            if (Connections.Count == 0) return;
            foreach (GraphEdge SourceConnection in Connections)
            {
                if (SourceConnection.Visual.SetPositionBezierLine(SourceConnection.GetNewPositionBezierLine(), true) && !Check)
                    Check = true;
            }
        }
    }
}
