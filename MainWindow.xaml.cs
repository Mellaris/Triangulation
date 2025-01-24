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
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Ellipse> circles = new List<Ellipse>();
        private List<(double X, double Y, double R)> stations = new List<(double X, double Y, double R)>();
        private Ellipse redPoint;
        private Ellipse draggedPoint = null;
        private bool isDragging = false;
        private Point dragStartOffset;

        public MainWindow()
        {
            InitializeComponent();
            InitializeScene();
        }

        private void InitializeScene()
        {
            redPoint = RedPoint;
            AddStationButton.Click += AddStation;
        }

        private void AddStation(object sender, RoutedEventArgs e)
        {
            // Создание новой станции
            double x = new Random().Next(100, 500);
            double y = new Random().Next(100, 500);
            double r = new Random().Next(100, 200);

            Ellipse circle = DrawCircle(x, y, r);
            Ellipse centerPoint = DrawCenterPoint(x, y);

            circles.Add(circle);
            stations.Add((x, y, r));

            if (stations.Count >= 3)
            {
                CalculateIntersection();
            }
        }

        private Ellipse DrawCircle(double x, double y, double radius)
        {
            Ellipse circle = new Ellipse
            {
                Width = radius * 2,
                Height = radius * 2,
                Stroke = Brushes.Black,
                StrokeThickness = 1,
                Tag = "Circle"
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
            MainCanvas.Children.Add(centerPoint);

            return centerPoint;
        }

        private void CalculateIntersection()
        {
            if (stations.Count < 3) return;

            var station1 = stations[0];
            var station2 = stations[1];
            var station3 = stations[2];

            var intersection = GetIntersectionPoint(station1.X, station1.Y, station1.R,
                                                     station2.X, station2.Y, station2.R,
                                                     station3.X, station3.Y, station3.R);

            if (intersection == null)
            {
                ErrorDisplay.Text = "Красная точка не в зоне пересечения.";
                return;
            }

            double redX = intersection.Value.X;
            double redY = intersection.Value.Y;

            UpdateCoordinatesDisplay(redX, redY);
            ErrorDisplay.Text = "Ошибок нет.";
        }

        private (double X, double Y)? GetIntersectionPoint(
            double x1, double y1, double r1,
            double x2, double y2, double r2,
            double x3, double y3, double r3)
        {
            double a = 2 * (x2 - x1);
            double b = 2 * (y2 - y1);
            double c = Math.Pow(r1, 2) - Math.Pow(r2, 2) - Math.Pow(x1, 2) + Math.Pow(x2, 2) - Math.Pow(y1, 2) + Math.Pow(y2, 2);

            double d = 2 * (x3 - x2);
            double e = 2 * (y3 - y2);
            double f = Math.Pow(r2, 2) - Math.Pow(r3, 2) - Math.Pow(x2, 2) + Math.Pow(x3, 2) - Math.Pow(y2, 2) + Math.Pow(y3, 2);

            double determinant = a * e - b * d;
            if (Math.Abs(determinant) < 1e-6) return null;

            double px = (c * e - b * f) / determinant;
            double py = (a * f - c * d) / determinant;

            return (px, py);
        }

        private void UpdateCoordinatesDisplay(double redX, double redY)
        {
            CoordinatesDisplay.Text = $"Координаты: Красная ({redX:F2}, {redY:F2})";
            for (int i = 0; i < stations.Count; i++)
            {
                CoordinatesDisplay.Text += $", Станция {i + 1} ({stations[i].X:F2}, {stations[i].Y:F2})";
            }
        }

        private void RedPoint_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            StartDragging(sender as Ellipse, e);
        }

        private void Station_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            StartDragging(sender as Ellipse, e);
        }

        private void StartDragging(Ellipse point, MouseButtonEventArgs e)
        {
            if (point == null) return;

            draggedPoint = point;
            isDragging = true;

            dragStartOffset = e.GetPosition(MainCanvas);
            dragStartOffset.X -= Canvas.GetLeft(point);
            dragStartOffset.Y -= Canvas.GetTop(point);

            Mouse.Capture(point);
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isDragging || draggedPoint == null) return;

            Point mousePosition = e.GetPosition(MainCanvas);
            Canvas.SetLeft(draggedPoint, mousePosition.X - dragStartOffset.X);
            Canvas.SetTop(draggedPoint, mousePosition.Y - dragStartOffset.Y);

            if (draggedPoint == redPoint)
            {
                CalculateIntersection();
            }
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
            draggedPoint = null;
            Mouse.Capture(null);
        }
    }
}
