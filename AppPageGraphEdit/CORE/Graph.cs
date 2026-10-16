using AppPageGraphEdit.UserControls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Shapes;
using Point = System.Windows.Point;

namespace AppPageGraphEdit.CORE
{
    internal class Graph
    {
        /// <summary>
        /// Визуальный элемент графа
        /// </summary>
        public GraphVisual Visual { get; }

        /// <summary>
        /// Точки подключения
        /// </summary>
        internal readonly List<GraphPoint> InputConnections = [];

        /// <summary>
        /// Точки подключения выхода
        /// </summary>
        internal readonly List<GraphPoint> OutputConnections = [];

        public Graph()
        {
            Visual = new(this);
        }

        /// <summary>
        /// Узнать текущую позицию графа
        /// </summary>
        /// <returns>Структура позиции графа внутри контейнера</returns>
        public Point GetPositionGraph() => new((int)Canvas.GetLeft(Visual), (int)Canvas.GetTop(Visual));

        /// <summary>
        /// Обновить визуальное отображение соединений
        /// </summary>
        public void UpdateVisualConnections()
        {
            if (InputConnections.Count == 0 && OutputConnections.Count == 0) return;
            foreach (GraphPoint Point in InputConnections)
            {
                foreach (GraphEdge SourceConnection in Point.Connections)
                {
                    SourceConnection.Visual.SetPositionBezierLine(SourceConnection.GetNewPositionBezierLine(), true);
                }
            }
            foreach (GraphPoint Point in OutputConnections)
            {
                foreach (GraphEdge SourceConnection in Point.Connections)
                {
                    SourceConnection.Visual.SetPositionBezierLine(SourceConnection.GetNewPositionBezierLine(), true);
                }
            }
        }

        /// <summary>
        /// Обновить визуальное отображение соединений
        /// </summary>
        public void UpdateVisualConnections(out bool Check)
        {
            Check = false;
            if (InputConnections.Count == 0 && OutputConnections.Count == 0) return;
            foreach (GraphPoint Point in InputConnections)
            {
                foreach (GraphEdge SourceConnection in Point.Connections)
                {
                    if (SourceConnection.Visual.SetPositionBezierLine(SourceConnection.GetNewPositionBezierLine(), true) && !Check)
                        Check = true;
                }
            }
            foreach (GraphPoint Point in OutputConnections)
            {
                foreach (GraphEdge SourceConnection in Point.Connections)
                {
                    if (SourceConnection.Visual.SetPositionBezierLine(SourceConnection.GetNewPositionBezierLine(), true) && !Check)
                        Check = true;
                }
            }
        }
    }
}
