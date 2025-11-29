using ShulteTestAutomation.Models;
using System.IO;
using System.Text;

namespace ShulteTestAutomation.Services
{
    public class CsvExportService
    {
        public string ExportSessionsToCsv(List<TestSession> sessions)
        {
            var csvBuilder = new StringBuilder();

            // Заголовок CSV
            csvBuilder.AppendLine("Дата;Испытуемый;Возраст;Размер таблицы;Последовательность;ER;BP;IN;Общее время;Ошибки;Время таблицы 1;Время таблицы 2;Время таблицы 3;Время таблицы 4;Время таблицы 5;Ошибки таблицы 1;Ошибки таблицы 2;Ошибки таблицы 3;Ошибки таблицы 4;Ошибки таблицы 5");

            // Данные
            foreach (var session in sessions)
            {
                var line = new List<string>
                {
                    session.StartTime.ToString("dd.MM.yyyy HH:mm"),
                    EscapeCsvField(session.SubjectName ?? "Неизвестный"),
                    session.SubjectAge ?? "Не указан",
                    session.Configuration.TableSize.ToString(),
                    session.Configuration.SequenceType.ToString(),
                    session.Results?.EfficiencyRate.ToString("F2") ?? "0",
                    session.Results?.WorkabilityIndex.ToString("F2") ?? "0",
                    session.Results?.StabilityIndex.ToString("F2") ?? "0",
                    session.Results?.TotalTime.ToString("F2") ?? "0",
                    session.Results?.TotalErrors.ToString() ?? "0"
                };

                // Время по таблицам
                for (int i = 0; i < 5; i++)
                {
                    if (session.TableTimes != null && i < session.TableTimes.Count)
                        line.Add(session.TableTimes[i].ToString("F2"));
                    else
                        line.Add("0");
                }

                // Ошибки по таблицам
                for (int i = 0; i < 5; i++)
                {
                    if (session.ErrorCounts != null && i < session.ErrorCounts.Count)
                        line.Add(session.ErrorCounts[i].ToString());
                    else
                        line.Add("0");
                }

                csvBuilder.AppendLine(string.Join(";", line));
            }

            // Сохранение файла
            string fileName = $"results_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);

            File.WriteAllText(filePath, csvBuilder.ToString(), Encoding.UTF8);

            return filePath;
        }

        private string EscapeCsvField(string field)
        {
            if (field == null) return string.Empty;

            // Экранируем кавычки и добавляем кавычки если есть точка с запятой
            if (field.Contains(";") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r"))
            {
                field = field.Replace("\"", "\"\"");
                field = $"\"{field}\"";
            }
            return field;
        }

        public string ExportDetailedReport(TestSession session)
        {
            var csvBuilder = new StringBuilder();

            // Детальный отчет для одной сессии
            csvBuilder.AppendLine("Детальный отчет по тестированию");
            csvBuilder.AppendLine($"Испытуемый: {session.SubjectName}");
            csvBuilder.AppendLine($"Дата тестирования: {session.StartTime:dd.MM.yyyy HH:mm}");
            csvBuilder.AppendLine($"Размер таблицы: {session.Configuration.TableSize}");
            csvBuilder.AppendLine($"Последовательность: {session.Configuration.SequenceType}");
            csvBuilder.AppendLine();

            // Показатели
            csvBuilder.AppendLine("Показатель;Значение");
            if (session.Results != null)
            {
                csvBuilder.AppendLine($"ER (Эффективность работы);{session.Results.EfficiencyRate:F2}");
                csvBuilder.AppendLine($"BP (Врабатываемость);{session.Results.WorkabilityIndex:F2}");
                csvBuilder.AppendLine($"IN (Психическая устойчивость);{session.Results.StabilityIndex:F2}");
                csvBuilder.AppendLine($"Общее время;{session.Results.TotalTime:F2}");
                csvBuilder.AppendLine($"Общее количество ошибок;{session.Results.TotalErrors}");
            }
            csvBuilder.AppendLine();

            // Время по таблицам
            csvBuilder.AppendLine("Таблица;Время (сек);Ошибки");
            for (int i = 0; i < 5; i++)
            {
                string time = "0";
                string errors = "0";

                if (session.TableTimes != null && i < session.TableTimes.Count)
                    time = session.TableTimes[i].ToString("F2");

                if (session.ErrorCounts != null && i < session.ErrorCounts.Count)
                    errors = session.ErrorCounts[i].ToString();

                csvBuilder.AppendLine($"{i + 1};{time};{errors}");
            }

            string fileName = $"detailed_report_{session.SessionId}.csv";
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);

            File.WriteAllText(filePath, csvBuilder.ToString(), Encoding.UTF8);

            return filePath;
        }
    }
}
