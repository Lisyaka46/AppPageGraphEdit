using AppPageGraphEdit.UserControls;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AppPageGraphEdit.CORE
{
    internal class GraphEdge
    {
        /// <summary>
        /// Начальный граф, от которого исходит соединение
        /// </summary>
        public GraphPoint? Start { get; set; }

        /// <summary>
        /// Конечный граф, к которому приходит соединение
        /// </summary>
        public GraphPoint? End { get; set; }

        /// <summary>
        /// Визуальное представление соединения в виде линии
        /// </summary>
        public GraphEdgeVisual Visual { get; }

        public GraphEdge()
        {
            Visual = new();
            Canvas.SetZIndex(Visual, -1);
        }

        /// <summary>
        /// Обновить отображение линии безье соединения на основе текущих позиций графов и контрольной точки
        /// </summary>
        public Vector GetNewPositionBezierLine()
        {
            if (Start == null && End == null) return new();
            return Visual.GetNewPositionBezierLine(
                Start == null ? Mouse.GetPosition(MainPage.CanvasGraph) : Start.Visual.GetPosition(),
                End == null ? Mouse.GetPosition(MainPage.CanvasGraph) : End.Visual.GetPosition());
        }
    }
}
