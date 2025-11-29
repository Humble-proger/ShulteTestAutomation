using ShulteTestAutomation.Models;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace ShulteTestAutomation.Services
{
    public class UserService
    {
        private readonly string _usersFile;
        private readonly DataService _dataService;

        public UserService()
        {
            _usersFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "users.json");
            _dataService = new DataService();
            InitializeDefaultUsers();
        }

        private void InitializeDefaultUsers()
        {
            var users = LoadUsers();

            // Создаем исследователя по умолчанию, если его нет
            if (!users.Any(u => u.Role == UserRole.Researcher))
            {
                var researcher = new User
                {
                    UserId = Guid.NewGuid().ToString(),
                    Username = "admin",
                    Password = "admin", // В реальном приложении нужно хэшировать пароль
                    Role = UserRole.Researcher,
                    CreatedDate = DateTime.Now
                };
                users.Add(researcher);
                SaveUsers(users);
            }
        }

        public List<User> LoadUsers()
        {
            try
            {
                if (File.Exists(_usersFile))
                {
                    var json = File.ReadAllText(_usersFile);
                    return JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
                }
            }
            catch (Exception)
            {
                // В случае ошибки возвращаем пустой список
            }

            return new List<User>();
        }

        public void SaveUsers(List<User> users)
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(users, options);
                File.WriteAllText(_usersFile, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения пользователей: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public User Authenticate(string username, string password)
        {
            var users = LoadUsers();
            return users.FirstOrDefault(u =>
                u.Username == username &&
                u.Password == password &&
                u.IsActive);
        }

        public bool CreateUser(User user)
        {
            var users = LoadUsers();

            if (users.Any(u => u.Username == user.Username))
            {
                return false; // Пользователь с таким именем уже существует
            }

            user.UserId = Guid.NewGuid().ToString();
            user.CreatedDate = DateTime.Now;
            users.Add(user);
            SaveUsers(users);
            return true;
        }

        public List<User> GetSubjects()
        {
            var users = LoadUsers();
            return users.Where(u => u.Role == UserRole.Subject && u.IsActive).ToList();
        }
        public User CreateSubjectUser(string username, string password, string subjectId)
        {
            var user = new User
            {
                UserId = Guid.NewGuid().ToString(),
                Username = username,
                Password = password,
                Role = UserRole.Subject,
                SubjectId = subjectId,
                CreatedDate = DateTime.Now
            };

            if (CreateUser(user))
            {
                return user;
            }

            return null;
        }

        public bool DeleteUser(string userId)
        {
            var users = LoadUsers();
            var user = users.FirstOrDefault(u => u.UserId == userId);

            if (user != null)
            {
                // Проверяем, есть ли сессии у этого пользователя
                var dataService = new DataService();
                var userSessions = dataService.LoadTestSessionsBySubject(user.SubjectId);

                if (userSessions.Any())
                {
                    // Предупреждаем о наличии сессий
                    var result = MessageBox.Show($"У пользователя есть {userSessions.Count} сессий тестирования. " +
                                               $"Удалить пользователя и все его сессии?",
                                               "Подтверждение удаления",
                                               MessageBoxButton.YesNo,
                                               MessageBoxImage.Warning);

                    if (result != MessageBoxResult.Yes)
                    {
                        return false;
                    }

                    // Удаляем все сессии пользователя
                    foreach (var session in userSessions)
                    {
                        dataService.DeleteSession(session.SessionId);
                    }
                }

                users.Remove(user);
                SaveUsers(users);
                return true;
            }

            return false;
        }
    }
}
