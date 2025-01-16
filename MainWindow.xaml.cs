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
            InitializeDefaultValues();
        }

        private void InitializeDefaultValues()
        {
            Station1Coords.Text = "100,200";
            Station2Coords.Text = "400,100";
            Station3Coords.Text = "300,300";
            Distance1.Text = "100";
            Distance2.Text = "150";
            Distance3.Text = "200";
            Radius1.Text = "200";
            Radius2.Text = "200";
            Radius3.Text = "200";
            UpdateVisualization(null, null);
        }

        private void UpdateVisualization(object sender, TextChangedEventArgs e)
        {
            try
            {
                MapCanvas.Children.Clear();

                var station1 = ParseCoords(Station1Coords.Text);
                var station2 = ParseCoords(Station2Coords.Text);
                var station3 = ParseCoords(Station3Coords.Text);

                double r1 = double.Parse(Distance1.Text);
                double r2 = double.Parse(Distance2.Text);
                double r3 = double.Parse(Distance3.Text);
                double radius1 = double.Parse(Radius1.Text);
                double radius2 = double.Parse(Radius2.Text);
                double radius3 = double.Parse(Radius3.Text);

                DrawStationWithRadius(station1, radius1, Brushes.Blue);
                DrawStationWithRadius(station2, radius2, Brushes.Green);
                DrawStationWithRadius(station3, radius3, Brushes.Orange);

                DrawTriangle(station1, station2, station3);

                if (!ValidateDistances(r1, r2, r3, station1, station2, station3))
                {
                    ResultText.Text = "Невозможно найти точку с такими расстояниями.";
                    return;
                }

                var (objectX, objectY) = CalculateObjectPosition(
                    station1.x, station1.y, r1,
                    station2.x, station2.y, r2,
                    station3.x, station3.y, r3
                );

                DrawObjectPoint(objectX, objectY, Brushes.Red);

                ResultText.Text = $"Координаты объекта: X={objectX:F2}, Y={objectY:F2}";
            }
            catch
            {
                ResultText.Text = "Ошибка ввода данных.";
            }
        }

        private (double x, double y) ParseCoords(string input)
        {
            var parts = input.Split(',');
            return (double.Parse(parts[0]), double.Parse(parts[1]));
        }

        private void DrawStationWithRadius((double x, double y) station, double radius, Brush color)
        {
            var ellipse = new Ellipse
            {
                Width = radius * 2,
                Height = radius * 2,
                Stroke = color,
                StrokeThickness = 1
            };
            Canvas.SetLeft(ellipse, station.x - radius);
            Canvas.SetTop(ellipse, station.y - radius);
            MapCanvas.Children.Add(ellipse);

            var stationDot = new Ellipse
            {
                Width = 10,
                Height = 10,
                Fill = Brushes.Black
            };
            Canvas.SetLeft(stationDot, station.x - 5);
            Canvas.SetTop(stationDot, station.y - 5);
            MapCanvas.Children.Add(stationDot);
        }

        private void DrawTriangle((double x, double y) s1, (double x, double y) s2, (double x, double y) s3)
        {
            var triangle = new Polygon
            {
                Stroke = Brushes.Gray,
                StrokeThickness = 1,
                Fill = Brushes.Transparent,
                Points = new PointCollection
                {
                    new Point(s1.x, s1.y),
                    new Point(s2.x, s2.y),
                    new Point(s3.x, s3.y)
                }
            };
            MapCanvas.Children.Add(triangle);
        }

        private void DrawObjectPoint(double x, double y, Brush color)
        {
            var point = new Ellipse
            {
                Width = 10,
                Height = 10,
                Fill = color
            };
            Canvas.SetLeft(point, x - 5);
            Canvas.SetTop(point, y - 5);
            MapCanvas.Children.Add(point);
        }

        private bool ValidateDistances(double r1, double r2, double r3, (double x, double y) s1, (double x, double y) s2, (double x, double y) s3)
        {
            double d1 = GetDistance(s1, s2);
            double d2 = GetDistance(s2, s3);
            double d3 = GetDistance(s1, s3);
            return r1 + r2 >= d1 && r2 + r3 >= d2 && r1 + r3 >= d3;
        }

        private double GetDistance((double x, double y) p1, (double x, double y) p2)
        {
            return Math.Sqrt(Math.Pow(p1.x - p2.x, 2) + Math.Pow(p1.y - p2.y, 2));
        }

        private (double x, double y) CalculateObjectPosition(double x1, double y1, double r1, double x2, double y2, double r2, double x3, double y3, double r3)
        {
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
    }
}

