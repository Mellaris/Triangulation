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
        private List<(Ellipse tower, Ellipse radius, double radiusValue)> towers = new();
        private bool isDragging;
        private UIElement draggedElement;
        private Point mouseOffset;

        public MainWindow()
        {
            InitializeComponent();
            InitializeEvents();
        }

        private void InitializeEvents()
        {
            // События для перетаскивания красной точки
            ObjectPoint.MouseDown += StartDrag;
            MouseMove += Dragging;
            MouseUp += EndDrag;
        }

        private void StartDrag(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;
            draggedElement = sender as UIElement;
            mouseOffset = e.GetPosition(this);
        }

        private void Dragging(object sender, MouseEventArgs e)
        {
            if (isDragging && draggedElement != null)
            {
                var pos = e.GetPosition(this);
                double offsetX = pos.X - mouseOffset.X;
                double offsetY = pos.Y - mouseOffset.Y;

                Canvas.SetLeft(draggedElement, Canvas.GetLeft(draggedElement) + offsetX);
                Canvas.SetTop(draggedElement, Canvas.GetTop(draggedElement) + offsetY);

                mouseOffset = pos;
                UpdateObjectCoordinates();
            }
        }

        private void EndDrag(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
            draggedElement = null;
        }

        private void AddTowerButton_Click(object sender, RoutedEventArgs e)
        {
            // Добавление новой вышки
            var tower = new Ellipse { Width = 20, Height = 20, Fill = Brushes.Black };
            var radius = new Ellipse { Stroke = Brushes.Black, StrokeThickness = 1 };
            double defaultRadius = 100;

            towers.Add((tower, radius, defaultRadius));

            // Расположение новой вышки
            Canvas.SetLeft(tower, 100 + towers.Count * 50);
            Canvas.SetTop(tower, 100 + towers.Count * 50);
            Canvas.SetLeft(radius, 100 + towers.Count * 50 - defaultRadius);
            Canvas.SetTop(radius, 100 + towers.Count * 50 - defaultRadius);
            radius.Width = defaultRadius * 2;
            radius.Height = defaultRadius * 2;

            tower.MouseDown += StartDrag;
            MainCanvas.Children.Add(radius);
            MainCanvas.Children.Add(tower);
        }

        private void UpdateObjectCoordinates()
        {
            // Координаты красной точки
            double objectX = Canvas.GetLeft(ObjectPoint) + ObjectPoint.Width / 2;
            double objectY = Canvas.GetTop(ObjectPoint) + ObjectPoint.Height / 2;

            // Находим пересечение радиусов
            var intersectingTowers = towers
                .Where(t => IsPointInsideCircle(
                    objectX,
                    objectY,
                    Canvas.GetLeft(t.tower) + t.tower.Width / 2,
                    Canvas.GetTop(t.tower) + t.tower.Height / 2,
                    t.radiusValue))
                .ToList();

            if (intersectingTowers.Count >= 3)
            {
                var coords = CalculateTrilateration(
                    GetTowerData(intersectingTowers[0]),
                    GetTowerData(intersectingTowers[1]),
                    GetTowerData(intersectingTowers[2]));

                if (coords != null)
                {
                    CoordinatesMenu.Header = $"Координаты объекта: ({coords.Value.x:F2}, {coords.Value.y:F2})";
                }
                else
                {
                    CoordinatesMenu.Header = "Координаты объекта: Невозможно определить";
                }
            }
            else
            {
                CoordinatesMenu.Header = "Координаты объекта: Невозможно определить";
            }
        }

        private (double x, double y, double r) GetTowerData((Ellipse tower, Ellipse radius, double radiusValue) tower)
        {
            double x = Canvas.GetLeft(tower.tower) + tower.tower.Width / 2;
            double y = Canvas.GetTop(tower.tower) + tower.tower.Height / 2;
            double r = tower.radiusValue;
            return (x, y, r);
        }

        private bool IsPointInsideCircle(double px, double py, double cx, double cy, double r)
        {
            return Math.Pow(px - cx, 2) + Math.Pow(py - cy, 2) <= Math.Pow(r, 2);
        }

        private (double x, double y)? CalculateTrilateration(
            (double x, double y, double r) s1,
            (double x, double y, double r) s2,
            (double x, double y, double r) s3)
        {
            // Формулы триангуляции
            double A = 2 * (s2.x - s1.x);
            double B = 2 * (s2.y - s1.y);
            double C = Math.Pow(s1.r, 2) - Math.Pow(s2.r, 2) - Math.Pow(s1.x, 2) + Math.Pow(s2.x, 2) - Math.Pow(s1.y, 2) + Math.Pow(s2.y, 2);

            double D = 2 * (s3.x - s2.x);
            double E = 2 * (s3.y - s2.y);
            double F = Math.Pow(s2.r, 2) - Math.Pow(s3.r, 2) - Math.Pow(s2.x, 2) + Math.Pow(s3.x, 2) - Math.Pow(s2.y, 2) + Math.Pow(s3.y, 2);

            double denominator = A * E - B * D;

            if (Math.Abs(denominator) < 1e-6) return null;

            double x = (C * E - B * F) / denominator;
            double y = (A * F - C * D) / denominator;

            return (x, y);
        }
    }
}
