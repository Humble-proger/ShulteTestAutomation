using ShulteTestAutomation.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShulteTestAutomation.Services
{
    public class TestSessionService
    {
        private readonly DataService _dataService;

        public TestSessionService(DataService dataService)
        {
            _dataService = dataService;
        }

        public TestResult CalculateResults(List<double> tableTimes, List<int> errorCounts)
        {
            if (tableTimes.Count != 5)
                throw new ArgumentException("Должно быть 5 временных показателей");

            var result = new TestResult();
            result.TotalTime = tableTimes.Sum();
            result.TotalErrors = errorCounts.Sum();

            // ER = Среднее время работы с одной таблицей
            result.EfficiencyRate = tableTimes.Average();

            // BP = Врабатываемость (T1 / ER)
            result.WorkabilityIndex = tableTimes[0] / result.EfficiencyRate;

            // IN = Психическая устойчивость (T4 / ER)
            result.StabilityIndex = tableTimes[3] / result.EfficiencyRate;

            return result;
        }

        public void SaveSession(TestSession session)
        {
            _dataService.SaveTestSession(session);
        }
    }
}
