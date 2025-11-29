namespace ShulteTestAutomation.Models
{
    public class TestResult
    {
        public double EfficiencyRate { get; set; } // ER
        public double WorkabilityIndex { get; set; } // BP
        public double StabilityIndex { get; set; } // IN
        public int TotalErrors { get; set; }
        public double TotalTime { get; set; }
    }
}
