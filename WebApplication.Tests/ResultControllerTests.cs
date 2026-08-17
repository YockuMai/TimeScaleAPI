using Microsoft.AspNetCore.Mvc;
using Moq;
using WebApplication.Application.DTOs;
using WebApplication.Application.Interfaces;
using WebApplication.WebApi;

namespace WebApplication.Tests
{
    public class ResultControllerTests
    {
        [Fact]
        public async Task GetResult_ReturnsOkWithResults()
        {
            // Arrange
            var aggregationFilterMock = new Mock<IAggregationFilter>();
            var expected = new List<AggregationResultDto>
            {
                new()
                {
                    FileId = 1,
                    FileName = "test.csv",
                    DeltaDate = 100,
                    MinDate = DateTime.UtcNow,
                    MeanExecutionTime = 10,
                    MeanValue = 100,
                    MedianValue = 100,
                    MaxValue = 200,
                    MinValue = 50
                }
            };
            aggregationFilterMock.Setup(f => f.Apply(It.IsAny<ResultFilterDto>()))
                .ReturnsAsync(expected);

            var controller = new ResultController(aggregationFilterMock.Object);

            // Act
            var result = await controller.GetResult(new ResultFilterDto());

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var results = Assert.IsAssignableFrom<IEnumerable<AggregationResultDto>>(okResult.Value);
            Assert.Single(results);
            aggregationFilterMock.Verify(f => f.Apply(It.IsAny<ResultFilterDto>()), Times.Once);
        }

        [Fact]
        public async Task GetResult_EmptyResult_ReturnsOkWithEmptyList()
        {
            // Arrange
            var aggregationFilterMock = new Mock<IAggregationFilter>();
            aggregationFilterMock.Setup(f => f.Apply(It.IsAny<ResultFilterDto>()))
                .ReturnsAsync(new List<AggregationResultDto>());

            var controller = new ResultController(aggregationFilterMock.Object);

            // Act
            var result = await controller.GetResult(new ResultFilterDto());

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var results = Assert.IsAssignableFrom<IEnumerable<AggregationResultDto>>(okResult.Value);
            Assert.Empty(results);
        }

        [Fact]
        public async Task GetResult_PassesFilterToService()
        {
            // Arrange
            var aggregationFilterMock = new Mock<IAggregationFilter>();
            aggregationFilterMock.Setup(f => f.Apply(It.IsAny<ResultFilterDto>()))
                .ReturnsAsync(new List<AggregationResultDto>());

            var controller = new ResultController(aggregationFilterMock.Object);
            var filter = new ResultFilterDto { FileName = "test.csv", MeanValueFrom = 100 };

            // Act
            await controller.GetResult(filter);

            // Assert
            aggregationFilterMock.Verify(f => f.Apply(filter), Times.Once);
        }
    }
}