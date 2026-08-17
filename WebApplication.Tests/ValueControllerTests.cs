using Microsoft.AspNetCore.Mvc;
using Moq;
using WebApplication.Application.DTOs;
using WebApplication.Application.Interfaces;
using WebApplication.WebApi;

namespace WebApplication.Tests
{
    public class ValueControllerTests
    {
        [Fact]
        public async Task GetTenValues_ReturnsOkWithValues()
        {
            // Arrange
            var valueFilterMock = new Mock<IValueFilter>();
            var expected = new List<ValuesResponseDto>
            {
                new() { Date = DateTime.UtcNow, ExecutionTime = 10, Value = 100 },
                new() { Date = DateTime.UtcNow.AddDays(-1), ExecutionTime = 20, Value = 200 }
            };
            valueFilterMock.Setup(f => f.GetTenLastValues("test.csv"))
                .ReturnsAsync(expected);

            var controller = new ValueController(valueFilterMock.Object);

            // Act
            var result = await controller.GetTenValues("test.csv");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var values = Assert.IsAssignableFrom<IEnumerable<ValuesResponseDto>>(okResult.Value);
            Assert.Equal(2, values.Count());
            valueFilterMock.Verify(f => f.GetTenLastValues("test.csv"), Times.Once);
        }

        [Fact]
        public async Task GetTenValues_EmptyResult_ReturnsOkWithEmptyList()
        {
            // Arrange
            var valueFilterMock = new Mock<IValueFilter>();
            valueFilterMock.Setup(f => f.GetTenLastValues(It.IsAny<string>()))
                .ReturnsAsync(new List<ValuesResponseDto>());

            var controller = new ValueController(valueFilterMock.Object);

            // Act
            var result = await controller.GetTenValues("nonexistent.csv");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var values = Assert.IsAssignableFrom<IEnumerable<ValuesResponseDto>>(okResult.Value);
            Assert.Empty(values);
        }
    }
}