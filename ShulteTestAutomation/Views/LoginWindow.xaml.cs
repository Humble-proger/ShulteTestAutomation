using ShulteTestAutomation.Models;
using ShulteTestAutomation.Services;
using System.Windows;
using System.Windows.Controls;

namespace ShulteTestAutomation.Views
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private readonly UserService _userService;

        public User CurrentUser { get; private set; }

        public LoginWindow()
        {
            InitializeComponent();
            _userService = new UserService();
            UsernameTextBox.Focus();

            // Обработка нажатия Enter в поле пароля
            PasswordBox.KeyDown += PasswordBox_KeyDown;
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            AuthenticateUser();
        }

        private void PasswordBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                AuthenticateUser();
            }
        }

        private void AuthenticateUser()
        {
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ShowError("Введите имя пользователя и пароль");
                return;
            }

            var user = _userService.Authenticate(username, password);
            if (user != null)
            {
                CurrentUser = user;
                this.DialogResult = true; // Устанавливаем DialogResult перед закрытием
                this.Close();
            }
            else
            {
                ShowError("Неверное имя пользователя или пароль");
            }
        }

        private void ShowError(string message)
        {
            ErrorText.Text = message;
            ErrorText.Visibility = Visibility.Visible;
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
