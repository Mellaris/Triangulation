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
        // Список станций: каждая станция представлена кругом (радиус), центром, радиусом, координатами x и y
        private List<(Ellipse Circle, Ellipse Center, double Radius, double X, double Y)> stations = new List<(Ellipse, Ellipse, double, double, double)>();
        private Ellipse redPoint; // Красная точка
        private Ellipse draggedPoint = null; 
        private bool isDragging = false; 
        private Point dragStartOffset; 
        private List<Line> triangleLines = new List<Line>(); // Линии треугольника

        public MainWindow()
        {
            InitializeComponent();
            redPoint = RedPoint; // Установка красной точки из разметки XAML
            Canvas.SetLeft(redPoint, 300); // Задаем начальное положение красной точки
            Canvas.SetTop(redPoint, 300);
        }
        // Добавление новой станции
        private void AddStation(object sender, RoutedEventArgs e)
        {
            double x = new Random().Next(100, 500);
            double y = new Random().Next(100, 500);
            double r = new Random().Next(100, 200);
            Ellipse circle = DrawCircle(x, y, r);
            Ellipse center = DrawCenterPoint(x, y);
            stations.Add((circle, center, r, x, y));
            // Пересчитываем пересечения
            CalculateIntersection();
        }
        // Создает круг станции
        private Ellipse DrawCircle(double x, double y, double radius)
        {
            Ellipse circle = new Ellipse
            {
                Width = radius * 2,
                Height = radius * 2,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };
            Canvas.SetLeft(circle, x - radius); // Устанавливаем левый отступ
            Canvas.SetTop(circle, y - radius); // Устанавливаем верхний отступ
            MainCanvas.Children.Add(circle); // Добавляем круг 
            return circle;
        }
        // Создает точку центра станции
        private Ellipse DrawCenterPoint(double x, double y)
        {
            Ellipse centerPoint = new Ellipse
            {
                Width = 10,
                Height = 10,
                Fill = Brushes.Black,
                Tag = "Station" // Устанавливаем тег для идентификации станций
            };
            Canvas.SetLeft(centerPoint, x - 5);
            Canvas.SetTop(centerPoint, y - 5);
            // Добавляем обработчик события нажатия
            centerPoint.MouseLeftButtonDown += Station_MouseLeftButtonDown;
            MainCanvas.Children.Add(centerPoint); // Добавляем точку
            return centerPoint;
        }
        // Обработчик начала перетаскивания станции
        private void Station_MouseLeftButtonDown(object stationSender, MouseButtonEventArgs mouseEventArgs)
        {
            if (stationSender is Ellipse station)
            {
                isDragging = true;
                draggedPoint = station;
                Point mousePosition = mouseEventArgs.GetPosition(MainCanvas);
                double left = Canvas.GetLeft(station);
                double top = Canvas.GetTop(station);
                dragStartOffset = new Point(mousePosition.X - left, mousePosition.Y - top);
                station.CaptureMouse();

                if (station.Name != "RedPoint")
                {
                    var selectedStation = stations.FirstOrDefault(s => s.X == left + 5 && s.Y == top + 5);
                    int id = stations.IndexOf(selectedStation);
                    TextBox radiusTextBox = new TextBox
                    {
                        Text = selectedStation.Radius.ToString(),
                        Width = 100,
                        Height = 25,
                        Margin = new Thickness(10)
                    };

                    // Кнопка "Применить изменения"
                    Button applyButton = new Button
                    {
                        Content = "Применить",
                        Width = 100,
                        Height = 30,
                        Margin = new Thickness(10)
                    };

                    // Добавляем обработчик нажатия кнопки
                    applyButton.Click += (sender, e) => ApplyRadiusChange(stations[id], radiusTextBox);

                    // Создаём панель для размещения
                    StackPanel panel = new StackPanel
                    {
                        Children = { radiusTextBox, applyButton }
                    };

                    // Задаём позицию панели в углу экрана
                    Canvas.SetLeft(panel, 10); // Отступ от левого края
                    Canvas.SetTop(panel, 10);  // Отступ от верхнего края

                    
                    MainCanvas.Children.Add(panel);
                }
            }
        }
        // Обработчик начала перетаскивания красной точки
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
                UpdateTriangle();
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
            int stationFromListID = 0;
            foreach (var (circle, center, radius, x, y) in stations)
            {
                if (center == station)
                {
                    stationFromListID = stations.IndexOf((circle, center, radius, x, y));
                    double centerX = Canvas.GetLeft(station) + 5;
                    double centerY = Canvas.GetTop(station) + 5;
                    Canvas.SetLeft(circle, centerX - radius);
                    Canvas.SetTop(circle, centerY - radius);
                    break;
                }
            }
            if (station.Name != "RedPoint")
            {
                var stationFromList = stations[stationFromListID];
                stationFromList.X = Canvas.GetLeft(station) + 5;
                stationFromList.Y = Canvas.GetTop(station) + 5;
                stations[stationFromListID] = stationFromList;
            }
        }

        private void CalculateIntersection()
        {
            if (stations.Count < 3)
            {
                IntersectionStatus.Text = "Недостаточно станций для расчёта пересечений.";
                SignalStrength1.Text = SignalStrength2.Text = SignalStrength3.Text = "";
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

            // Координаты текущей позиции красной точки
            double redX = Canvas.GetLeft(redPoint) + redPoint.Width / 2;
            double redY = Canvas.GetTop(redPoint) + redPoint.Height / 2;

            // Проверяем, находится ли красная точка внутри пересечений
            bool isInside = IsPointInsideCircle(redX, redY, x1, y1, r1) &&
                            IsPointInsideCircle(redX, redY, x2, y2, r2) &&
                            IsPointInsideCircle(redX, redY, x3, y3, r3);

            if (isInside)
            {
                // Если точка внутри, считаем её координаты
                if (CheckTriangulation(x1, y1, r1, x2, y2, r2, x3, y3, r3, out double calculatedX, out double calculatedY))
                {
                    IntersectionStatus.Text = $"Пересечение найдено. ({calculatedX:F2}, {calculatedY:F2})";
                    UpdateSignalStrengths(x1, y1, r1, x2, y2, r2, x3, y3, r3);
                }
                else
                {
                    // Если триангуляция не может быть выполнена
                    IntersectionStatus.Text = "Ошибка при расчёте пересечения.";
                }
            }
            else
            {
                // Если точка вне пересечений, обнуляем данные
                IntersectionStatus.Text = "Красная точка вне пересечений.";
                SignalStrength1.Text = SignalStrength2.Text = SignalStrength3.Text = "";
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
        private void ApplyRadiusChange((Ellipse Circle, Ellipse Center, double Radius, double X, double Y) station, TextBox radiusTextBox)
        {
            // Проверяем, что введено число
            if (double.TryParse(radiusTextBox.Text, out double newRadius) && newRadius > 0)
            {
                // Обновляем радиус в списке станций
                int id = stations.IndexOf(station);
                station.Radius = newRadius;
                stations[id] = station;

                // Обновляем отображение круга радиуса
                UpdateRadiusCircle(stations[id]);
            }
            else
            {
                MessageBox.Show("Введите корректное значение радиуса");
            }
        }
        private void UpdateRadiusCircle((Ellipse Circle, Ellipse Center, double Radius, double X, double Y) station)
        {
            // Находим круг, соответствующий станции
            Ellipse radiusCircle = station.Circle;

            // Обновляем размер круга
            radiusCircle.Width = station.Radius * 2;
            radiusCircle.Height = station.Radius * 2;

            // Обновляем позицию круга
            Canvas.SetLeft(radiusCircle, station.X - station.Radius);
            Canvas.SetTop(radiusCircle, station.Y - station.Radius);
            CalculateIntersection();
        }
        private List<(Ellipse Circle, Ellipse Center, double Radius, double X, double Y)> FindNearestStations()
        {
            double redX = Canvas.GetLeft(RedPoint) + RedPoint.Width / 2;
            double redY = Canvas.GetTop(RedPoint) + RedPoint.Height / 2;

            return stations
                .OrderBy(s => Math.Sqrt(Math.Pow(s.X - redX, 2) + Math.Pow(s.Y - redY, 2)))
                .Take(3)
                .ToList();
        }
        private void UpdateTriangle()
        {
            // Удаляем старые линии
            foreach (var line in triangleLines)
            {
                MainCanvas.Children.Remove(line);
            }
            triangleLines.Clear();

            // Находим три ближайшие станции
            var nearestStations = FindNearestStations();
            if (nearestStations.Count < 3) return;

            // Рисуем треугольник
            for (int i = 0; i < 3; i++)
            {
                var startStation = nearestStations[i];
                var endStation = nearestStations[(i + 1) % 3];

                Line line = new Line
                {
                    X1 = startStation.X,
                    Y1 = startStation.Y,
                    X2 = endStation.X,
                    Y2 = endStation.Y,
                    Stroke = Brushes.Black,
                    StrokeThickness = 2
                };

                triangleLines.Add(line);
                MainCanvas.Children.Add(line);
            }
        }
    }
}


