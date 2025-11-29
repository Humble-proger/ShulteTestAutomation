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
    /// Логика взаимодействия для CredentialsWindow.xaml
    /// </summary>
    public partial class CredentialsWindow : Window
    {
        public CredentialsWindow(string username, string password, string subjectName)
        {
            InitializeComponent();

            UsernameTextBox.Text = username;
            PasswordTextBox.Text = password;
            SubjectNameText.Text = subjectName;
        }

        private void CopyUsername_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(UsernameTextBox.Text);
            MessageBox.Show("Логин скопирован в буфер обмена", "Успех",
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CopyPassword_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(PasswordTextBox.Text);
            MessageBox.Show("Пароль скопирован в буфер обмена", "Успех",
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
