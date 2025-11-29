using ShulteTestAutomation.Models;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace ShulteTestAutomation.Services
{
    public class ConfigurationService
    {
        private readonly string _configFile;

        public ConfigurationService()
        {
            _configFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "appsettings.json");
            Directory.CreateDirectory(Path.GetDirectoryName(_configFile));
        }

        public TestConfiguration LoadConfiguration()
        {
            try
            {
                if (File.Exists(_configFile))
                {
                    var json = File.ReadAllText(_configFile);
                    return JsonSerializer.Deserialize<TestConfiguration>(json) ?? GetDefaultConfiguration();
                }
            }
            catch (Exception)
            {
                // В случае ошибки возвращаем конфигурацию по умолчанию
            }

            return GetDefaultConfiguration();
        }

        public void SaveConfiguration(TestConfiguration configuration)
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(configuration, options);
                File.WriteAllText(_configFile, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения настроек: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private TestConfiguration GetDefaultConfiguration()
        {
            return new TestConfiguration
            {
                TableSize = 5,
                SequenceType = SequenceType.Ascending,
                ShuffleAfterEachStep = false,
                IsDefault = true
            };
        }
    }
}
