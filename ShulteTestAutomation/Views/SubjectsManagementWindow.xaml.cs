using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ShulteTestAutomation.Models;
using ShulteTestAutomation.Services;

namespace ShulteTestAutomation.Views
{
    /// <summary>
    /// Логика взаимодействия для SubjectsManagementWindow.xaml
    /// </summary>
    public partial class SubjectsManagementWindow : Window
    {
        private readonly DataService _dataService;
        private readonly UserService _userService;

        public SubjectsManagementWindow()
        {
            InitializeComponent();
            _dataService = new DataService();
            _userService = new UserService();
            LoadSubjects();
        }

        private void LoadSubjects()
        {
            var subjects = _dataService.LoadSubjects();
            SubjectsListView.ItemsSource = subjects;
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadSubjects();
        }

        private void AddSubject_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SubjectEditDialog();
            if (dialog.ShowDialog() == true)
            {
                var newSubject = dialog.Subject;
                _dataService.SaveSubject(newSubject);

                // Создаем пользователя для испытуемого
                var userService = new UserService();
                var username = GenerateUsername(newSubject.Name);
                var password = GenerateRandomPassword();

                var user = userService.CreateSubjectUser(username, password, newSubject.SubjectId);
                if (user != null)
                {
                    MessageBox.Show($"Создан пользователь для испытуемого:\nЛогин: {username}\nПароль: {password}",
                                  "Успех",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Information);
                }

                LoadSubjects();
            }
        }

        private string GenerateUsername(string name)
        {
            // Генерация логина на основе имени
            var baseName = name.Replace(" ", ".").ToLower();
            return $"{baseName}.{DateTime.Now:MMdd}";
        }

        private string GenerateRandomPassword()
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private void EditSubject_Click(object sender, RoutedEventArgs e)
        {
            if (SubjectsListView.SelectedItem is Subject selectedSubject)
            {
                var dialog = new SubjectEditDialog(selectedSubject);
                if (dialog.ShowDialog() == true)
                {
                    _dataService.SaveSubject(dialog.Subject);
                    LoadSubjects();
                    MessageBox.Show("Данные испытуемого обновлены!", "Успех",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Выберите испытуемого для редактирования", "Информация",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeleteSubject_Click(object sender, RoutedEventArgs e)
        {
            if (SubjectsListView.SelectedItem is Subject selectedSubject)
            {
                var result = MessageBox.Show($"Удалить испытуемого {selectedSubject.Name}?",
                                           "Подтверждение удаления",
                                           MessageBoxButton.YesNo,
                                           MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    var subjects = _dataService.LoadSubjects();
                    subjects.RemoveAll(s => s.SubjectId == selectedSubject.SubjectId);
                    _dataService.SaveSubjects(subjects);
                    LoadSubjects();
                    MessageBox.Show("Испытуемый удален!", "Успех",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Выберите испытуемого для удаления", "Информация",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void SubjectsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SubjectsListView.SelectedItem is Subject subject)
            {
                ShowSubjectDetails(subject);
            }
        }

        private void ShowSubjectDetails(Subject subject)
        {
            SubjectDetailsPanel.Children.Clear();

            SubjectDetailsPanel.Children.Add(new TextBlock
            {
                Text = $"ФИО: {subject.Name}",
                FontWeight = FontWeights.Bold
            });

            if (subject.Age > 0)
            {
                SubjectDetailsPanel.Children.Add(new TextBlock
                {
                    Text = $"Возраст: {subject.Age}"
                });
            }

            if (!string.IsNullOrEmpty(subject.Gender))
            {
                SubjectDetailsPanel.Children.Add(new TextBlock
                {
                    Text = $"Пол: {subject.Gender}"
                });
            }

            SubjectDetailsPanel.Children.Add(new TextBlock
            {
                Text = $"Дата регистрации: {subject.RegistrationDate:dd.MM.yyyy}"
            });

            SubjectDetailsPanel.Children.Add(new TextBlock
            {
                Text = $"Количество тестов: {subject.TestSessions?.Count ?? 0}"
            });

            if (!string.IsNullOrEmpty(subject.Notes))
            {
                SubjectDetailsPanel.Children.Add(new TextBlock
                {
                    Text = $"Примечания: {subject.Notes}",
                    TextWrapping = TextWrapping.Wrap
                });
            }
        }
    }
}
