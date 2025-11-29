using ShulteTestAutomation.Models;

namespace ShulteTestAutomation.Services
{
    public class SchulteTableGenerator
    {
        private Random _random = new Random();

        public List<int> GenerateTable(int size, SequenceType sequenceType)
        {
            int totalNumbers = size * size;
            var numbers = Enumerable.Range(1, totalNumbers).ToList();

            switch (sequenceType)
            {
                case SequenceType.Ascending:
                    // Оставляем в порядке возрастания
                    break;
                case SequenceType.Descending:
                    numbers.Reverse();
                    break;
                case SequenceType.Random:
                    Shuffle(numbers);
                    break;
            }

            return numbers;
        }

        public List<int> ShuffleRemainingNumbers(List<int> currentNumbers, int lastSelectedNumber)
        {
            var remaining = currentNumbers.Where(n => n > lastSelectedNumber).ToList();
            Shuffle(remaining);
            return remaining.Concat(currentNumbers.Where(n => n <= lastSelectedNumber))
                           .OrderBy(n => n <= lastSelectedNumber ? 0 : 1)
                           .ToList();
        }

        private void Shuffle<T>(IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = _random.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
