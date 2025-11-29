using ShulteTestAutomation.Models;
using System.Windows;

namespace ShulteTestAutomation.Views
{
    /// <summary>
    /// Логика взаимодействия для ConfigurationWindow.xaml
    /// </summary>
    public partial class ConfigurationWindow : Window
    {
        public TestConfiguration Configuration { get; private set; }

        public ConfigurationWindow(TestConfiguration currentConfig = null)
        {
            InitializeComponent();
            Configuration = currentConfig ?? new TestConfiguration();
            ApplyConfigurationToUI();
        }

        private void ApplyConfigurationToUI()
        {
            // Размер таблицы
            TableSizeComboBox.SelectedIndex = Configuration.TableSize - 3;

            // Последовательность обхода
            switch (Configuration.SequenceType)
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
            ShuffleCheckBox.IsChecked = Configuration.ShuffleAfterEachStep;
        }

        private void SaveConfiguration_Click(object sender, RoutedEventArgs e)
        {
            Configuration = new TestConfiguration
            {
                TableSize = TableSizeComboBox.SelectedIndex + 3,
                ShuffleAfterEachStep = ShuffleCheckBox.IsChecked ?? false,
                IsDefault = false
            };

            // Определяем последовательность обхода
            if (AscendingRadio.IsChecked == true)
                Configuration.SequenceType = SequenceType.Ascending;
            else if (DescendingRadio.IsChecked == true)
                Configuration.SequenceType = SequenceType.Descending;
            else if (RandomRadio.IsChecked == true)
                Configuration.SequenceType = SequenceType.Random;

            this.DialogResult = true;
            this.Close();
        }

        private void ResetToDefault_Click(object sender, RoutedEventArgs e)
        {
            Configuration = new TestConfiguration();
            ApplyConfigurationToUI();
            MessageBox.Show("Настройки сброшены к стандартным", "Информация",
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
