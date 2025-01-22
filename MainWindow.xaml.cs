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
        //Радиусы окр для трех вышек
        private double tower1Radius = 100, tower2Radius = 100, tower3Radius = 100;
        //переменные для перетаскивания
        private bool isDragging;
        private UIElement draggedElement;
        private Point mouseOffset;

        public MainWindow()
        {
            InitializeComponent();
            //Установка начальных радиусов окр
            InitializeRadii();
            //Событие для управления
            InitializeEvents();
        }

        // Метод инициализации радиусов окружностей для вышек
        private void InitializeRadii()
        {
            UpdateTowerAndRadius(Tower1, Radius1, tower1Radius);
            UpdateTowerAndRadius(Tower2, Radius2, tower2Radius);
            UpdateTowerAndRadius(Tower3, Radius3, tower3Radius);
        }

        // Метод подписки на события, такие как перетаскивание и изменение радиусов
        private void InitializeEvents()
        {
            Tower1.MouseDown += StartDrag;
            Tower2.MouseDown += StartDrag;
            Tower3.MouseDown += StartDrag;
            ObjectPoint.MouseDown += StartDrag;

            // События для обработки перетаскивания и его завершения
            MouseMove += Dragging;
            MouseUp += EndDrag;

            // События для изменения радиусов окружностей
            Radius1Input.TextChanged += RadiusChanged;
            Radius2Input.TextChanged += RadiusChanged;
            Radius3Input.TextChanged += RadiusChanged;
        }

        // Обработка изменения радиуса через текстовые поля
        private void RadiusChanged(object sender, TextChangedEventArgs e)
        {
            // Проверяем, что текстовое поле содержит число
            if (sender is TextBox textBox && double.TryParse(textBox.Text, out double newRadius))
            {
                // Определяем, к какой вышке относится изменение радиуса
                if (textBox.Name == "Radius1Input")
                {
                    tower1Radius = newRadius;
                    UpdateTowerAndRadius(Tower1, Radius1, tower1Radius);
                }
                else if (textBox.Name == "Radius2Input")
                {
                    tower2Radius = newRadius;
                    UpdateTowerAndRadius(Tower2, Radius2, tower2Radius);
                }
                else if (textBox.Name == "Radius3Input")
                {
                    tower3Radius = newRadius;
                    UpdateTowerAndRadius(Tower3, Radius3, tower3Radius);
                }
                // Обновляем отображение информации в меню
                UpdateMenu();
            }
        }

        // Начало перетаскивания элемента
        private void StartDrag(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;
            draggedElement = sender as UIElement;
            mouseOffset = e.GetPosition(this);
        }

        // Обработка процесса перетаскивания
        private void Dragging(object sender, MouseEventArgs e)
        {
            if (isDragging && draggedElement != null)
            {
                // Вычисляем новое положение элемента на основе смещения мыши
                var pos = e.GetPosition(this);
                double offsetX = pos.X - mouseOffset.X;
                double offsetY = pos.Y - mouseOffset.Y;

                // Устанавливаем новые координаты элемента на Canvas
                Canvas.SetLeft(draggedElement, Canvas.GetLeft(draggedElement) + offsetX);
                Canvas.SetTop(draggedElement, Canvas.GetTop(draggedElement) + offsetY);

                // Обновляем смещение
                mouseOffset = pos;

                // Обновляем данные в зависимости от перетаскиваемого элемента
                if (draggedElement == Tower1)
                    UpdateTowerAndRadius(Tower1, Radius1, tower1Radius);
                else if (draggedElement == Tower2)
                    UpdateTowerAndRadius(Tower2, Radius2, tower2Radius);
                else if (draggedElement == Tower3)
                    UpdateTowerAndRadius(Tower3, Radius3, tower3Radius);
                else if (draggedElement == ObjectPoint)
                    UpdateObjectCoordinates();

                UpdateMenu();
            }
        }

        // Завершение перетаскивания
        private void EndDrag(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
            draggedElement = null;
            // Проверяем и пересчитываем координаты после перемещения
            if (draggedElement == ObjectPoint)
                UpdateObjectCoordinates();
        }

        // Обновление положения и размера окружности для вышек
        private void UpdateTowerAndRadius(Ellipse tower, Ellipse radius, double radiusValue)
        {
            double x = Canvas.GetLeft(tower) + tower.Width / 2;
            double y = Canvas.GetTop(tower) + tower.Height / 2;

            // Устанавливаем центр окружности и её размер
            Canvas.SetLeft(radius, x - radiusValue);
            Canvas.SetTop(radius, y - radiusValue);
            radius.Width = radiusValue * 2;
            radius.Height = radiusValue * 2;
        }

        // Обновление координат красной точки
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

            // Проверяем, находится ли точка внутри всех трёх окружностей
            if (IsPointInsideCircle(objX, objY, x1, y1, tower1Radius) &&
                IsPointInsideCircle(objX, objY, x2, y2, tower2Radius) &&
                IsPointInsideCircle(objX, objY, x3, y3, tower3Radius))
            {
                var coordinates = CalculateTrilateration(x1, y1, tower1Radius, x2, y2, tower2Radius, x3, y3, tower3Radius);

                if (coordinates != null)
                {
                    CoordinatesMenu.Header = $"Координаты объекта: ({coordinates.Value.x:F2}, {coordinates.Value.y:F2})";
                }
            }
            else
            {
                CoordinatesMenu.Header = "Координаты объекта: Невозможно определить";
            }
        }

        private (double x, double y)? CalculateTrilateration(double x1, double y1, double r1,
                                                            double x2, double y2, double r2,
                                                            double x3, double y3, double r3)
        {
            double A = 2 * (x2 - x1);
            double B = 2 * (y2 - y1);
            double C = r1 * r1 - r2 * r2 - x1 * x1 + x2 * x2 - y1 * y1 + y2 * y2;

            double D = 2 * (x3 - x2);
            double E = 2 * (y3 - y2);
            double F = r2 * r2 - r3 * r3 - x2 * x2 + x3 * x3 - y2 * y2 + y3 * y3;

            double denominator = A * E - B * D;
            if (Math.Abs(denominator) < 1e-6)
                return null;

            double x = (C * E - B * F) / denominator;
            double y = (A * F - C * D) / denominator;

            if (IsPointInsideCircle(x, y, x1, y1, r1) &&
                IsPointInsideCircle(x, y, x2, y2, r2) &&
                IsPointInsideCircle(x, y, x3, y3, r3))
            {
                return (x, y);
            }

            return null;
        }

        private bool IsPointInsideCircle(double x, double y, double cx, double cy, double r)
        {
            return Math.Pow(x - cx, 2) + Math.Pow(y - cy, 2) <= Math.Pow(r, 2);
        }

        private void UpdateMenu()
        {
            Tower1Menu.Header = $"Вышка 1: Радиус = {tower1Radius}, Координаты = ({Canvas.GetLeft(Tower1):F2}, {Canvas.GetTop(Tower1):F2})";
            Tower2Menu.Header = $"Вышка 2: Радиус = {tower2Radius}, Координаты = ({Canvas.GetLeft(Tower2):F2}, {Canvas.GetTop(Tower2):F2})";
            Tower3Menu.Header = $"Вышка 3: Радиус = {tower3Radius}, Координаты = ({Canvas.GetLeft(Tower3):F2}, {Canvas.GetTop(Tower3):F2})";
        }
    }
}
