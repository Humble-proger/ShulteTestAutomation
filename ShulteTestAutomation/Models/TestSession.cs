using System.Text.Json.Serialization;

namespace ShulteTestAutomation.Models
{
    public class TestSession
    {
        public string SessionId { get; set; }
        public string SubjectId { get; set; }
        
        [JsonIgnore]
        public string SubjectName { get; set; }

        [JsonIgnore]
        public string SubjectAge { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TestConfiguration Configuration { get; set; }
        public List<double> TableTimes { get; set; } = new List<double>();
        public List<int> ErrorCounts { get; set; } = new List<int>();
        public TestResult Results { get; set; }
    }
}
