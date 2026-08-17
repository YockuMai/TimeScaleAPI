using WebApplication.Application.DTOs;
using WebApplication.Application.Services;
using WebApplication.Model;
using WebApplication.Model.Entities;

namespace WebApplication.Tests
{
    public class AggregationFilterTests
    {
        private static async Task<AppDbContext> CreateContextWithResults()
        {
            var context = TestDbContextFactory.Create();

            var file1 = new UploadedFile { Name = "file1.csv" };
            var file2 = new UploadedFile { Name = "file2.csv" };
            var file3 = new UploadedFile { Name = "file3.csv" };
            context.Files.AddRange(file1, file2, file3);
            await context.SaveChangesAsync();

            context.Results.AddRange(
                new AggregationResult
                {
                    FileId = file1.Id,
                    DeltaDate = 100,
                    MinDate = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc),
                    MeanExecutionTime = 10,
                    MeanValue = 100,
                    MedianValue = 100,
                    MaxValue = 200,
                    MinValue = 50
                },
                new AggregationResult
                {
                    FileId = file2.Id,
                    DeltaDate = 200,
                    MinDate = new DateTime(2024, 2, 1, 10, 0, 0, DateTimeKind.Utc),
                    MeanExecutionTime = 20,
                    MeanValue = 200,
                    MedianValue = 200,
                    MaxValue = 300,
                    MinValue = 100
                },
                new AggregationResult
                {
                    FileId = file3.Id,
                    DeltaDate = 300,
                    MinDate = new DateTime(2024, 3, 1, 10, 0, 0, DateTimeKind.Utc),
                    MeanExecutionTime = 30,
                    MeanValue = 300,
                    MedianValue = 300,
                    MaxValue = 400,
                    MinValue = 200
                }
            );
            await context.SaveChangesAsync();

            return context;
        }

        [Fact]
        public async Task Apply_NoFilter_ReturnsAllResults()
        {
            // Arrange
            using var context = await CreateContextWithResults();
            var filter = new AggregationFilter(context);

            // Act
            var result = (await filter.Apply(new ResultFilterDto())).ToList();

            // Assert
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task Apply_FilterByFileName_ReturnsOnlyMatchingResults()
        {
            // Arrange
            using var context = await CreateContextWithResults();
            var filter = new AggregationFilter(context);

            // Act
            var result = (await filter.Apply(new ResultFilterDto { FileName = "file1.csv" })).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal("file1.csv", result[0].FileName);
        }

        [Fact]
        public async Task Apply_FilterByFirstOperationFrom_ReturnsResultsWithMinDateAfter()
        {
            // Arrange
            using var context = await CreateContextWithResults();
            var filter = new AggregationFilter(context);

            // Act
            var result = (await filter.Apply(new ResultFilterDto
            {
                FirstOperationFrom = new DateTime(2024, 2, 1, 10, 0, 0, DateTimeKind.Utc)
            })).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, r => Assert.True(r.MinDate >= new DateTime(2024, 2, 1, 10, 0, 0, DateTimeKind.Utc)));
        }

        [Fact]
        public async Task Apply_FilterByFirstOperationTo_ReturnsResultsWithMinDateBefore()
        {
            // Arrange
            using var context = await CreateContextWithResults();
            var filter = new AggregationFilter(context);

            // Act
            var result = (await filter.Apply(new ResultFilterDto
            {
                FirstOperationTo = new DateTime(2024, 2, 1, 10, 0, 0, DateTimeKind.Utc)
            })).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, r => Assert.True(r.MinDate <= new DateTime(2024, 2, 1, 10, 0, 0, DateTimeKind.Utc)));
        }

        [Fact]
        public async Task Apply_FilterByMeanValueFrom_ReturnsResultsWithMeanValueAbove()
        {
            // Arrange
            using var context = await CreateContextWithResults();
            var filter = new AggregationFilter(context);

            // Act
            var result = (await filter.Apply(new ResultFilterDto { MeanValueFrom = 200 })).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, r => Assert.True(r.MeanValue >= 200));
        }

        [Fact]
        public async Task Apply_FilterByMeanValueTo_ReturnsResultsWithMeanValueBelow()
        {
            // Arrange
            using var context = await CreateContextWithResults();
            var filter = new AggregationFilter(context);

            // Act
            var result = (await filter.Apply(new ResultFilterDto { MeanValueTo = 200 })).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, r => Assert.True(r.MeanValue <= 200));
        }

        [Fact]
        public async Task Apply_FilterByMeanExecutionTimeFrom_ReturnsResultsWithMeanExecutionTimeAbove()
        {
            // Arrange
            using var context = await CreateContextWithResults();
            var filter = new AggregationFilter(context);

            // Act
            var result = (await filter.Apply(new ResultFilterDto { MeanExecutionTimeFrom = 20 })).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, r => Assert.True(r.MeanExecutionTime >= 20));
        }

        [Fact]
        public async Task Apply_FilterByMeanExecutionTimeTo_ReturnsResultsWithMeanExecutionTimeBelow()
        {
            // Arrange
            using var context = await CreateContextWithResults();
            var filter = new AggregationFilter(context);

            // Act
            var result = (await filter.Apply(new ResultFilterDto { MeanExecutionTimeTo = 20 })).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, r => Assert.True(r.MeanExecutionTime <= 20));
        }

        [Fact]
        public async Task Apply_CombinedFilters_ReturnsMatchingResults()
        {
            // Arrange
            using var context = await CreateContextWithResults();
            var filter = new AggregationFilter(context);

            // Act
            var result = (await filter.Apply(new ResultFilterDto
            {
                FileName = "file1.csv",
                MeanValueFrom = 100,
                MeanValueTo = 200
            })).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal("file1.csv", result[0].FileName);
            Assert.True(result[0].MeanValue >= 100 && result[0].MeanValue <= 200);
        }

        [Fact]
        public async Task Apply_NoMatchingResults_ReturnsEmpty()
        {
            // Arrange
            using var context = await CreateContextWithResults();
            var filter = new AggregationFilter(context);

            // Act
            var result = (await filter.Apply(new ResultFilterDto { FileName = "nonexistent.csv" })).ToList();

            // Assert
            Assert.Empty(result);
        }
    }
}