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
            Canvas.SetLeft(redPoint, 300);
            Canvas.SetTop(redPoint, 300);
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
            MainCanvas.Children.Add(circle);
            return circle;
        }

        private Ellipse DrawCenterPoint(double x, double y)
        {
            Ellipse centerPoint = new Ellipse
            {
                Width = 10,
                Height = 10,
                Fill = Brushes.Black,
                Tag = "Station"
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
                Point mousePosition = e.GetPosition(MainCanvas);
                double left = Canvas.GetLeft(station);
                double top = Canvas.GetTop(station);
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
                Point mousePosition = e.GetPosition(MainCanvas);
                double left = Canvas.GetLeft(redPoint);
                double top = Canvas.GetTop(redPoint);
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

            Canvas.SetLeft(draggedPoint, newX);
            Canvas.SetTop(draggedPoint, newY);

            if (draggedPoint == redPoint || draggedPoint.Tag?.ToString() == "Station")
            {
                UpdateStationPosition(draggedPoint);
                CalculateIntersection();
            }
        }
        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isDragging)
            {
                isDragging = false;
                draggedPoint?.ReleaseMouseCapture();
                draggedPoint = null;
            }
        }

        private void UpdateStationPosition(Ellipse station)
        {
            foreach (var (circle, center, radius) in stations)
            {
                if (center == station)
                {
                    double centerX = Canvas.GetLeft(station) + 5;
                    double centerY = Canvas.GetTop(station) + 5;
                    Canvas.SetLeft(circle, centerX - radius);
                    Canvas.SetTop(circle, centerY - radius);
                    break;
                }
            }
        }

        private void CalculateIntersection()
        {
            if (stations.Count < 3)
            {
                IntersectionStatus.Text = "Недостаточно станций для расчёта пересечений.";
                return;
            }

            var station1 = stations[0];
            var station2 = stations[1];
            var station3 = stations[2];

            double x1 = Canvas.GetLeft(station1.Center) + 5;
            double y1 = Canvas.GetTop(station1.Center) + 5;
            double r1 = station1.Radius;

            double x2 = Canvas.GetLeft(station2.Center) + 5;
            double y2 = Canvas.GetTop(station2.Center) + 5;
            double r2 = station2.Radius;

            double x3 = Canvas.GetLeft(station3.Center) + 5;
            double y3 = Canvas.GetTop(station3.Center) + 5;
            double r3 = station3.Radius;

            if (CheckTriangulation(x1, y1, r1, x2, y2, r2, x3, y3, r3, out double redX, out double redY))
            {
                Canvas.SetLeft(redPoint, redX - redPoint.Width / 2);
                Canvas.SetTop(redPoint, redY - redPoint.Height / 2);

                IntersectionStatus.Text = $"Пересечение найдено. ({redX:F2}, {redY:F2})";
            }
            else
            {
                IntersectionStatus.Text = "Пересечения нет.";
            }
        }

        private bool CheckTriangulation(
            double x1, double y1, double r1,
            double x2, double y2, double r2,
            double x3, double y3, double r3,
            out double redX, out double redY)
        {
            redX = 0;
            redY = 0;

            double a = 2 * (x2 - x1);
            double b = 2 * (y2 - y1);
            double c = 2 * (x3 - x1);
            double d = 2 * (y3 - y1);

            double e = r1 * r1 - r2 * r2 - x1 * x1 - y1 * y1 + x2 * x2 + y2 * y2;
            double f = r1 * r1 - r3 * r3 - x1 * x1 - y1 * y1 + x3 * x3 + y3 * y3;

            double denominator = a * d - b * c;
            if (Math.Abs(denominator) < 1e-6)
            {
                return false;
            }

            redX = (e * d - b * f) / denominator;
            redY = (a * f - e * c) / denominator;
            UpdateSignalStrengths(x1, y1, r1, x2, y2, r2, x3, y3, r3);
            return IsPointInsideCircle(redX, redY, x1, y1, r1)
                && IsPointInsideCircle(redX, redY, x2, y2, r2)
                && IsPointInsideCircle(redX, redY, x3, y3, r3);
            
        }
        private void UpdateSignalStrengths(
           double x1, double y1, double r1,
           double x2, double y2, double r2,
           double x3, double y3, double r3)
        {
            double redX = Canvas.GetLeft(redPoint) + redPoint.Width / 2;
            double redY = Canvas.GetTop(redPoint) + redPoint.Height / 2;

            double signal1 = Math.Max(0, (r1 - Math.Sqrt(Math.Pow(redX - x1, 2) + Math.Pow(redY - y1, 2))) / r1) * 100;
            double signal2 = Math.Max(0, (r2 - Math.Sqrt(Math.Pow(redX - x2, 2) + Math.Pow(redY - y2, 2))) / r2) * 100;
            double signal3 = Math.Max(0, (r3 - Math.Sqrt(Math.Pow(redX - x3, 2) + Math.Pow(redY - y3, 2))) / r3) * 100;

            SignalStrength1.Text = $"Сигнал 1: {signal1:F1}%";
            SignalStrength2.Text = $"Сигнал 2: {signal2:F1}%";
            SignalStrength3.Text = $"Сигнал 3: {signal3:F1}%";
        }
        private bool IsPointInsideCircle(double px, double py, double cx, double cy, double radius)
        {
            double distance = Math.Sqrt(Math.Pow(px - cx, 2) + Math.Pow(py - cy, 2));
            return distance <= radius;
        }
    }
}


