using ShulteTestAutomation.Models;
using System.Windows;
using System.Windows.Controls;

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
                    RegistrationDate = DateTime.Now,
                    TestConfiguration = null // По умолчанию нет индивидуальных настроек
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

            // Загрузка настроек теста
            if (Subject.TestConfiguration != null)
            {
                UseCustomSettingsCheckBox.IsChecked = true;
                CustomSettingsPanel.Visibility = Visibility.Visible;

                // Размер таблицы
                TableSizeComboBox.SelectedIndex = Subject.TestConfiguration.TableSize - 3;

                // Последовательность обхода
                switch (Subject.TestConfiguration.SequenceType)
                {
                    case SequenceType.Ascending:
                        AscendingRadio.IsChecked = true;
                        break;
                    case SequenceType.Descending:
                        DescendingRadio.IsChecked = true;
                        break;
                    case SequenceType.Random:
                        RandomRadio.IsChecked = true;
                        break;
                }

                // Режим перемешивания
                ShuffleCheckBox.IsChecked = Subject.TestConfiguration.ShuffleAfterEachStep;
            }
        }

        private void UseCustomSettingsCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            CustomSettingsPanel.Visibility = UseCustomSettingsCheckBox.IsChecked == true
                ? Visibility.Visible
                : Visibility.Collapsed;
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

            // Сохранение настроек теста
            if (UseCustomSettingsCheckBox.IsChecked == true)
            {
                Subject.TestConfiguration = new TestConfiguration
                {
                    TableSize = TableSizeComboBox.SelectedIndex + 3,
                    ShuffleAfterEachStep = ShuffleCheckBox.IsChecked ?? false,
                    IsDefault = false
                };

                // Определяем последовательность обхода
                if (AscendingRadio.IsChecked == true)
                    Subject.TestConfiguration.SequenceType = SequenceType.Ascending;
                else if (DescendingRadio.IsChecked == true)
                    Subject.TestConfiguration.SequenceType = SequenceType.Descending;
                else if (RandomRadio.IsChecked == true)
                    Subject.TestConfiguration.SequenceType = SequenceType.Random;
            }
            else
            {
                Subject.TestConfiguration = null;
            }

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
