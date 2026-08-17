using WebApplication.Application.Services;
using WebApplication.Model;
using WebApplication.Model.Entities;

namespace WebApplication.Tests
{
    public class AggregationCalculatorTests
    {
        private static async Task<(AppDbContext context, int fileId)> CreateContextWithFile()
        {
            var context = TestDbContextFactory.Create();
            var file = new UploadedFile { Name = "test.csv" };
            context.Files.Add(file);
            await context.SaveChangesAsync();
            return (context, file.Id);
        }

        [Fact]
        public async Task CalculateAggregationAsync_ValidValues_SavesAggregationResult()
        {
            // Arrange
            var (context, fileId) = await CreateContextWithFile();
            var calculator = new AggregationCalculator(context);

            var values = new List<DataValue>
            {
                new() { FileId = fileId, Date = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc), ExecutionTime = 10, Value = 100 },
                new() { FileId = fileId, Date = new DateTime(2024, 1, 3, 10, 0, 0, DateTimeKind.Utc), ExecutionTime = 20, Value = 200 },
                new() { FileId = fileId, Date = new DateTime(2024, 1, 2, 10, 0, 0, DateTimeKind.Utc), ExecutionTime = 30, Value = 300 }
            };

            // Act
            await calculator.CalculateAggregationAsync(values);

            // Assert
            var result = Assert.Single(context.Results);
            Assert.Equal(fileId, result.FileId);
            Assert.Equal(172800, result.DeltaDate); // 2 days in seconds
            Assert.Equal(new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc), result.MinDate);
            Assert.Equal(20, result.MeanExecutionTime);
            Assert.Equal(200, result.MeanValue);
            Assert.Equal(200, result.MedianValue);
            Assert.Equal(100, result.MinValue);
            Assert.Equal(300, result.MaxValue);
        }

        [Fact]
        public async Task CalculateAggregationAsync_EvenNumberOfValues_CalculatesMedianAsAverage()
        {
            // Arrange
            var (context, fileId) = await CreateContextWithFile();
            var calculator = new AggregationCalculator(context);

            var values = new List<DataValue>
            {
                new() { FileId = fileId, Date = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc), ExecutionTime = 10, Value = 100 },
                new() { FileId = fileId, Date = new DateTime(2024, 1, 2, 10, 0, 0, DateTimeKind.Utc), ExecutionTime = 20, Value = 200 },
                new() { FileId = fileId, Date = new DateTime(2024, 1, 3, 10, 0, 0, DateTimeKind.Utc), ExecutionTime = 30, Value = 300 },
                new() { FileId = fileId, Date = new DateTime(2024, 1, 4, 10, 0, 0, DateTimeKind.Utc), ExecutionTime = 40, Value = 400 }
            };

            // Act
            await calculator.CalculateAggregationAsync(values);

            // Assert
            var result = Assert.Single(context.Results);
            Assert.Equal(250, result.MedianValue); // (200 + 300) / 2
        }

        [Fact]
        public async Task CalculateAggregationAsync_OddNumberOfValues_CalculatesMedianAsMiddle()
        {
            // Arrange
            var (context, fileId) = await CreateContextWithFile();
            var calculator = new AggregationCalculator(context);

            var values = new List<DataValue>
            {
                new() { FileId = fileId, Date = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc), ExecutionTime = 10, Value = 100 },
                new() { FileId = fileId, Date = new DateTime(2024, 1, 2, 10, 0, 0, DateTimeKind.Utc), ExecutionTime = 20, Value = 200 },
                new() { FileId = fileId, Date = new DateTime(2024, 1, 3, 10, 0, 0, DateTimeKind.Utc), ExecutionTime = 30, Value = 300 }
            };

            // Act
            await calculator.CalculateAggregationAsync(values);

            // Assert
            var result = Assert.Single(context.Results);
            Assert.Equal(200, result.MedianValue);
        }

        [Fact]
        public async Task CalculateAggregationAsync_EmptyValues_ThrowsArgumentException()
        {
            // Arrange
            var (context, _) = await CreateContextWithFile();
            var calculator = new AggregationCalculator(context);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => calculator.CalculateAggregationAsync(new List<DataValue>()));
            Assert.Equal("Значения пусты", ex.Message);
        }

        [Fact]
        public async Task CalculateAggregationAsync_SingleValue_CalculatesCorrectly()
        {
            // Arrange
            var (context, fileId) = await CreateContextWithFile();
            var calculator = new AggregationCalculator(context);

            var values = new List<DataValue>
            {
                new() { FileId = fileId, Date = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc), ExecutionTime = 10, Value = 100 }
            };

            // Act
            await calculator.CalculateAggregationAsync(values);

            // Assert
            var result = Assert.Single(context.Results);
            Assert.Equal(0, result.DeltaDate);
            Assert.Equal(10, result.MeanExecutionTime);
            Assert.Equal(100, result.MeanValue);
            Assert.Equal(100, result.MedianValue);
            Assert.Equal(100, result.MinValue);
            Assert.Equal(100, result.MaxValue);
        }
    }
}