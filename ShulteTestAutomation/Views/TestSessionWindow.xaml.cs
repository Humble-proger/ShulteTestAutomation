using ShulteTestAutomation.Models;
using ShulteTestAutomation.Services;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

namespace ShulteTestAutomation.Views
{
    /// <summary>
    /// Логика взаимодействия для TestSessionWindow.xaml
    /// </summary>
    public partial class TestSessionWindow : Window
    {
        private TestSession _currentSession;
        private List<int> _currentTable;
        private int _currentNumber;
        private int _currentTableIndex;
        private DateTime _tableStartTime;
        private DispatcherTimer _timer;
        private TimeSpan _elapsedTime;
        private SchulteTableGenerator _tableGenerator;
        private TestSessionService _testService;
        private User _currentUser;

        private List<double> _tableTimes = new List<double>();
        private List<int> _errorCounts = new List<int>();
        private int _currentErrorCount;

        public TestSessionWindow(TestConfiguration configuration = null, User user = null)
        {
            InitializeComponent();
            _currentUser = user;
            InitializeServices();
            StartNewSession(configuration);
        }

        private void InitializeServices()
        {
            _tableGenerator = new SchulteTableGenerator();
            _testService = new TestSessionService(new DataService());
        }

        private void StartNewSession(TestConfiguration configuration)
        {
            _currentSession = new TestSession
            {
                SessionId = Guid.NewGuid().ToString(),
                StartTime = DateTime.Now,
                Configuration = configuration ?? new TestConfiguration(),
                SubjectId = _currentUser?.SubjectId ?? "anonymous"
            };

            _currentTableIndex = 0;
            StartNextTable();
            StartTimer();
        }

        private void StartNextTable()
        {
            _currentTableIndex++;
            _currentNumber = 1;
            _currentErrorCount = 0;
            _tableStartTime = DateTime.Now;
            ErrorPanel.Visibility = Visibility.Collapsed;

            CurrentTableText.Text = $"{_currentTableIndex}/5";
            CurrentNumberText.Text = _currentNumber.ToString();

            // Генерация новой таблицы
            _currentTable = _tableGenerator.GenerateTable(
                _currentSession.Configuration.TableSize,
                _currentSession.Configuration.SequenceType);

            // Обновление UniformGrid с правильным количеством строк и столбцов
            UpdateTableGridLayout();

            TableGrid.ItemsSource = _currentTable;
        }

        private void UpdateTableGridLayout()
        {
            var uniformGrid = TableGrid.ItemsPanel.LoadContent() as UniformGrid;
            if (uniformGrid != null)
            {
                uniformGrid.Rows = _currentSession.Configuration.TableSize;
                uniformGrid.Columns = _currentSession.Configuration.TableSize;

                // Обновляем ItemsPanel
                var template = new ItemsPanelTemplate();
                var factory = new FrameworkElementFactory(typeof(UniformGrid));
                factory.SetValue(UniformGrid.RowsProperty, _currentSession.Configuration.TableSize);
                factory.SetValue(UniformGrid.ColumnsProperty, _currentSession.Configuration.TableSize);
                template.VisualTree = factory;
                TableGrid.ItemsPanel = template;
            }
        }

        private void StartTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(10);
            _timer.Tick += Timer_Tick;
            _elapsedTime = TimeSpan.Zero;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _elapsedTime = _elapsedTime.Add(TimeSpan.FromMilliseconds(10));
            TimerText.Text = _elapsedTime.ToString(@"mm\:ss\.ff");
        }

        private void NumberButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            int selectedNumber = int.Parse(button.Content.ToString());

            if (selectedNumber == _currentNumber)
            {
                // Правильное нажатие
                button.IsEnabled = false;
                button.Background = SystemColors.ControlLightBrush;
                _currentNumber++;
                CurrentNumberText.Text = _currentNumber.ToString();

                // Если включено перемешивание после каждого шага
                if (_currentSession.Configuration.ShuffleAfterEachStep &&
                    _currentNumber <= _currentSession.Configuration.TableSize * _currentSession.Configuration.TableSize)
                {
                    _currentTable = _tableGenerator.ShuffleRemainingNumbers(_currentTable, selectedNumber);
                    TableGrid.ItemsSource = null;
                    TableGrid.ItemsSource = _currentTable;
                }

                // Проверка завершения таблицы
                if (_currentNumber > _currentSession.Configuration.TableSize * _currentSession.Configuration.TableSize)
                {
                    CompleteCurrentTable();
                }
            }
            else
            {
                // Ошибка
                HandleError();
            }
        }

        private void HandleError()
        {
            _currentErrorCount++;
            ErrorPanel.Visibility = Visibility.Visible;

            // Воспроизведение звука ошибки
            System.Media.SystemSounds.Beep.Play();

            // Визуальная индикация ошибки
            var animation = new System.Windows.Media.Animation.DoubleAnimation
            {
                To = 0.3,
                Duration = TimeSpan.FromSeconds(0.1),
                AutoReverse = true,
                RepeatBehavior = new System.Windows.Media.Animation.RepeatBehavior(3)
            };
            ErrorPanel.BeginAnimation(OpacityProperty, animation);
        }

        private void CompleteCurrentTable()
        {
            _timer.Stop();
            var tableTime = (DateTime.Now - _tableStartTime).TotalSeconds;
            _tableTimes.Add(tableTime);
            _errorCounts.Add(_currentErrorCount);

            // Показ промежуточных результатов
            ShowIntermediateResults(tableTime);
        }

        private void ShowIntermediateResults(double tableTime)
        {
            string message = $"Таблица {_currentTableIndex} завершена!\n\n" +
                           $"Время: {tableTime:F2} сек.\n" +
                           $"Ошибок: {_currentErrorCount}";

            MessageBox.Show(message, "Промежуточный результат",
                          MessageBoxButton.OK, MessageBoxImage.Information);

            if (_currentTableIndex < 5)
            {
                StartNextTable();
                _timer.Start();
            }
            else
            {
                CompleteSession();
            }
        }

        private void CompleteSession()
        {
            // Расчет итоговых результатов
            var results = _testService.CalculateResults(_tableTimes, _errorCounts);
            _currentSession.Results = results;
            _currentSession.EndTime = DateTime.Now;
            _currentSession.TableTimes = _tableTimes;
            _currentSession.ErrorCounts = _errorCounts;

            // Сохранение сессии
            _testService.SaveSession(_currentSession);

            // Показ финальных результатов с передачей пользователя
            var resultsWindow = new ResultsWindow(_currentSession, _currentUser);
            resultsWindow.Show();
            this.Close();
        }

        private void CancelTest_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Прервать тестирование?", "Подтверждение",
                              MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                new MainWindow().Show();
                this.Close();
            }
        }
    }
}
