using ShulteTestAutomation.Models;
using ShulteTestAutomation.Services;
using ShulteTestAutomation.Views;
using System.Windows;

namespace ShulteTestAutomation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ConfigurationService _configService;
        private readonly UserService _userService;
        private TestConfiguration _currentConfig;
        private User _currentUser;

        public MainWindow()
        {
            InitializeComponent();
            _configService = new ConfigurationService();
            _userService = new UserService();
            _currentConfig = _configService.LoadConfiguration();

            // Показываем окно авторизации при загрузке
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ShowLogin();
        }

        private void ShowLogin()
        {
            var loginWindow = new LoginWindow();
            loginWindow.Owner = this;
            loginWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            if (loginWindow.ShowDialog() == true)
            {
                _currentUser = loginWindow.CurrentUser;
                UpdateUIForUserRole();
            }
            else
            {
                // Если пользователь отменил вход, закрываем приложение
                Application.Current.Shutdown();
            }
        }

        private void UpdateUIForUserRole()
        {
            if (_currentUser != null)
            {
                UserInfoText.Text = $"{_currentUser.Username} ({(_currentUser.Role == UserRole.Researcher ? "Исследователь" : "Испытуемый")})";
                LogoutButton.Visibility = Visibility.Visible;

                if (_currentUser.Role == UserRole.Researcher)
                {
                    ResearcherPanel.Visibility = Visibility.Visible;
                    SettingsButton.Visibility = Visibility.Visible;
                }
                else
                {
                    ResearcherPanel.Visibility = Visibility.Collapsed;
                    SettingsButton.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                LogoutButton.Visibility = Visibility.Collapsed;
            }
        }

        private void StartTest_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null)
            {
                MessageBox.Show("Сначала выполните вход в систему", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var testWindow = new TestSessionWindow(_currentConfig, _currentUser);
            testWindow.Show();
            this.Close();
        }

        private void ShowHistory_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null)
            {
                MessageBox.Show("Сначала выполните вход в систему", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var historyWindow = new HistoryWindow(_currentUser);
            historyWindow.Owner = this;
            historyWindow.ShowDialog();
        }

        private void ShowSettings_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null)
            {
                MessageBox.Show("Сначала выполните вход в систему", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (_currentUser.Role == UserRole.Researcher)
            {
                var settingsWindow = new ConfigurationWindow(_currentConfig);
                settingsWindow.Owner = this;
                if (settingsWindow.ShowDialog() == true)
                {
                    _currentConfig = settingsWindow.Configuration;
                    _configService.SaveConfiguration(_currentConfig);
                    MessageBox.Show("Настройки сохранены!", "Успех",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Только исследователи могут изменять настройки теста", "Ограничение",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ManageSubjects_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null)
            {
                MessageBox.Show("Сначала выполните вход в систему", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (_currentUser.Role == UserRole.Researcher)
            {
                var subjectsWindow = new SubjectsManagementWindow();
                subjectsWindow.Owner = this;
                subjectsWindow.ShowDialog();
            }
        }

        private void ShowAllResults_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null)
            {
                MessageBox.Show("Сначала выполните вход в систему", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (_currentUser.Role == UserRole.Researcher)
            {
                var allResultsWindow = new AllResultsWindow();
                allResultsWindow.Owner = this;
                allResultsWindow.ShowDialog();
            }
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            _currentUser = null;
            UserInfoText.Text = "Не авторизован";
            ResearcherPanel.Visibility = Visibility.Collapsed;
            SettingsButton.Visibility = Visibility.Collapsed;

            // Показываем окно авторизации снова
            ShowLogin();
        }
    }
}