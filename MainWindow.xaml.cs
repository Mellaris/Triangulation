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
        private bool isDraggingRedPoint = false; // Перемещение красной точки
        private Point dragStartPoint;

        public MainWindow()
        {
            InitializeComponent();
            InitializeEvents();
            InitializeDefaultPositions();
        }

        private void InitializeEvents()
        {
            // События для вышек
            Tower1.MouseMove += Tower_MouseMove;
            Tower2.MouseMove += Tower_MouseMove;
            Tower3.MouseMove += Tower_MouseMove;

            // События для красной точки
            ObjectPoint.MouseLeftButtonDown += RedPoint_MouseDown;
            ObjectPoint.MouseLeftButtonUp += RedPoint_MouseUp;
            ObjectPoint.MouseMove += RedPoint_MouseMove;
        }

        private void InitializeDefaultPositions()
        {
            // Установка начальных позиций для вышек и радиусов
            SetPosition(Tower1, 100, 100);
            SetPosition(Tower2, 300, 100);
            SetPosition(Tower3, 200, 300);

            SetRadius(Radius1, Tower1, 100);
            SetRadius(Radius2, Tower2, 100);
            SetRadius(Radius3, Tower3, 100);

            // Установка начальной позиции красной точки
            SetPosition(ObjectPoint, 200, 200);

            // Обновление меню
            UpdateMenu();
        }

        private void SetPosition(UIElement element, double x, double y)
        {
            Canvas.SetLeft(element, x - element.RenderSize.Width / 2);
            Canvas.SetTop(element, y - element.RenderSize.Height / 2);
        }

        private void SetRadius(Ellipse radius, Ellipse tower, double value)
        {
            radius.Width = value * 2;
            radius.Height = value * 2;
            Canvas.SetLeft(radius, Canvas.GetLeft(tower) - value + tower.Width / 2);
            Canvas.SetTop(radius, Canvas.GetTop(tower) - value + tower.Height / 2);
        }

        private void Tower_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var tower = sender as Ellipse;
                var position = e.GetPosition(MapCanvas);

                // Установка новых координат вышки
                SetPosition(tower, position.X, position.Y);

                // Обновление радиуса
                Ellipse radius = GetAssociatedRadius(tower);
                SetRadius(radius, tower, radius.Width / 2);

                // Проверка положения красной точки
                UpdateObjectCoordinates();
            }
        }

        private Ellipse GetAssociatedRadius(Ellipse tower)
        {
            if (tower == Tower1) return Radius1;
            if (tower == Tower2) return Radius2;
            return Radius3;
        }

        private void RedPoint_MouseDown(object sender, MouseButtonEventArgs e)
        {
            isDraggingRedPoint = true;
            dragStartPoint = e.GetPosition(MapCanvas);
            ObjectPoint.CaptureMouse();
        }

        private void RedPoint_MouseUp(object sender, MouseEventArgs e)
        {
            isDraggingRedPoint = false;
            ObjectPoint.ReleaseMouseCapture();
            UpdateObjectCoordinates();
        }

        private void RedPoint_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDraggingRedPoint)
            {
                var position = e.GetPosition(MapCanvas);

                // Перемещение красной точки
                SetPosition(ObjectPoint, position.X, position.Y);
            }
        }

        private void UpdateObjectCoordinates()
        {
            // Координаты вышек
            var t1 = GetTowerCenter(Tower1);
            var t2 = GetTowerCenter(Tower2);
            var t3 = GetTowerCenter(Tower3);

            // Радиусы
            var r1 = Radius1.Width / 2;
            var r2 = Radius2.Width / 2;
            var r3 = Radius3.Width / 2;

            // Координаты красной точки
            var redPoint = GetTowerCenter(ObjectPoint);

            // Проверка, находится ли точка внутри всех окружностей
            bool isInside =
                IsPointInsideCircle(redPoint, t1, r1) &&
                IsPointInsideCircle(redPoint, t2, r2) &&
                IsPointInsideCircle(redPoint, t3, r3);

            if (isInside)
            {
                CoordinatesMenu.Header = $"Координаты объекта: ({redPoint.X:F2}, {redPoint.Y:F2})";
            }
            else
            {
                CoordinatesMenu.Header = "Координаты объекта: невозможно рассчитать";
            }

            UpdateMenu();
        }

        private bool IsPointInsideCircle(Point point, Point center, double radius)
        {
            double distanceSquared = Math.Pow(point.X - center.X, 2) + Math.Pow(point.Y - center.Y, 2);
            return distanceSquared <= radius * radius;
        }

        private Point GetTowerCenter(UIElement element)
        {
            return new Point(
                Canvas.GetLeft(element) + element.RenderSize.Width / 2,
                Canvas.GetTop(element) + element.RenderSize.Height / 2
            );
        }

        private void UpdateMenu()
        {
            Tower1Menu.Header = $"Вышка 1: Координаты: ({GetTowerCenter(Tower1).X:F2}, {GetTowerCenter(Tower1).Y:F2}), Радиус: {Radius1.Width / 2:F2}";
            Tower2Menu.Header = $"Вышка 2: Координаты: ({GetTowerCenter(Tower2).X:F2}, {GetTowerCenter(Tower2).Y:F2}), Радиус: {Radius2.Width / 2:F2}";
            Tower3Menu.Header = $"Вышка 3: Координаты: ({GetTowerCenter(Tower3).X:F2}, {GetTowerCenter(Tower3).Y:F2}), Радиус: {Radius3.Width / 2:F2}";
        }
    }
}
