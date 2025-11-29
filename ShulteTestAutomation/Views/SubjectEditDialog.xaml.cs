using ShulteTestAutomation.Models;
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
    /// Логика взаимодействия для SubjectEditDialog.xaml
    /// </summary>
    public partial class SubjectEditDialog : Window
    {
        public Subject Subject { get; private set; }

        public SubjectEditDialog(Subject existingSubject = null)
        {
            InitializeComponent();

            if (existingSubject != null)
            {
                Subject = existingSubject;
                LoadSubjectData();
            }
            else
            {
                Subject = new Subject
                {
                    SubjectId = Guid.NewGuid().ToString(),
                    RegistrationDate = DateTime.Now
                };
            }
        }

        private void LoadSubjectData()
        {
            NameTextBox.Text = Subject.Name;
            AgeTextBox.Text = Subject.Age > 0 ? Subject.Age.ToString() : "";
            NotesTextBox.Text = Subject.Notes;

            // Установка пола
            if (!string.IsNullOrEmpty(Subject.Gender))
            {
                foreach (ComboBoxItem item in GenderComboBox.Items)
                {
                    if (item.Content.ToString() == Subject.Gender)
                    {
                        GenderComboBox.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("ФИО испытуемого обязательно для заполнения", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Обновление данных испытуемого
            Subject.Name = NameTextBox.Text.Trim();

            if (int.TryParse(AgeTextBox.Text, out int age))
            {
                Subject.Age = age;
            }
            else
            {
                Subject.Age = 0;
            }

            if (GenderComboBox.SelectedItem is ComboBoxItem selectedGender)
            {
                Subject.Gender = selectedGender.Content.ToString() == "Не указан" ? "" : selectedGender.Content.ToString();
            }

            Subject.Notes = NotesTextBox.Text;

            this.DialogResult = true;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
