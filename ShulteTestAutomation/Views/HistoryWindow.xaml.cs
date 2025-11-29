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
            LoadSessions();
        }

        private void LoadSessions()
        {
            var sessions = _dataService.LoadTestSessions();
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

        private void SessionsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SessionsListView.SelectedItem is TestSession session)
            {
                ShowSessionDetails(session);
            }
        }

        private void ShowSessionDetails(TestSession session)
        {
            SessionDetailsPanel.Children.Clear();

            // Общая информация
            SessionDetailsPanel.Children.Add(new TextBlock
            {
                Text = $"Дата: {session.StartTime:dd.MM.yyyy HH:mm}",
                FontWeight = FontWeights.Bold
            });

            // Время по таблицам
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

            // Ошибки по таблицам
            if (session.ErrorCounts.Any())
            {
                SessionDetailsPanel.Children.Add(new TextBlock
                {
                    Text = "Ошибки по таблицам:",
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 5, 0, 0)
                });

                for (int i = 0; i < session.ErrorCounts.Count; i++)
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
}
