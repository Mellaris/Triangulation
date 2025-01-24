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
        // Список станций
        private List<(Ellipse Circle, Ellipse Center, double Radius)> stations = new List<(Ellipse, Ellipse, double)>();
        private Ellipse redPoint;
        private Ellipse draggedPoint = null;
        private bool isDragging = false;
        private Point dragStartOffset;

        public MainWindow()
        {
            InitializeComponent();
            redPoint = RedPoint;
            if (redPoint == null)
            {
                MessageBox.Show("Красная точка не инициализирована.");
                return;
            }
            AddStationButton.Click += AddStation;
        }

        // Добавление новой станции
        private void AddStation(object sender, RoutedEventArgs e)
        {
            // Генерация случайных данных для новой станции
            double x = new Random().Next(100, 500);
            double y = new Random().Next(100, 500);
            double r = new Random().Next(100, 200);

            // Создание круга и центра станции
            Ellipse circle = DrawCircle(x, y, r);
            Ellipse center = DrawCenterPoint(x, y);

            // Добавление станции в список
            stations.Add((circle, center, r));

            // Если станций 3 или больше, рассчитываем пересечение
            if (stations.Count >= 3)
            {
                CalculateIntersection();
            }
        }

        // Рисование круга (станции)
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

        // Рисование центра станции
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

        // Расчёт пересечения (если 3 и более станции)
        private void CalculateIntersection()
        {
            if (stations.Count < 3) return;

            var station1 = stations[0];
            var station2 = stations[1];
            var station3 = stations[2];

            double redX = Canvas.GetLeft(redPoint) + redPoint.Width / 2;
            double redY = Canvas.GetTop(redPoint) + redPoint.Height / 2;

            bool isInside = IsPointInsideThreeCircles(
                redX, redY,
                station1.Circle, station1.Radius,
                station2.Circle, station2.Radius,
                station3.Circle, station3.Radius
            );

            // Обновление информации о точке
            if (isInside)
            {
                ErrorDisplay.Text = "Красная точка внутри пересечения.";
                CalculateSignalStrength(redX, redY);  // Рассчитываем силу сигнала
            }
            else
            {
                ErrorDisplay.Text = "Красная точка не в зоне пересечения.";
                SignalStrengthDisplay.Text = "Сила сигнала: Н/Д";
            }

            UpdateCoordinatesDisplay(redX, redY);
        }


        // Проверка, находится ли точка внутри трёх кругов
        private bool IsPointInsideThreeCircles(
            double px, double py,
            Ellipse circle1, double r1,
            Ellipse circle2, double r2,
            Ellipse circle3, double r3)
        {
            double x1 = Canvas.GetLeft(circle1) + r1;
            double y1 = Canvas.GetTop(circle1) + r1;

            double x2 = Canvas.GetLeft(circle2) + r2;
            double y2 = Canvas.GetTop(circle2) + r2;

            double x3 = Canvas.GetLeft(circle3) + r3;
            double y3 = Canvas.GetTop(circle3) + r3;

            return IsPointInsideCircle(px, py, x1, y1, r1) &&
                   IsPointInsideCircle(px, py, x2, y2, r2) &&
                   IsPointInsideCircle(px, py, x3, y3, r3);
        }

        // Проверка, внутри ли точка круга
        private bool IsPointInsideCircle(double px, double py, double cx, double cy, double r)
        {
            double distance = Math.Sqrt(Math.Pow(px - cx, 2) + Math.Pow(py - cy, 2));
            return distance <= r;
        }

        // Обновление отображения координат
        private void UpdateCoordinatesDisplay(double redX, double redY)
        {
            CoordinatesDisplay.Text = $"Координаты: Красная ({redX:F2}, {redY:F2})";
        }

        // Вычисление силы сигнала
        private void CalculateSignalStrength(double redX, double redY)
        {
            double totalSignalStrength = 0;
            double maxSignalStrength = 0;

            foreach (var station in stations)
            {
                double centerX = Canvas.GetLeft(station.Center) + 5;  // Центр чёрной точки
                double centerY = Canvas.GetTop(station.Center) + 5;

                // Расстояние между красной точкой и центром станции
                double distance = Math.Sqrt(Math.Pow(redX - centerX, 2) + Math.Pow(redY - centerY, 2));

                // Сила сигнала: инвертированное расстояние
                double signalStrength = Math.Max(0, (station.Radius - distance) / station.Radius);

                totalSignalStrength += signalStrength;
                maxSignalStrength = Math.Max(maxSignalStrength, signalStrength);
            }

            SignalStrengthDisplay.Text = $"Сила сигнала: {totalSignalStrength:F2} (Макс. сила: {maxSignalStrength:F2})";
        }

        // Обработчик события нажатия на станцию
        private void Station_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            StartDragging(sender as Ellipse, e);
        }

        // Начало перетаскивания
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

        // Обработчик движения мыши
        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isDragging || draggedPoint == null) return;

            Point mousePosition = e.GetPosition(MainCanvas);
            double newX = mousePosition.X - dragStartOffset.X;
            double newY = mousePosition.Y - dragStartOffset.Y;

            if (draggedPoint == redPoint)
            {
                Canvas.SetLeft(draggedPoint, newX);
                Canvas.SetTop(draggedPoint, newY);
                CalculateIntersection();
            }
            else
            {
                // Перемещение центра круга
                Ellipse circle = stations.Find(s => s.Center == draggedPoint).Circle;
                double radius = stations.Find(s => s.Center == draggedPoint).Radius;

                Canvas.SetLeft(draggedPoint, newX);
                Canvas.SetTop(draggedPoint, newY);
                Canvas.SetLeft(circle, newX - radius);
                Canvas.SetTop(circle, newY - radius);
            }
        }


        // Обработчик события отпускания кнопки мыши
        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
            draggedPoint = null;
            Mouse.Capture(null);
        }

        // Обработчик нажатия на красную точку
        private void RedPoint_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            StartDragging(sender as Ellipse, e);
        }
    }
}


