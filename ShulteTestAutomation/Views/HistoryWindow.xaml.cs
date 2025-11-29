using ShulteTestAutomation.Models;
using ShulteTestAutomation.Services;
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
using System.Windows.Shapes;

namespace ShulteTestAutomation.Views
{
    /// <summary>
    /// Логика взаимодействия для HistoryWindow.xaml
    /// </summary>
    public partial class HistoryWindow : Window
    {
        private readonly DataService _dataService;
        private readonly User _currentUser;

        public HistoryWindow(User user = null)
        {
            InitializeComponent();
            _dataService = new DataService();
            _currentUser = user;

            // Скрываем кнопку удаления для испытуемых
            if (_currentUser?.Role != UserRole.Researcher)
            {
                DeleteButton.Visibility = Visibility.Collapsed;
            }

            LoadSessions();
        }

        private void LoadSessions()
        {
            var sessions = _dataService.LoadTestSessions();

            // Если это испытуемый, показываем только его сессии
            if (_currentUser?.Role == UserRole.Subject)
            {
                sessions = sessions.Where(s => s.SubjectId == _currentUser.SubjectId).ToList();
            }

            SessionsListView.ItemsSource = sessions;
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadSessions();
        }

        private void SessionsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SessionsListView.SelectedItem is TestSession session)
            {
                ShowSessionDetails(session);
                DrawFatigueChart(session);
            }
        }

        private void ShowSessionDetails(TestSession session)
        {
            SessionDetailsPanel.Children.Clear();

            SessionDetailsPanel.Children.Add(new TextBlock
            {
                Text = $"Дата: {session.StartTime:dd.MM.yyyy HH:mm}",
                FontWeight = FontWeights.Bold
            });

            SessionDetailsPanel.Children.Add(new TextBlock
            {
                Text = $"Размер таблицы: {session.Configuration.TableSize}x{session.Configuration.TableSize}",
                Margin = new Thickness(0, 5, 0, 0)
            });

            SessionDetailsPanel.Children.Add(new TextBlock
            {
                Text = $"Последовательность: {session.Configuration.SequenceType}",
                Margin = new Thickness(0, 2, 0, 0)
            });

            SessionDetailsPanel.Children.Add(new TextBlock
            {
                Text = "Время по таблицам:",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 5, 0, 0)
            });

            for (int i = 0; i < session.TableTimes.Count; i++)
            {
                SessionDetailsPanel.Children.Add(new TextBlock
                {
                    Text = $"  Таблица {i + 1}: {session.TableTimes[i]:F2} сек.",
                    Margin = new Thickness(10, 0, 0, 0)
                });
            }

            if (session.ErrorCounts.Any(e => e > 0))
            {
                SessionDetailsPanel.Children.Add(new TextBlock
                {
                    Text = "Ошибки по таблицам:",
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 5, 0, 0)
                });

                for (int i = 0; i < session.ErrorCounts.Count; i++)
                {
                    if (session.ErrorCounts[i] > 0)
                    {
                        SessionDetailsPanel.Children.Add(new TextBlock
                        {
                            Text = $"  Таблица {i + 1}: {session.ErrorCounts[i]} ошибок",
                            Margin = new Thickness(10, 0, 0, 0)
                        });
                    }
                }
            }
        }

        private void DrawFatigueChart(TestSession session)
        {
            FatigueChart.Children.Clear();
            NoChartText.Visibility = Visibility.Collapsed;

            if (session.TableTimes == null || session.TableTimes.Count == 0)
            {
                NoChartText.Visibility = Visibility.Visible;
                return;
            }

            double maxTime = session.TableTimes.Max();
            double minTime = session.TableTimes.Min();
            double canvasWidth = FatigueChart.ActualWidth - 60;
            double canvasHeight = FatigueChart.ActualHeight - 60;

            if (canvasWidth <= 0 || canvasHeight <= 0) return;

            double xStep = canvasWidth / (session.TableTimes.Count - 1);
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

            for (int i = 0; i < session.TableTimes.Count; i++)
            {
                double x = 40 + i * xStep;
                double y = 40 + canvasHeight - ((session.TableTimes[i] - minTime) * yScale);

                polyline.Points.Add(new Point(x, y));

                // Точка
                var point = new Ellipse
                {
                    Width = 8,
                    Height = 8,
                    Fill = Brushes.Red,
                    Stroke = Brushes.DarkRed,
                    StrokeThickness = 1
                };
                Canvas.SetLeft(point, x - 4);
                Canvas.SetTop(point, y - 4);
                FatigueChart.Children.Add(point);

                // Подписи по оси X
                var xLabel = new TextBlock
                {
                    Text = (i + 1).ToString(),
                    FontSize = 10
                };
                Canvas.SetLeft(xLabel, x - 5);
                Canvas.SetTop(xLabel, canvasHeight + 45);
                FatigueChart.Children.Add(xLabel);
            }

            FatigueChart.Children.Add(polyline);
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
        }

        private void FatigueChart_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (SessionsListView.SelectedItem is TestSession session)
            {
                DrawFatigueChart(session);
            }
        }

        private void ExportToCsv_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var sessions = SessionsListView.ItemsSource as List<TestSession> ?? new List<TestSession>();
                if (!sessions.Any())
                {
                    MessageBox.Show("Нет данных для экспорта", "Информация",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var csvService = new CsvExportService();
                string filePath = csvService.ExportSessionsToCsv(sessions);

                MessageBox.Show($"Данные успешно экспортированы в файл:\n{filePath}",
                              "Экспорт завершен",
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте в CSV: {ex.Message}",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        private void DeleteSelected_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser?.Role != UserRole.Researcher)
            {
                MessageBox.Show("Только исследователи могут удалять записи тестирований", "Ограничение",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedSession = SessionsListView.SelectedItem as TestSession;
            if (selectedSession == null)
            {
                MessageBox.Show("Выберите сессию для удаления", "Информация",
                              MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show($"Удалить сессию от {selectedSession.StartTime:dd.MM.yyyy HH:mm}?",
                                       "Подтверждение удаления",
                                       MessageBoxButton.YesNo,
                                       MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _dataService.DeleteSession(selectedSession.SessionId);
                    LoadSessions();
                    MessageBox.Show("Сессия удалена", "Успех",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
