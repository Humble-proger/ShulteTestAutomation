using ShulteTestAutomation.Models;
using System.IO;
using System.Text.Json;

namespace ShulteTestAutomation.Services
{
    public class DataService
    {
        private readonly string _dataDirectory;
        private readonly string _subjectsFile;
        private readonly string _sessionsDirectory;

        public DataService()
        {
            _dataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            _subjectsFile = Path.Combine(_dataDirectory, "subjects.json");
            _sessionsDirectory = Path.Combine(_dataDirectory, "Sessions");

            // Создаем директории, если они не существуют
            Directory.CreateDirectory(_dataDirectory);
            Directory.CreateDirectory(_sessionsDirectory);
        }

        // Работа с испытуемыми
        public List<Subject> LoadSubjects()
        {
            if (!File.Exists(_subjectsFile))
                return new List<Subject>();

            try
            {
                var json = File.ReadAllText(_subjectsFile);
                return JsonSerializer.Deserialize<List<Subject>>(json) ?? new List<Subject>();
            }
            catch (Exception)
            {
                return new List<Subject>();
            }
        }

        public void SaveSubjects(List<Subject> subjects)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(subjects, options);
            File.WriteAllText(_subjectsFile, json);
        }

        public void SaveSubject(Subject subject)
        {
            var subjects = LoadSubjects();
            var existing = subjects.FirstOrDefault(s => s.SubjectId == subject.SubjectId);

            if (existing != null)
            {
                subjects.Remove(existing);
            }

            subjects.Add(subject);
            SaveSubjects(subjects);
        }

        // Работа с сессиями тестирования
        public void SaveTestSession(TestSession session)
        {
            var sessionFile = Path.Combine(_sessionsDirectory, $"{session.SessionId}.json");
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(session, options);
            File.WriteAllText(sessionFile, json);
        }

        public List<TestSession> LoadTestSessions()
        {
            var sessions = new List<TestSession>();

            if (!Directory.Exists(_sessionsDirectory))
                return sessions;

            foreach (var file in Directory.GetFiles(_sessionsDirectory, "*.json"))
            {
                try
                {
                    var json = File.ReadAllText(file);
                    var session = JsonSerializer.Deserialize<TestSession>(json);
                    if (session != null)
                        sessions.Add(session);
                }
                catch (Exception)
                {
                    // Пропускаем поврежденные файлы
                }
            }

            return sessions.OrderByDescending(s => s.StartTime).ToList();
        }

        public List<TestSession> LoadTestSessionsBySubject(string subjectId)
        {
            var allSessions = LoadTestSessions();
            return allSessions.Where(s => s.SubjectId == subjectId).ToList();
        }

        public Subject GetOrCreateDefaultSubject()
        {
            var subjects = LoadSubjects();
            var defaultSubject = subjects.FirstOrDefault(s => s.Name == "Анонимный испытуемый");

            if (defaultSubject == null)
            {
                defaultSubject = new Subject
                {
                    SubjectId = Guid.NewGuid().ToString(),
                    Name = "Анонимный испытуемый",
                    RegistrationDate = DateTime.Now
                };
                SaveSubject(defaultSubject);
            }

            return defaultSubject;
        }
        public void LinkSessionToSubject(TestSession session, string subjectId)
        {
            var subjects = LoadSubjects();
            var subject = subjects.FirstOrDefault(s => s.SubjectId == subjectId);

            if (subject != null)
            {
                subject.TestSessions.Add(session);
                SaveSubjects(subjects);
            }
        }

        public List<TestSession> GetSubjectSessions(string subjectId)
        {
            var allSessions = LoadTestSessions();
            return allSessions.Where(s => s.SubjectId == subjectId).ToList();
        }
        public List<TestSession> LoadTestSessionsWithSubjects()
        {
            var sessions = LoadTestSessions();
            var subjects = LoadSubjects();

            foreach (var session in sessions)
            {
                var subject = subjects.FirstOrDefault(s => s.SubjectId == session.SubjectId);
                if (subject != null)
                {
                    session.SubjectName = subject.Name;
                    session.SubjectAge = subject.Age > 0 ? subject.Age.ToString() : "Не указан";
                }
            }

            return sessions;
        }
    }
}
