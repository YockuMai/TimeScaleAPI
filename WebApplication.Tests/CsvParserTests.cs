using System.Text;
using WebApplication.Application.Services;
using WebApplication.Model;

namespace WebApplication.Tests
{
    public class CsvParserTests
    {
        [Fact]
        public async Task ParseCsvAsync_ValidCsv_ReturnsValuesAndSavesToDb()
        {
            // Arrange
            using var context = TestDbContextFactory.Create();
            var parser = new CsvParser(context);
            var csv = "Date;ExecutionTime;Value\n" +
                      "2024-01-01T10-00-00.000Z;10;100.5\n" +
                      "2024-01-02T11-00-00.000Z;20;200.5\n";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

            // Act
            var result = (await parser.ParseCsvAsync(stream, "test.csv")).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(10, result[0].ExecutionTime);
            Assert.Equal(100.5, result[0].Value);
            Assert.Equal(20, result[1].ExecutionTime);
            Assert.Equal(200.5, result[1].Value);
            Assert.Equal(1, context.Files.Count());
            Assert.Equal(2, context.Values.Count());
        }

        [Fact]
        public async Task ParseCsvAsync_ExistingFile_DeletesOldValuesAndResults()
        {
            // Arrange
            using var context = TestDbContextFactory.Create();
            var file = new Model.Entities.UploadedFile { Name = "test.csv" };
            context.Files.Add(file);
            await context.SaveChangesAsync();

            context.Values.Add(new Model.Entities.DataValue { FileId = file.Id, Date = DateTime.UtcNow, ExecutionTime = 1, Value = 1 });
            context.Results.Add(new Model.Entities.AggregationResult { FileId = file.Id, DeltaDate = 1, MinDate = DateTime.UtcNow, MeanExecutionTime = 1, MeanValue = 1, MedianValue = 1, MaxValue = 1, MinValue = 1 });
            await context.SaveChangesAsync();

            var parser = new CsvParser(context);
            var csv = "Date;ExecutionTime;Value\n" +
                      "2024-01-01T10-00-00.000Z;10;100.5\n";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

            // Act
            var result = (await parser.ParseCsvAsync(stream, "test.csv")).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal(1, context.Files.Count());
            Assert.Single(context.Values);
            Assert.Empty(context.Results);
        }

        [Fact]
        public async Task ParseCsvAsync_InvalidColumnCount_ThrowsInvalidDataException()
        {
            // Arrange
            using var context = TestDbContextFactory.Create();
            var parser = new CsvParser(context);
            var csv = "Date;ExecutionTime;Value\n" +
                      "2024-01-01T10-00-00.000Z;10\n";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidDataException>(() => parser.ParseCsvAsync(stream, "test.csv"));
            Assert.Contains("ожидается 3 значения", ex.Message);
        }

        [Fact]
        public async Task ParseCsvAsync_InvalidDateFormat_ThrowsInvalidDataException()
        {
            // Arrange
            using var context = TestDbContextFactory.Create();
            var parser = new CsvParser(context);
            var csv = "Date;ExecutionTime;Value\n" +
                      "invalid-date;10;100.5\n";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidDataException>(() => parser.ParseCsvAsync(stream, "test.csv"));
            Assert.Contains("неверный формат даты", ex.Message);
        }

        [Fact]
        public async Task ParseCsvAsync_DateBeforeMinDate_ThrowsInvalidDataException()
        {
            // Arrange
            using var context = TestDbContextFactory.Create();
            var parser = new CsvParser(context);
            var csv = "Date;ExecutionTime;Value\n" +
                      "1999-12-31T10-00-00.000Z;10;100.5\n";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidDataException>(() => parser.ParseCsvAsync(stream, "test.csv"));
            Assert.Contains("дата вне допустимого диапазона", ex.Message);
        }

        [Fact]
        public async Task ParseCsvAsync_DateInFuture_ThrowsInvalidDataException()
        {
            // Arrange
            using var context = TestDbContextFactory.Create();
            var parser = new CsvParser(context);
            var csv = "Date;ExecutionTime;Value\n" +
                      "2999-01-01T10-00-00.000Z;10;100.5\n";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidDataException>(() => parser.ParseCsvAsync(stream, "test.csv"));
            Assert.Contains("дата вне допустимого диапазона", ex.Message);
        }

        [Fact]
        public async Task ParseCsvAsync_InvalidExecutionTime_ThrowsInvalidDataException()
        {
            // Arrange
            using var context = TestDbContextFactory.Create();
            var parser = new CsvParser(context);
            var csv = "Date;ExecutionTime;Value\n" +
                      "2024-01-01T10-00-00.000Z;abc;100.5\n";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidDataException>(() => parser.ParseCsvAsync(stream, "test.csv"));
            Assert.Contains("неверный формат ExecutionTime", ex.Message);
        }

        [Fact]
        public async Task ParseCsvAsync_NegativeExecutionTime_ThrowsInvalidDataException()
        {
            // Arrange
            using var context = TestDbContextFactory.Create();
            var parser = new CsvParser(context);
            var csv = "Date;ExecutionTime;Value\n" +
                      "2024-01-01T10-00-00.000Z;-5;100.5\n";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidDataException>(() => parser.ParseCsvAsync(stream, "test.csv"));
            Assert.Contains("ExecutionTime не может быть меньше 0", ex.Message);
        }

        [Fact]
        public async Task ParseCsvAsync_InvalidValue_ThrowsInvalidDataException()
        {
            // Arrange
            using var context = TestDbContextFactory.Create();
            var parser = new CsvParser(context);
            var csv = "Date;ExecutionTime;Value\n" +
                      "2024-01-01T10-00-00.000Z;10;abc\n";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidDataException>(() => parser.ParseCsvAsync(stream, "test.csv"));
            Assert.Contains("неверный формат Value", ex.Message);
        }

        [Fact]
        public async Task ParseCsvAsync_NegativeValue_ThrowsInvalidDataException()
        {
            // Arrange
            using var context = TestDbContextFactory.Create();
            var parser = new CsvParser(context);
            var csv = "Date;ExecutionTime;Value\n" +
                      "2024-01-01T10-00-00.000Z;10;-100.5\n";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidDataException>(() => parser.ParseCsvAsync(stream, "test.csv"));
            Assert.Contains("Value не может быть меньше 0", ex.Message);
        }

        [Fact]
        public async Task ParseCsvAsync_EmptyFile_ThrowsInvalidDataException()
        {
            // Arrange
            using var context = TestDbContextFactory.Create();
            var parser = new CsvParser(context);
            var csv = "Date;ExecutionTime;Value\n";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidDataException>(() => parser.ParseCsvAsync(stream, "test.csv"));
            Assert.Contains("Количество строк должно быть от 1 до 10000", ex.Message);
        }

        [Fact]
        public async Task ParseCsvAsync_TooManyRows_ThrowsInvalidDataException()
        {
            // Arrange
            using var context = TestDbContextFactory.Create();
            var parser = new CsvParser(context);
            var sb = new StringBuilder();
            sb.AppendLine("Date;ExecutionTime;Value");
            for (int i = 0; i < 10001; i++)
            {
                sb.AppendLine($"2024-01-01T10-00-00.000Z;10;100.5");
            }
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString()));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidDataException>(() => parser.ParseCsvAsync(stream, "test.csv"));
            Assert.Contains("Количество строк должно быть от 1 до 10000", ex.Message);
        }
    }
}