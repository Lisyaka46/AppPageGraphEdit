using AppPageGraphEdit.CORE;
using IEL.UserElementsControl.Base;
using OPLAPI.CORE.Animation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AppPageGraphEdit.UserControls
{
    /// <summary>
    /// Логика взаимодействия для GraphEdgeVisual.xaml
    /// </summary>
    public partial class GraphEdgeVisual : IELObjectBase
    {
        #region StrokeThickness
        /// <summary>
        /// Данные свойства <see cref="StrokeThickness"/>
        /// </summary>
        public static readonly DependencyProperty StrokeThicknessProperty = Path.StrokeThicknessProperty;

        /// <summary>
        /// Состояние отвечающее за толщину ребра
        /// </summary>
        [Description("Толщина ребра")]
        public double StrokeThickness
        {
            get => (double)SourceEdge.GetValue(StrokeThicknessProperty);
            set => SourceEdge.SetValue(StrokeThicknessProperty, value);
        }
        #endregion

        /// <summary>
        /// Текущая контрольная точка для кривой Безье, определяющая форму соединения<br/>
        /// </summary>
        private Point CurrentControlPoint;

        /// <summary>
        /// Фигура, определяющая форму соединения, включая начальную точку и сегменты кривой Безье<br/>
        /// </summary>
        private PathFigure Figure { get; }

        /// <summary>
        /// Визуальный элемент, представляющий кривую Безье для соединения<br/>
        /// </summary>
        private QuadraticBezierSegment BezierSegment { get; }

        /// <summary>
        /// Текущее время создания объекта
        /// </summary>
        private DateTime TimeCreated = DateTime.Now;

        /// <summary>
        /// Деление которое соеденение преодаливает за кадр обновления позиции
        /// </summary>
        private readonly double smooth = 0.035d;

        /// <summary>
        /// Текущее состояние анимирования ребра
        /// </summary>
        internal bool IsAnimated { get; private set; }

        /// <summary>
        /// Объект анимирования ребра
        /// </summary>
        private readonly DoubleAnimation AnimationEdge = new()
        {
            Duration = TimeSpan.FromMilliseconds(400d),
        };

        public GraphEdgeVisual()
        {
            InitializeComponent();
            Figure = new();
            Figure.Segments.Add(BezierSegment = new());
            PathGeometry SourceGeometry = new();
            SourceGeometry.Figures.Add(Figure);
            SourceEdge.Stroke = Brushes.Black;
            SourceEdge.StrokeThickness = 2;
            SourceEdge.Data = SourceGeometry;
            Canvas.SetZIndex(SourceEdge, -1);
            CurrentControlPoint = new(0, 0);
        }

        #region Render
        /// <summary>
        /// Установить новую точку покоя относительно точкек начала и конца
        /// </summary>
        /// <param name="Start">Точка начала</param>
        /// <param name="End">Точка конца</param>
        /// <remarks>
        /// Эта функция не подходит для расчёта абсолютной точки покоя<br/>
        /// Предназначена для вычисления начального/стартового значения точки покоя, относительно рендеринга
        /// </remarks>
        internal void SetNewRestingPoint(Point Start, Point End)
        {
            double
                CenterX = (Start.X + End.X) / 3,
                CenterY = (Start.Y + End.Y) / 3;
            CurrentControlPoint = new(CenterX, CenterY);
        }

        /// <summary>
        /// Обновить отображение линии безье соединения на основе текущих позиций графов и контрольной точки
        /// </summary>
        public Vector GetNewPositionBezierLine(Point CurrentPointStart, Point CurrentPointEnd)
        {
            Vector DeltaPosStart = CurrentPointStart - Figure.StartPoint;
            Vector DeltaPosEnd = CurrentPointEnd - BezierSegment.Point2;
            // Смещение позиции
            double DeltaPosTick = Math.Sqrt(Math.Max(DeltaPosStart.LengthSquared, DeltaPosEnd.LengthSquared));

            Figure.StartPoint = CurrentPointStart;
            BezierSegment.Point2 = CurrentPointEnd;

            // Центр провисания
            double
                CenterX = (CurrentPointStart.X + CurrentPointEnd.X) / 2,
                CenterY = (CurrentPointStart.Y + CurrentPointEnd.Y) / 2;

            // Размах
            double ScopeX = CurrentPointEnd.X - CurrentPointStart.X;
            double ScopeY = CurrentPointEnd.Y - CurrentPointStart.Y;

            // Длинна
            double length = Math.Sqrt(ScopeX * ScopeX + ScopeY * ScopeY);

            double sag = 0 + length * 0.08d; // TODO: 0 => Weight

            // Угол направления хорды
            double Angle = Math.Atan2(ScopeY, ScopeX);

            // Точка покоя
            Point RestTarget = new(CenterX, CenterY + sag);

            if (DeltaPosTick > 0.3d)
            {
                double time = (TimeCreated - DateTime.Now).TotalMilliseconds;
                // Амплитуда раскачивания
                double Amplitude = Math.Min(45d, DeltaPosTick * 3.3d);

                // Максимальная раскачка
                double MaxIntensity = Math.Min(1d, DeltaPosTick) + 1;

                // Сила продольного качания
                double SagSwing = Math.Sin(time) * Amplitude;

                // Маятниковое качание
                double PendulumSwing = SagSwing * 2.8d;

                // Проекция маятника на перпендикуляр
                Vector PendulumPerpendicular = new(-Math.Sin(Angle), Math.Cos(Angle));
                Vector PendulumPerpendicularOffset = PendulumPerpendicular * PendulumSwing;

                // Колебание по координатам
                double HorizontalWave = Math.Sin(time) * (Amplitude * 3) * MaxIntensity;

                RestTarget.X = CenterX + PendulumPerpendicularOffset.X + HorizontalWave;
                RestTarget.Y = CenterY + sag + SagSwing + PendulumPerpendicularOffset.Y;

                if (DeltaPosTick > 1d)
                {
                    // Амплитуда хлыста
                    double AplitudeWhip = Math.Sin(time) * Amplitude;
                    RestTarget.X += Math.Cos(Angle) * AplitudeWhip;
                    RestTarget.Y += Math.Sin(Angle) * AplitudeWhip;
                }
            }

            return CurrentControlPoint == default ?
                (Vector)RestTarget : new(RestTarget.X - CurrentControlPoint.X, RestTarget.Y - CurrentControlPoint.Y);
        }

        /// <summary>
        /// Обновить текущую позицию точек кривой Безье
        /// </summary>
        /// <param name="DeltaPos">Точка стремления</param>
        /// <param name="SmoothUse">Использование деления на кадры</param>
        /// <returns>Состояние обновилась или нет</returns>
        internal bool SetPositionBezierLine(Vector DeltaPos, bool SmoothUse)
        {
            if (Math.Abs(DeltaPos.X) < 1.15d && Math.Abs(DeltaPos.Y) < 1.15d)
                return false;
            CurrentControlPoint += SmoothUse ? (DeltaPos * smooth) : DeltaPos;
            BezierSegment.Point1 = CurrentControlPoint;
            return true;
        }
        #endregion

        /// <summary>
        /// Включить анимирование ребра
        /// </summary>
        internal void OnAnimate()
        {
            SourceEdge.StrokeDashArray.Add(3);
            SourceEdge.StrokeDashOffset = 0;

            AnimationEdge.From = 0;
            AnimationEdge.To = -6.24d;
            AnimationEdge.RepeatBehavior = RepeatBehavior.Forever;
            SourceEdge.BeginAnimation(Path.StrokeDashOffsetProperty, AnimationEdge);
            
            IsAnimated = true;
        }

        /// <summary>
        /// Выключить анимирование ребра
        /// </summary>
        internal void OffAnimate()
        {
            SourceEdge.BeginAnimation(Path.StrokeDashOffsetProperty, null);
            SourceEdge.StrokeDashArray.Clear();
            SourceEdge.StrokeDashOffset = 0;

            IsAnimated = false;
        }
    }
}
