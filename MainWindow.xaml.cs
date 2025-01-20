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
        private double tower1Radius = 100, tower2Radius = 100, tower3Radius = 100;

        public MainWindow()
        {
            InitializeComponent();
            InitializeObjects();
        }

        private void InitializeObjects()
        {
            // Устанавливаем начальные координаты
            SetTowerPosition(Tower1, Radius1, 100, 100, tower1Radius);
            SetTowerPosition(Tower2, Radius2, 300, 100, tower2Radius);
            SetTowerPosition(Tower3, Radius3, 200, 300, tower3Radius);
            SetObjectPosition(ObjectPoint, 200, 200);

            // Добавляем обработчики событий
            Tower1.MouseMove += Tower_MouseMove;
            Tower2.MouseMove += Tower_MouseMove;
            Tower3.MouseMove += Tower_MouseMove;
            ObjectPoint.MouseMove += Object_MouseMove;
        }

        private void SetTowerPosition(Ellipse tower, Ellipse radius, double x, double y, double r)
        {
            Canvas.SetLeft(tower, x - tower.Width / 2);
            Canvas.SetTop(tower, y - tower.Height / 2);
            Canvas.SetLeft(radius, x - r);
            Canvas.SetTop(radius, y - r);
            radius.Width = radius.Height = r * 2;
        }
        private void SetObjectPosition(Ellipse obj, double x, double y)
        {
            Canvas.SetLeft(obj, x - obj.Width / 2);
            Canvas.SetTop(obj, y - obj.Height / 2);
        }

        private void Tower_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var tower = sender as Ellipse;
                var pos = e.GetPosition(MapCanvas);
                var radius = tower == Tower1 ? Radius1 : tower == Tower2 ? Radius2 : Radius3;
                SetTowerPosition(tower, radius, pos.X, pos.Y, GetRadiusForTower(tower));
                UpdateObjectCoordinates();
                UpdateMenu();
            }
        }

        private void Object_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var pos = e.GetPosition(MapCanvas);
                SetObjectPosition(ObjectPoint, pos.X, pos.Y);
                UpdateObjectCoordinates();
                UpdateMenu();
            }
        }

        private double GetRadiusForTower(Ellipse tower)
        {
            return tower == Tower1 ? tower1Radius : tower == Tower2 ? tower2Radius : tower3Radius;
        }

        private void RadiusInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                double newRadius;

                if (!double.TryParse(textBox.Text, out newRadius) || newRadius < 0)
                {
                    textBox.Background = Brushes.LightCoral;
                    return;
                }

                textBox.Background = Brushes.White;

                if (textBox.Name == "Radius1Input")
                {
                    tower1Radius = newRadius;
                    SetTowerPosition(Tower1, Radius1, Canvas.GetLeft(Tower1) + Tower1.Width / 2, Canvas.GetTop(Tower1) + Tower1.Height / 2, newRadius);
                }
                else if (textBox.Name == "Radius2Input")
                {
                    tower2Radius = newRadius;
                    SetTowerPosition(Tower2, Radius2, Canvas.GetLeft(Tower2) + Tower2.Width / 2, Canvas.GetTop(Tower2) + Tower2.Height / 2, newRadius);
                }
                else if (textBox.Name == "Radius3Input")
                {
                    tower3Radius = newRadius;
                    SetTowerPosition(Tower3, Radius3, Canvas.GetLeft(Tower3) + Tower3.Width / 2, Canvas.GetTop(Tower3) + Tower3.Height / 2, newRadius);
                }

                UpdateObjectCoordinates();
                UpdateMenu();
            }
        }

        private void UpdateObjectCoordinates()
        {
            double x1 = Canvas.GetLeft(Tower1) + Tower1.Width / 2;
            double y1 = Canvas.GetTop(Tower1) + Tower1.Height / 2;
            double x2 = Canvas.GetLeft(Tower2) + Tower2.Width / 2;
            double y2 = Canvas.GetTop(Tower2) + Tower2.Height / 2;
            double x3 = Canvas.GetLeft(Tower3) + Tower3.Width / 2;
            double y3 = Canvas.GetTop(Tower3) + Tower3.Height / 2;

            double objX = Canvas.GetLeft(ObjectPoint) + ObjectPoint.Width / 2;
            double objY = Canvas.GetTop(ObjectPoint) + ObjectPoint.Height / 2;

            if (IsPointInsideCircle(objX, objY, x1, y1, tower1Radius) &&
                IsPointInsideCircle(objX, objY, x2, y2, tower2Radius) &&
                IsPointInsideCircle(objX, objY, x3, y3, tower3Radius))
            {
                CoordinatesMenu.Header = $"Координаты объекта: ({objX:F2}, {objY:F2})";
            }
            else
            {
                CoordinatesMenu.Header = "Координаты объекта: Невозможно определить";
            }
        }

        private bool IsPointInsideCircle(double x, double y, double cx, double cy, double r)
        {
            return Math.Pow(x - cx, 2) + Math.Pow(y - cy, 2) <= Math.Pow(r, 2);
        }
        private void UpdateMenu()
        {
            Tower1Menu.Header = $"Вышка 1: Радиус = {tower1Radius}, Координаты = ({Canvas.GetLeft(Tower1) + Tower1.Width / 2:F2}, {Canvas.GetTop(Tower1) + Tower1.Height / 2:F2})";
            Tower2Menu.Header = $"Вышка 2: Радиус = {tower2Radius}, Координаты = ({Canvas.GetLeft(Tower2) + Tower2.Width / 2:F2}, {Canvas.GetTop(Tower2) + Tower2.Height / 2:F2})";
            Tower3Menu.Header = $"Вышка 3: Радиус = {tower3Radius}, Координаты = ({Canvas.GetLeft(Tower3) + Tower3.Width / 2:F2}, {Canvas.GetTop(Tower3) + Tower3.Height / 2:F2})";
        }
    }
}
