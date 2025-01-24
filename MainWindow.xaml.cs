using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TriangulationApp
{
    public partial class MainWindow : Window
    {
        private List<(Ellipse Circle, Ellipse Center, double Radius)> stations = new List<(Ellipse, Ellipse, double)>();
        private Ellipse redPoint;
        private Ellipse draggedPoint = null;
        private bool isDragging = false;
        private Point dragStartOffset;

        public MainWindow()
        {
            InitializeComponent();
            redPoint = RedPoint;
        }

        private void AddStation(object sender, RoutedEventArgs e)
        {
            double x = new Random().Next(100, 500);
            double y = new Random().Next(100, 500);
            double r = new Random().Next(100, 200);

            Ellipse circle = DrawCircle(x, y, r);
            Ellipse center = DrawCenterPoint(x, y);

            stations.Add((circle, center, r));

            CalculateIntersection();
        }

        private Ellipse DrawCircle(double x, double y, double radius)
        {
            Ellipse circle = new Ellipse
            {
                Width = radius * 2,
                Height = radius * 2,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };

            Canvas.SetLeft(circle, x - radius);
            Canvas.SetTop(circle, y - radius);
            circle.MouseLeftButtonDown += Station_MouseLeftButtonDown;
            MainCanvas.Children.Add(circle);

            return circle;
        }

        private Ellipse DrawCenterPoint(double x, double y)
        {
            Ellipse centerPoint = new Ellipse
            {
                Width = 10,
                Height = 10,
                Fill = Brushes.Black
            };

            Canvas.SetLeft(centerPoint, x - 5);
            Canvas.SetTop(centerPoint, y - 5);
            centerPoint.MouseLeftButtonDown += Station_MouseLeftButtonDown;
            MainCanvas.Children.Add(centerPoint);

            return centerPoint;
        }

        private void Station_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Ellipse station)
            {
                isDragging = true;
                draggedPoint = station;

                // Сохраняем начальную позицию мыши относительно элемента
                Point mousePosition = e.GetPosition(MainCanvas);
                double left = Canvas.GetLeft(station);
                double top = Canvas.GetTop(station);

                // Рассчитываем смещение
                dragStartOffset = new Point(mousePosition.X - left, mousePosition.Y - top);

                station.CaptureMouse();
            }
        }

        private void RedPoint_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Ellipse redPoint)
            {
                isDragging = true;
                draggedPoint = redPoint;

                // Сохраняем начальную позицию мыши относительно элемента
                Point mousePosition = e.GetPosition(MainCanvas);
                double left = Canvas.GetLeft(redPoint);
                double top = Canvas.GetTop(redPoint);

                // Рассчитываем смещение
                dragStartOffset = new Point(mousePosition.X - left, mousePosition.Y - top);

                redPoint.CaptureMouse();
            }
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isDragging || draggedPoint == null) return;

            Point mousePosition = e.GetPosition(MainCanvas);
            double newX = mousePosition.X - dragStartOffset.X;
            double newY = mousePosition.Y - dragStartOffset.Y;

            if (draggedPoint.Tag?.ToString() == "Center")
            {
                // Если перетаскиваем черную точку (центр окружности)
                var station = stations.FirstOrDefault(s => s.Center == draggedPoint);
                if (station != null)
                {
                    // Двигаем центр окружности
                    Canvas.SetLeft(station.Center, newX);
                    Canvas.SetTop(station.Center, newY);

                    // Двигаем окружность (радиус)
                    Canvas.SetLeft(station.Circle, newX - station.Radius);
                    Canvas.SetTop(station.Circle, newY - station.Radius);
                }
            }
            else if (draggedPoint == redPoint)
            {
                // Если перетаскиваем красную точку
                Canvas.SetLeft(redPoint, newX);
                Canvas.SetTop(redPoint, newY);
            }
        }


        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
            draggedPoint = null;
            Mouse.Capture(null);

            // После завершения перетаскивания проверяем пересечение и выполняем расчет
            if (stations.Count >= 3 && draggedPoint == redPoint)
            {
                CalculateRedPointPosition();
            }
        }


        private void CalculateIntersection()
        {
            if (stations.Count < 3) return;
            var station1 = stations[0];
            var station2 = stations[1];
            var station3 = stations[2];

            double x1 = Canvas.GetLeft(station1.Center) + 5;
            double y1 = Canvas.GetTop(station1.Center) + 5;

            double x2 = Canvas.GetLeft(station2.Center) + 5;
            double y2 = Canvas.GetTop(station2.Center) + 5;

            double x3 = Canvas.GetLeft(station3.Center) + 5;
            double y3 = Canvas.GetTop(station3.Center) + 5;

            double r1 = station1.Radius;
            double r2 = station2.Radius;
            double r3 = station3.Radius;

            (double redX, double redY, bool isInside) = Triangulate(x1, y1, r1, x2, y2, r2, x3, y3, r3);

            if (isInside)
            {
                Canvas.SetLeft(redPoint, redX - redPoint.Width / 2);
                Canvas.SetTop(redPoint, redY - redPoint.Height / 2);

                CalculateSignalStrength(redX, redY);
                ErrorDisplay.Text = "Красная точка внутри пересечений.";
            }
            else
            {
                ErrorDisplay.Text = "Красная точка вне зоны пересечений.";
            }
        }

        private (double, double, bool) Triangulate(double x1, double y1, double r1, double x2, double y2, double r2, double x3, double y3, double r3)
        {
            double A = 2 * (x2 - x1);
            double B = 2 * (y2 - y1);
            double C = r1 * r1 - r2 * r2 - x1 * x1 - y1 * y1 + x2 * x2 + y2 * y2;

            double D = 2 * (x3 - x1);
            double E = 2 * (y3 - y1);
            double F = r1 * r1 - r3 * r3 - x1 * x1 - y1 * y1 + x3 * x3 + y3 * y3;

            double det = A * E - B * D;
            if (Math.Abs(det) < 1e-6)
            {
                return (0, 0, false);
            }

            double x = (C * E - B * F) / det;
            double y = (A * F - C * D) / det;

            bool isInside = IsPointInsideCircle(x, y, x1, y1, r1) &&
                            IsPointInsideCircle(x, y, x2, y2, r2) &&
                            IsPointInsideCircle(x, y, x3, y3, r3);

            return (x, y, isInside);
        }

        private bool IsPointInsideCircle(double px, double py, double cx, double cy, double r)
        {
            double distance = Math.Sqrt(Math.Pow(px - cx, 2) + Math.Pow(py - cy, 2));
            return distance <= r;
        }

        private void CalculateSignalStrength(double redX, double redY)
        {
            double[] strengths = new double[3];

            for (int i = 0; i < stations.Count; i++)
            {
                var station = stations[i];
                double centerX = Canvas.GetLeft(station.Center) + 5;
                double centerY = Canvas.GetTop(station.Center) + 5;
                double distance = Math.Sqrt(Math.Pow(redX - centerX, 2) + Math.Pow(redY - centerY, 2));
                strengths[i] = Math.Max(0, (station.Radius - distance) / station.Radius);
            }

            SignalStrengthDisplay1.Text = $"Сигнал станции 1: {strengths[0]:F2}";
            SignalStrengthDisplay2.Text = $"Сигнал станции 2: {strengths[1]:F2}";
            SignalStrengthDisplay3.Text = $"Сигнал станции 3: {strengths[2]:F2}";
        }
    }
}


