using ShulteTestAutomation.Models;
using ShulteTestAutomation.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ShulteTestAutomation.Views
{
    /// <summary>
    /// Логика взаимодействия для AllResultsWindow.xaml
    /// </summary>
    public partial class AllResultsWindow : Window
    {
        private readonly DataService _dataService;
        private readonly UserService _userService;
        private List<TestSession> _allSessions;

        public AllResultsWindow()
        {
            InitializeComponent();
            _dataService = new DataService();
            _userService = new UserService();
            LoadAllSessions();
        }

        private void LoadAllSessions()
        {
            _allSessions = _dataService.LoadTestSessions();

            // Обогащаем данные информацией об испытуемых
            var subjects = _dataService.LoadSubjects();
            foreach (var session in _allSessions)
            {
                var subject = subjects.FirstOrDefault(s => s.SubjectId == session.SubjectId);
                if (subject != null)
                {
                    session.SubjectName = subject.Name;
                    session.SubjectAge = subject.Age > 0 ? subject.Age.ToString() : "Не указан";
                }
                else
                {
                    session.SubjectName = "Неизвестный";
                    session.SubjectAge = "Не указан";
                }
            }

            AllSessionsListView.ItemsSource = _allSessions;
        }

        private void AllSessionsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AllSessionsListView.SelectedItem is TestSession session)
            {
                ShowSessionDetails(session);
                DrawFatigueChart(session);
                ShowDetailedResults(session);
            }
        }

        private void ShowDetailedResults(TestSession session)
        {
            DetailedResultsPanel.Children.Clear();

            // Время по таблицам
            DetailedResultsPanel.Children.Add(new TextBlock
            {
                Text = "Время по таблицам:",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 5)
            });

            for (int i = 0; i < session.TableTimes.Count; i++)
            {
                DetailedResultsPanel.Children.Add(new TextBlock
                {
                    Text = $"Таблица {i + 1}: {session.TableTimes[i]:F2} сек.",
                    Margin = new Thickness(10, 0, 0, 2)
                });
            }

            // Ошибки по таблицам
            if (session.ErrorCounts.Any(e => e > 0))
            {
                DetailedResultsPanel.Children.Add(new TextBlock
                {
                    Text = "Ошибки по таблицам:",
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 10, 0, 5)
                });

                for (int i = 0; i < session.ErrorCounts.Count; i++)
                {
                    DetailedResultsPanel.Children.Add(new TextBlock
                    {
                        Text = $"Таблица {i + 1}: {session.ErrorCounts[i]} ошибок",
                        Margin = new Thickness(10, 0, 0, 2)
                    });
                }
            }

            // Интерпретация результатов
            if (session.Results != null)
            {
                string interpretation = GetInterpretation(session);

                DetailedResultsPanel.Children.Add(new TextBlock
                {
                    Text = "Интерпретация:",
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 10, 0, 5)
                });

                DetailedResultsPanel.Children.Add(new TextBlock
                {
                    Text = interpretation,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(10, 0, 0, 0)
                });
            }
        }

        private string GetInterpretation(TestSession session)
        {
            if (session.Results == null) return "";

            string interpretation = "";

            if (session.Results.WorkabilityIndex < 1.0)
                interpretation += "Хорошая врабатываемость. ";
            else
                interpretation += "Врабатываемость требует тренировки. ";

            if (session.Results.StabilityIndex < 1.0)
                interpretation += "Высокая психическая устойчивость.";
            else
                interpretation += "Устойчивость внимания снижена к концу теста.";

            return interpretation;
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
            if (AllSessionsListView.SelectedItem is TestSession session)
            {
                DrawFatigueChart(session);
            }
        }

        private void ShowDetailedView_Click(object sender, RoutedEventArgs e)
        {
            var selectedSession = AllSessionsListView.SelectedItem as TestSession;
            if (selectedSession == null)
            {
                MessageBox.Show("Выберите сессию для детального просмотра", "Информация",
                              MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Открываем окно с детальным просмотром, аналогичное ResultsWindow
            var detailedWindow = new ResultsWindow(selectedSession);
            detailedWindow.Owner = this;
            detailedWindow.ShowDialog();
        }

        private void ShowSessionDetails(TestSession session)
        {
            SessionDetailsPanel.Children.Clear();

            SessionDetailsPanel.Children.Add(new TextBlock
            {
                Text = $"Испытуемый: {session.SubjectName}",
                FontWeight = FontWeights.Bold,
                FontSize = 14
            });

            SessionDetailsPanel.Children.Add(new TextBlock
            {
                Text = $"Возраст: {session.SubjectAge}",
                Margin = new Thickness(0, 5, 0, 0)
            });

            SessionDetailsPanel.Children.Add(new TextBlock
            {
                Text = $"Дата тестирования: {session.StartTime:dd.MM.yyyy HH:mm}",
                Margin = new Thickness(0, 5, 0, 0)
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
                Text = $"Перемешивание: {(session.Configuration.ShuffleAfterEachStep ? "Включено" : "Выключено")}",
                Margin = new Thickness(0, 2, 0, 0)
            });

            if (session.Results != null)
            {
                SessionDetailsPanel.Children.Add(new TextBlock
                {
                    Text = "Основные показатели:",
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 10, 0, 0)
                });

                SessionDetailsPanel.Children.Add(new TextBlock
                {
                    Text = $"ER (Эффективность работы): {session.Results.EfficiencyRate:F2}",
                    Margin = new Thickness(10, 2, 0, 0)
                });

                SessionDetailsPanel.Children.Add(new TextBlock
                {
                    Text = $"BP (Врабатываемость): {session.Results.WorkabilityIndex:F2}",
                    Margin = new Thickness(10, 2, 0, 0)
                });

                SessionDetailsPanel.Children.Add(new TextBlock
                {
                    Text = $"IN (Психическая устойчивость): {session.Results.StabilityIndex:F2}",
                    Margin = new Thickness(10, 2, 0, 0)
                });

                SessionDetailsPanel.Children.Add(new TextBlock
                {
                    Text = $"Общее время: {session.Results.TotalTime:F2} сек.",
                    Margin = new Thickness(10, 2, 0, 0)
                });

                SessionDetailsPanel.Children.Add(new TextBlock
                {
                    Text = $"Общее количество ошибок: {session.Results.TotalErrors}",
                    Margin = new Thickness(10, 2, 0, 0)
                });
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadAllSessions();
        }

        private void DeleteSelected_Click(object sender, RoutedEventArgs e)
        {
            var selectedSessions = AllSessionsListView.SelectedItems.Cast<TestSession>().ToList();
            if (selectedSessions.Count == 0)
            {
                MessageBox.Show("Выберите сессии для удаления", "Информация",
                              MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show($"Вы уверены, что хотите удалить {selectedSessions.Count} сессий?",
                                        "Подтверждение удаления",
                                        MessageBoxButton.YesNo,
                                        MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    foreach (var session in selectedSessions)
                    {
                        _dataService.DeleteSession(session.SessionId);
                    }
                    LoadAllSessions();
                    MessageBox.Show("Сессии удалены", "Успех",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void ExportToCsv_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var csvService = new CsvExportService();
                string filePath = csvService.ExportSessionsToCsv(_allSessions);

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

        private void ShowStatistics_Click(object sender, RoutedEventArgs e)
        {
            ShowGeneralStatistics();
        }

        private void CompareSelected_Click(object sender, RoutedEventArgs e)
        {
            var selectedSessions = AllSessionsListView.SelectedItems.Cast<TestSession>().ToList();
            if (selectedSessions.Count < 2)
            {
                MessageBox.Show("Выберите как минимум 2 сессии для сравнения",
                              "Информация",
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);
                return;
            }

            ShowComparisonStatistics(selectedSessions);
        }

        private void ShowGeneralStatistics()
        {
            StatisticsPanel.Children.Clear();

            if (!_allSessions.Any())
            {
                StatisticsPanel.Children.Add(new TextBlock { Text = "Нет данных для статистики" });
                return;
            }

            // Основная статистика
            var erValues = _allSessions.Where(s => s.Results != null).Select(s => s.Results.EfficiencyRate).ToList();
            var bpValues = _allSessions.Where(s => s.Results != null).Select(s => s.Results.WorkabilityIndex).ToList();
            var inValues = _allSessions.Where(s => s.Results != null).Select(s => s.Results.StabilityIndex).ToList();

            if (erValues.Any())
            {
                AddStatisticLine("Общее количество тестов:", _allSessions.Count.ToString());
                AddStatisticLine("Количество испытуемых:", _allSessions.Select(s => s.SubjectId).Distinct().Count().ToString());
                AddStatisticLine("Средний ER (Эффективность):", $"{erValues.Average():F2}");
                AddStatisticLine("Средний BP (Врабатываемость):", $"{bpValues.Average():F2}");
                AddStatisticLine("Средний IN (Устойчивость):", $"{inValues.Average():F2}");
                AddStatisticLine("Минимальный ER:", $"{erValues.Min():F2}");
                AddStatisticLine("Максимальный ER:", $"{erValues.Max():F2}");
            }
        }

        private void ShowComparisonStatistics(List<TestSession> sessions)
        {
            StatisticsPanel.Children.Clear();

            AddStatisticLine("Сравнение выбранных сессий:", $"{sessions.Count} сессий");
            AddStatisticLine("", ""); // Пустая строка

            foreach (var session in sessions)
            {
                if (session.Results != null)
                {
                    AddStatisticLine($"{session.SubjectName} - {session.StartTime:dd.MM.yyyy}",
                                   $"ER: {session.Results.EfficiencyRate:F2}, " +
                                   $"BP: {session.Results.WorkabilityIndex:F2}, " +
                                   $"IN: {session.Results.StabilityIndex:F2}");
                }
            }

            // Сравнительная статистика
            var erValues = sessions.Where(s => s.Results != null).Select(s => s.Results.EfficiencyRate).ToList();
            if (erValues.Count >= 2)
            {
                AddStatisticLine("", ""); // Пустая строка
                AddStatisticLine("Лучший ER:", $"{erValues.Min():F2}");
                AddStatisticLine("Худший ER:", $"{erValues.Max():F2}");
                AddStatisticLine("Разница:", $"{(erValues.Max() - erValues.Min()):F2}");
            }
        }

        private void AddStatisticLine(string label, string value)
        {
            var stackPanel = new StackPanel { Orientation = Orientation.Horizontal };
            stackPanel.Children.Add(new TextBlock
            {
                Text = label,
                FontWeight = FontWeights.Bold,
                Width = 200
            });
            stackPanel.Children.Add(new TextBlock { Text = value });
            StatisticsPanel.Children.Add(stackPanel);
        }
    }
}
