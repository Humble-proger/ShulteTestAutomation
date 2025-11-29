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
                    // Добавляем свойства для отображения
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
