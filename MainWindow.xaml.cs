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
        public MainWindow()
        {
            InitializeComponent();

            // Инициализация начальных данных
            Station1Coords.Text = "100,200";
            Station2Coords.Text = "400,100";
            Station3Coords.Text = "300,300";

            Distance1.Text = "100";
            Distance2.Text = "100";
            Distance3.Text = "100";

            UpdateStationsAndTriangle();
        }

        private void StationCoords_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Обновление позиций станций и треугольника
            UpdateStationsAndTriangle();
        }
        private void UpdateStationsAndTriangle()
        {
            try
            {
                // Получение координат станций
                var station1Coords = ParseCoords(Station1Coords.Text);
                var station2Coords = ParseCoords(Station2Coords.Text);
                var station3Coords = ParseCoords(Station3Coords.Text);

                // Обновление позиций станций
                UpdateStationPosition(Station1, station1Coords);
                UpdateStationPosition(Station2, station2Coords);
                UpdateStationPosition(Station3, station3Coords);

                // Обновление треугольника
                Triangle.Points = new PointCollection
                {
                    new Point(station1Coords.x, station1Coords.y),
                    new Point(station2Coords.x, station2Coords.y),
                    new Point(station3Coords.x, station3Coords.y)
                };
                // Получение расстояний
                double r1 = double.Parse(Distance1.Text.Trim());
                double r2 = double.Parse(Distance2.Text.Trim());
                double r3 = double.Parse(Distance3.Text.Trim());

                // Вычисление координат объекта
                var (objectX, objectY) = CalculateObjectPosition(
                    station1Coords.x, station1Coords.y, r1,
                    station2Coords.x, station2Coords.y, r2,
                    station3Coords.x, station3Coords.y, r3
                );
                if (IsPointInsideTriangle(objectX, objectY, station1Coords, station2Coords, station3Coords))
                {
                    UpdateStationPosition(ObjectPoint, (objectX, objectY));
                    
                }
            }
            catch
            {
                // Игнорируем ошибки до ввода корректных данных
            }
        }

        private void RecalculateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Получение координат станций
                var station1Coords = ParseCoords(Station1Coords.Text);
                var station2Coords = ParseCoords(Station2Coords.Text);
                var station3Coords = ParseCoords(Station3Coords.Text);

                // Получение расстояний
                double r1 = double.Parse(Distance1.Text.Trim());
                double r2 = double.Parse(Distance2.Text.Trim());
                double r3 = double.Parse(Distance3.Text.Trim());

                // Вычисление координат объекта
                var (objectX, objectY) = CalculateObjectPosition(
                    station1Coords.x, station1Coords.y, r1,
                    station2Coords.x, station2Coords.y, r2,
                    station3Coords.x, station3Coords.y, r3
                );

                // Проверка, находится ли точка внутри треугольника
                if (IsPointInsideTriangle(objectX, objectY, station1Coords, station2Coords, station3Coords))
                {
                    UpdateStationPosition(ObjectPoint, (objectX, objectY));
                    ResultText.Text = $"Координаты объекта: X={objectX:F2}, Y={objectY:F2}";
                }
                else
                {
                    ResultText.Text = "Ошибка: красная точка должна находиться внутри треугольника!";
                }
            }
            catch (Exception ex)
            {
                ResultText.Text = $"Ошибка: {ex.Message}";
            }
        }

        private (double x, double y) ParseCoords(string input)
        {
            var parts = input.Split(',').Select(double.Parse).ToArray();
            return (parts[0], parts[1]);
        }

        private void UpdateStationPosition(FrameworkElement element, (double x, double y) position)
        {
            Canvas.SetLeft(element, position.x - element.Width / 2);
            Canvas.SetTop(element, position.y - element.Height / 2);
        }

        private (double x, double y) CalculateObjectPosition(
            double x1, double y1, double r1,
            double x2, double y2, double r2,
            double x3, double y3, double r3)
        {
            // Решение системы уравнений (триангуляция)
            double A = 2 * (x2 - x1);
            double B = 2 * (y2 - y1);
            double C = r1 * r1 - r2 * r2 - x1 * x1 + x2 * x2 - y1 * y1 + y2 * y2;

            double D = 2 * (x3 - x1);
            double E = 2 * (y3 - y1);
            double F = r1 * r1 - r3 * r3 - x1 * x1 + x3 * x3 - y1 * y1 + y3 * y3;

            double x = (C * E - F * B) / (A * E - D * B);
            double y = (C * D - A * F) / (B * D - A * E);

            return (x, y);
        }
        private bool IsPointInsideTriangle(double px, double py, (double x, double y) t1, (double x, double y) t2, (double x, double y) t3)
        {
            double Area((double x, double y) p1, (double x, double y) p2, (double x, double y) p3) =>
                Math.Abs((p1.x * (p2.y - p3.y) + p2.x * (p3.y - p1.y) + p3.x * (p1.y - p2.y)) / 2.0);

            double fullArea = Area(t1, t2, t3);
            double area1 = Area((px, py), t2, t3);
            double area2 = Area(t1, (px, py), t3);
            double area3 = Area(t1, t2, (px, py));

            return Math.Abs(fullArea - (area1 + area2 + area3)) < 1e-6;
        }
    }
}
