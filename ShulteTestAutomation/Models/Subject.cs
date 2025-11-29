using System;
using System.Collections.Generic;

namespace ShulteTestAutomation.Models
{
    public class Subject
    {
        public string SubjectId { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; }
        public DateTime RegistrationDate { get; set; }
        public string Notes { get; set; }
        public TestConfiguration TestConfiguration { get; set; }

        public List<TestSession> TestSessions { get; set; } = new List<TestSession>();
    }
}
