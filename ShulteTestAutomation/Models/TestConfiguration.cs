namespace ShulteTestAutomation.Models
{
    public class TestConfiguration
    {
        public int TableSize { get; set; } = 5;
        public SequenceType SequenceType { get; set; } = SequenceType.Ascending;
        public bool ShuffleAfterEachStep { get; set; } = false;
        public bool IsDefault { get; set; } = true;
    }
}
