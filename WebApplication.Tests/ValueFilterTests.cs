using WebApplication.Application.Services;
using WebApplication.Model;
using WebApplication.Model.Entities;

namespace WebApplication.Tests
{
    public class ValueFilterTests
    {
        [Fact]
        public async Task GetTenLastValues_ReturnsLastTenValuesOrderedByDateDescending()
        {
            // Arrange
            using var context = TestDbContextFactory.Create();
            var file = new UploadedFile { Name = "test.csv" };
            context.Files.Add(file);
            await context.SaveChangesAsync();

            for (int i = 1; i <= 15; i++)
            {
                context.Values.Add(new DataValue
                {
                    FileId = file.Id,
                    Date = new DateTime(2024, 1, i, 10, 0, 0, DateTimeKind.Utc),
                    ExecutionTime = i,
                    Value = i * 10
                });
            }
            await context.SaveChangesAsync();

            var filter = new ValueFilter(context);

            // Act
            var result = (await filter.GetTenLastValues("test.csv")).ToList();

            // Assert
            Assert.Equal(10, result.Count);
            Assert.Equal(15, result[0].ExecutionTime);
            Assert.Equal(150, result[0].Value);
            Assert.Equal(6, result[9].ExecutionTime);
            Assert.Equal(60, result[9].Value);
        }

        [Fact]
        public async Task GetTenLastValues_NoMatchingFile_ReturnsEmpty()
        {
            // Arrange
            using var context = TestDbContextFactory.Create();
            var filter = new ValueFilter(context);

            // Act
            var result = (await filter.GetTenLastValues("nonexistent.csv")).ToList();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetTenLastValues_LessThanTenValues_ReturnsAll()
        {
            // Arrange
            using var context = TestDbContextFactory.Create();
            var file = new UploadedFile { Name = "test.csv" };
            context.Files.Add(file);
            await context.SaveChangesAsync();

            context.Values.Add(new DataValue
            {
                FileId = file.Id,
                Date = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc),
                ExecutionTime = 1,
                Value = 10
            });
            context.Values.Add(new DataValue
            {
                FileId = file.Id,
                Date = new DateTime(2024, 1, 2, 10, 0, 0, DateTimeKind.Utc),
                ExecutionTime = 2,
                Value = 20
            });
            await context.SaveChangesAsync();

            var filter = new ValueFilter(context);

            // Act
            var result = (await filter.GetTenLastValues("test.csv")).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(2, result[0].ExecutionTime);
            Assert.Equal(1, result[1].ExecutionTime);
        }

        [Fact]
        public async Task GetTenLastValues_OnlyReturnsValuesForMatchingFile()
        {
            // Arrange
            using var context = TestDbContextFactory.Create();
            var file1 = new UploadedFile { Name = "file1.csv" };
            var file2 = new UploadedFile { Name = "file2.csv" };
            context.Files.AddRange(file1, file2);
            await context.SaveChangesAsync();

            context.Values.Add(new DataValue
            {
                FileId = file1.Id,
                Date = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc),
                ExecutionTime = 1,
                Value = 10
            });
            context.Values.Add(new DataValue
            {
                FileId = file2.Id,
                Date = new DateTime(2024, 1, 2, 10, 0, 0, DateTimeKind.Utc),
                ExecutionTime = 2,
                Value = 20
            });
            await context.SaveChangesAsync();

            var filter = new ValueFilter(context);

            // Act
            var result = (await filter.GetTenLastValues("file1.csv")).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal(1, result[0].ExecutionTime);
            Assert.Equal(10, result[0].Value);
        }
    }
}