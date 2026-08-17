using System.Globalization;
using Microsoft.EntityFrameworkCore;
using WebApplication.Application.Interfaces;
using WebApplication.Model;
using WebApplication.Model.Entities;

namespace WebApplication.Application.Services
{
    public class CsvParser : ICsvParser
    {
        private const int MaxRows = 10000;
        private const int MinRows = 1;
        private const int HeaderLineNumber = 1;
        private const int ExpectedColumnCount = 3;
        private const char ColumnSeparator = ';';
        private const string DateFormat = "yyyy-MM-dd'T'HH-mm-ss.FFFFFFF'Z'";
        private static readonly DateTime MinDate = new(2000, 1, 1);

        private readonly AppDbContext _context;

        public CsvParser(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DataValue>> ParseCsvAsync(Stream stream, string fileName)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var fileId = await GetOrCreateFileIdAsync(fileName);
                var values = await ReadValuesAsync(stream, fileId);

                ValidateRowCount(values.Count);

                _context.Values.AddRange(values);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return values;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task<int> GetOrCreateFileIdAsync(string fileName)
        {
            var existingFile = await _context.Files
                .FirstOrDefaultAsync(f => f.Name == fileName);

            if (existingFile != null)
            {
                await _context.Values.Where(v => v.FileId == existingFile.Id).ExecuteDeleteAsync();
                await _context.Results.Where(r => r.FileId == existingFile.Id).ExecuteDeleteAsync();
                return existingFile.Id;
            }

            var file = new UploadedFile { Name = fileName };
            _context.Files.Add(file);
            await _context.SaveChangesAsync();
            return file.Id;
        }

        private async Task<List<DataValue>> ReadValuesAsync(Stream stream, int fileId)
        {
            using var reader = new StreamReader(stream);
            var values = new List<DataValue>();
            var lineNumber = 0;

            while (await reader.ReadLineAsync() is { } line)
            {
                lineNumber++;
                if (lineNumber == HeaderLineNumber)
                    continue;

                var dataValue = ParseLine(line, lineNumber);
                dataValue.FileId = fileId;
                values.Add(dataValue);
            }

            return values;
        }

        private static DataValue ParseLine(string line, int lineNumber)
        {
            var parts = line.Split(ColumnSeparator);
            if (parts.Length != ExpectedColumnCount)
                throw new InvalidDataException($"Строка {lineNumber}: ожидается {ExpectedColumnCount} значения");

            var date = ParseDate(parts[0], lineNumber);
            var executionTime = ParseExecutionTime(parts[1], lineNumber);
            var value = ParseValue(parts[2], lineNumber);

            return new DataValue
            {
                Date = date,
                ExecutionTime = executionTime,
                Value = value
            };
        }

        private static DateTime ParseDate(string raw, int lineNumber)
        {
            if (!DateTime.TryParseExact(raw, DateFormat,
                    CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var date))
                throw new InvalidDataException($"Строка {lineNumber}: неверный формат даты");

            if (date < MinDate || date > DateTime.UtcNow)
                throw new InvalidDataException($"Строка {lineNumber}: дата вне допустимого диапазона");

            return date;
        }

        private static int ParseExecutionTime(string raw, int lineNumber)
        {
            if (!int.TryParse(raw, out var executionTime))
                throw new InvalidDataException($"Строка {lineNumber}: неверный формат ExecutionTime");

            if (executionTime < 0)
                throw new InvalidDataException($"Строка {lineNumber}: ExecutionTime не может быть меньше 0");

            return executionTime;
        }

        private static double ParseValue(string raw, int lineNumber)
        {
            if (!double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
                throw new InvalidDataException($"Строка {lineNumber}: неверный формат Value");

            if (value < 0)
                throw new InvalidDataException($"Строка {lineNumber}: Value не может быть меньше 0");

            return value;
        }

        private static void ValidateRowCount(int count)
        {
            if (count < MinRows || count > MaxRows)
                throw new InvalidDataException($"Количество строк должно быть от {MinRows} до {MaxRows}, получено: {count}");
        }
    }
}