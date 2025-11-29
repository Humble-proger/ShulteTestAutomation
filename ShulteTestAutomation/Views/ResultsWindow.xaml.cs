using ShulteTestAutomation.Models;
using ShulteTestAutomation.Services;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ShulteTestAutomation.Views
{
    /// <summary>
    /// Логика взаимодействия для ResultsWindow.xaml
    /// </summary>
    public partial class ResultsWindow : Window
    {
        private readonly TestSession _session;
        private readonly User _currentUser;

        public ResultsWindow(TestSession session, User user = null)
        {
            InitializeComponent();
            _session = session;
            _currentUser = user;
            DisplayResults();
            DrawFatigueChart();
        }

        private void DisplayResults()
        {
            // Отображение времени по таблицам
            TableTimesItems.ItemsSource = _session.TableTimes.Select((time, index) =>
                $"Таблица {index + 1}: {time:F2} сек.");

            // Отображение ошибок по таблицам
            ErrorCountsItems.ItemsSource = _session.ErrorCounts.Select((errors, index) =>
                $"Таблица {index + 1}: {errors} ошибок");

            // Расчетные показатели
            if (_session.Results != null)
            {
                EfficiencyText.Text = $"ER (Эффективность работы): {_session.Results.EfficiencyRate:F2}";
                WorkabilityText.Text = $"BP (Врабатываемость): {_session.Results.WorkabilityIndex:F2}";
                StabilityText.Text = $"IN (Психическая устойчивость): {_session.Results.StabilityIndex:F2}";

                // Интерпретация результатов
                string interpretation = GetInterpretation();
                InterpretationText.Text = interpretation;
            }
        }

        private string GetInterpretation()
        {
            if (_session.Results == null) return "";

            string interpretation = "Интерпретация: ";

            if (_session.Results.WorkabilityIndex < 1.0)
                interpretation += "Хорошая врабатываемость. ";
            else
                interpretation += "Врабатываемость требует тренировки. ";

            if (_session.Results.StabilityIndex < 1.0)
                interpretation += "Высокая психическая устойчивость.";
            else
                interpretation += "Устойчивость внимания снижена к концу теста.";

            return interpretation;
        }

        private void DrawFatigueChart()
        {
            FatigueChart.Children.Clear();

            if (_session.TableTimes == null || _session.TableTimes.Count == 0)
                return;

            double maxTime = _session.TableTimes.Max();
            double minTime = _session.TableTimes.Min();
            double canvasWidth = FatigueChart.ActualWidth - 60; // Отступы для осей
            double canvasHeight = FatigueChart.ActualHeight - 60;

            if (canvasWidth <= 0 || canvasHeight <= 0)
            {
                // Если размеры еще не установлены, отложим отрисовку
                return;
            }

            double xStep = canvasWidth / (_session.TableTimes.Count - 1);
            double yScale = canvasHeight / (maxTime - minTime);

            // Оси
            DrawAxes(canvasWidth, canvasHeight);

            // Точки и линии графика
            Polyline polyline = new Polyline
            {
                Stroke = Brushes.Blue,
                StrokeThickness = 3,
                StrokeLineJoin = PenLineJoin.Round
            };

            for (int i = 0; i < _session.TableTimes.Count; i++)
            {
                double x = 40 + i * xStep;
                double y = 40 + canvasHeight - ((_session.TableTimes[i] - minTime) * yScale);

                polyline.Points.Add(new Point(x, y));

                // Точка
                var point = new Ellipse
                {
                    Width = 10,
                    Height = 10,
                    Fill = Brushes.Red,
                    Stroke = Brushes.DarkRed,
                    StrokeThickness = 2
                };
                Canvas.SetLeft(point, x - 5);
                Canvas.SetTop(point, y - 5);
                FatigueChart.Children.Add(point);

                // Подписи по оси X
                var xLabel = new TextBlock
                {
                    Text = $"Табл.{i + 1}",
                    FontSize = 10,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                Canvas.SetLeft(xLabel, x - 15);
                Canvas.SetTop(xLabel, canvasHeight + 45);
                FatigueChart.Children.Add(xLabel);

                // Подписи значений по оси Y
                if (i == 0 || i == _session.TableTimes.Count - 1 || _session.TableTimes[i] == maxTime || _session.TableTimes[i] == minTime)
                {
                    var yLabel = new TextBlock
                    {
                        Text = _session.TableTimes[i].ToString("F1"),
                        FontSize = 9,
                        HorizontalAlignment = HorizontalAlignment.Right
                    };
                    Canvas.SetLeft(yLabel, 5);
                    Canvas.SetTop(yLabel, y - 8);
                    FatigueChart.Children.Add(yLabel);
                }
            }

            FatigueChart.Children.Add(polyline);

            // Заголовок графика
            var title = new TextBlock
            {
                Text = "Динамика времени прохождения таблиц",
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            Canvas.SetLeft(title, canvasWidth / 2 - 50);
            Canvas.SetTop(title, 5);
            FatigueChart.Children.Add(title);
        }

        private void DrawAxes(double width, double height)
        {
            // Ось X
            var xAxis = new Line
            {
                X1 = 40,
                Y1 = 40 + height,
                X2 = 40 + width,
                Y2 = 40 + height,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };

            // Ось Y
            var yAxis = new Line
            {
                X1 = 40,
                Y1 = 40,
                X2 = 40,
                Y2 = 40 + height,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };

            FatigueChart.Children.Add(xAxis);
            FatigueChart.Children.Add(yAxis);

            // Стрелки для осей
            DrawArrow(40 + width, 40 + height, 35 + width, 35 + height); // X axis arrow
            DrawArrow(40, 40, 35, 45); // Y axis arrow

            // Подписи осей
            var xAxisLabel = new TextBlock
            {
                Text = "Таблицы",
                FontWeight = FontWeights.Bold
            };
            Canvas.SetLeft(xAxisLabel, 40 + width / 2 - 20);
            Canvas.SetTop(xAxisLabel, 40 + height + 20);

            var yAxisLabel = new TextBlock
            {
                Text = "Время (сек)",
                FontWeight = FontWeights.Bold,
                RenderTransform = new RotateTransform(-90)
            };
            Canvas.SetLeft(yAxisLabel, 5);
            Canvas.SetTop(yAxisLabel, 40 + height / 2 - 20);

            FatigueChart.Children.Add(xAxisLabel);
            FatigueChart.Children.Add(yAxisLabel);
        }

        private void DrawArrow(double x1, double y1, double x2, double y2)
        {
            var arrow = new Line
            {
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };
            FatigueChart.Children.Add(arrow);
        }

        private void FatigueChart_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawFatigueChart();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawFatigueChart();
        }

        private void GoToMain_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private void ShowHistory_Click(object sender, RoutedEventArgs e)
        {
            var historyWindow = new HistoryWindow(_currentUser);
            historyWindow.ShowDialog();
        }
    }
}
