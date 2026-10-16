using AppPageGraphEdit.UserControls;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace AppPageGraphEdit.CORE.Render
{
    /// <summary>
    /// Класс 
    /// </summary>
    internal class RenderConnectionsGraph
    {
        /// <summary>
        /// Массив рёбер, обновляемых постоянно на странице
        /// </summary>
        private List<GraphEdge> Do_ImmuneRenderGraphEdges = [];

        /// <summary>
        /// Массив рёбер, доезжаемых на странице
        /// </summary>
        private List<GraphEdge> Do_RenderGraphEdges = [];

        /// <summary>
        /// Массив рёбер графов, в состоянии окончания подключения (без конечной точки)
        /// </summary>
        private readonly List<GraphEdge> RenderEdgePreConnection = [];

        /// <summary>
        /// Индекс выделенного доезжающего графа
        /// </summary>
        private int Do_RenderGraphsI;

        /// <summary>
        /// Инициализация объекта управления рендерингом графов
        /// </summary>
        /// <param name="Source">Контейнер, в котором будут отображаться графы и их подключения</param>
        internal RenderConnectionsGraph()
        {
        }

        /// <summary>
        /// Запустить рендеринг
        /// </summary>
        internal void RenderStart()
        {
            CompositionTarget.Rendering += UpdateRenderingBezierHandler;
        }

        /// <summary>
        /// Остановить рендеринг
        /// </summary>
        internal void RenderStop()
        {
            CompositionTarget.Rendering -= UpdateRenderingBezierHandler;
        }

        ///// <summary>
        ///// Добавить обновление ребра крафа с учётом наличия обновления подключённых графов
        ///// </summary>
        ///// <param name="Source"></param>
        //internal void TryDoRenderGraphConnection(in GraphConnection Source)
        //{
        //    if (!Do_RenderGraphs.Contains(Source.Start) && !Do_RenderGraphs.Contains(Source.End))
        //        Do_RenderGraphs.Add(Source.Start);
        //}

        #region Events
        /// <summary>
        /// Событие обновления рендеринга, которое вызывается каждый кадр для обработки кривых безье
        /// </summary>
        private void UpdateRenderingBezierHandler(object? sender, EventArgs e)
        {
            if (Do_RenderGraphEdges.Count > 0)
            {
                for (Do_RenderGraphsI = 0; Do_RenderGraphsI < Do_RenderGraphEdges.Count; Do_RenderGraphsI++)
                {
                    if (!Do_RenderGraphEdges[Do_RenderGraphsI].Visual.SetPositionBezierLine(
                        Do_RenderGraphEdges[Do_RenderGraphsI].GetNewPositionBezierLine(), true))
                            Do_RenderGraphEdges.RemoveAt(Do_RenderGraphsI--);
                }
            }

            for (Do_RenderGraphsI = 0; Do_RenderGraphsI < RenderEdgePreConnection.Count; Do_RenderGraphsI++)
            {
                RenderEdgePreConnection[Do_RenderGraphsI].Visual.SetPositionBezierLine(
                    RenderEdgePreConnection[Do_RenderGraphsI].GetNewPositionBezierLine(), true);
                if (RenderEdgePreConnection[Do_RenderGraphsI].Start != null && RenderEdgePreConnection[Do_RenderGraphsI].End != null)
                {
                    Do_RenderGraphEdges.Add(RenderEdgePreConnection[Do_RenderGraphsI]);
                    RenderEdgePreConnection.RemoveAt(Do_RenderGraphsI--);
                }
            }

            for (Do_RenderGraphsI = 0; Do_RenderGraphsI < Do_ImmuneRenderGraphEdges.Count; Do_RenderGraphsI++)
            {
                Do_ImmuneRenderGraphEdges[Do_RenderGraphsI].Visual.SetPositionBezierLine(
                    Do_ImmuneRenderGraphEdges[Do_RenderGraphsI].GetNewPositionBezierLine(), true);
            }
        }

        /// <summary>
        /// Задать рендер текущих рёбер то точки расчёта с автоматическим удалением
        /// </summary>
        /// <param name="SourceEdges">Текущие рёбра</param>
        internal void SetGraphDoEdgesRender(IEnumerable<GraphEdge> SourceEdges)
        {
            Do_RenderGraphEdges.AddRange(SourceEdges.Union(Do_RenderGraphEdges));
        }

        /// <summary>
        /// Задать рендер текущих рёбер
        /// </summary>
        /// <param name="SourceEdges">Текущие рёбра</param>
        internal void SetGraphEdgesRender(IEnumerable<GraphEdge> SourceEdges)
        {
            Do_ImmuneRenderGraphEdges.AddRange(Do_ImmuneRenderGraphEdges.Union(SourceEdges));
        }

        /// <summary>
        /// Удалить рендер текущего ребра
        /// </summary>
        /// <param name="SourceEdge">Текущие ребро</param>
        internal void DeleteGraphEdgeRender(GraphEdge SourceEdge)
        {
            Do_ImmuneRenderGraphEdges.Remove(SourceEdge);
        }

        /// <summary>
        /// Отключить рендер текущих рёбер с доезжанием их до точки покоя
        /// </summary>
        internal void EndGraphEdgesRender()
        {
            Do_RenderGraphEdges.AddRange(Do_ImmuneRenderGraphEdges);
            Do_ImmuneRenderGraphEdges.Clear();
        }

        /// <summary>
        /// Добавить в рендеринг подключаемое ребро без конечной точки
        /// </summary>
        /// <param name="Source">Текущее подключаемое ребро</param>
        internal void AddGraphEdgePreConnection(GraphEdge Source)
        {
            RenderEdgePreConnection.Add(Source);
        }

        ///// <summary>
        ///// Обработчик события отпускания мыши от элемента
        ///// </summary>
        //private void OnMouseUp(object sender, MouseButtonEventArgs e)
        //{
        //    if (DraggingGraph == null) return;
        //    DraggingGraph.UpdateVisualConnections(out Do_DraggingGraphCheck);
        //    DraggingGraph.Visual.ReleaseMouseCapture();
        //    if (Do_DraggingGraphCheck)
        //    {
        //        DraggingGraph.InputConnections.ForEach(i => Do_RenderGraphEdges.AddRange(i.Connections));
        //        DraggingGraph.OutputConnections.ForEach(i => Do_RenderGraphEdges.AddRange(i.Connections));
        //        Do_RenderGraphEdges = [..Do_RenderGraphEdges.Distinct()];
        //    }
        //    DraggingGraph = null;
        //}
        #endregion
    }
}
