using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebApplication.Application.DTOs;
using WebApplication.Application.Interfaces;
using WebApplication.Model.Entities;
using WebApplication.WebApi;

namespace WebApplication.Tests
{
    public class FileControllerTests
    {
        [Fact]
        public async Task UploadFile_ValidCsvFile_ReturnsOk()
        {
            // Arrange
            var csvParserMock = new Mock<ICsvParser>();
            var aggregationCalculatorMock = new Mock<IAggregationCalculator>();

            var values = new List<DataValue>
            {
                new() { FileId = 1, Date = DateTime.UtcNow, ExecutionTime = 10, Value = 100 }
            };
            csvParserMock.Setup(p => p.ParseCsvAsync(It.IsAny<Stream>(), It.IsAny<string>()))
                .ReturnsAsync(values);

            var controller = new FileController(csvParserMock.Object, aggregationCalculatorMock.Object);

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("test.csv");
            fileMock.Setup(f => f.Length).Returns(100);
            fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

            var dto = new UploadFileDto { File = fileMock.Object };

            // Act
            var result = await controller.UploadFile(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Файл успешно сохранён", okResult.Value);
            csvParserMock.Verify(p => p.ParseCsvAsync(It.IsAny<Stream>(), "test.csv"), Times.Once);
            aggregationCalculatorMock.Verify(c => c.CalculateAggregationAsync(values), Times.Once);
        }

        [Fact]
        public async Task UploadFile_NullFile_ReturnsBadRequest()
        {
            // Arrange
            var csvParserMock = new Mock<ICsvParser>();
            var aggregationCalculatorMock = new Mock<IAggregationCalculator>();
            var controller = new FileController(csvParserMock.Object, aggregationCalculatorMock.Object);

            var dto = new UploadFileDto { File = null };

            // Act
            var result = await controller.UploadFile(dto);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Файл не выбран или пуст", badRequest.Value);
            csvParserMock.Verify(p => p.ParseCsvAsync(It.IsAny<Stream>(), It.IsAny<string>()), Times.Never);
            aggregationCalculatorMock.Verify(c => c.CalculateAggregationAsync(It.IsAny<IEnumerable<DataValue>>()), Times.Never);
        }

        [Fact]
        public async Task UploadFile_EmptyFile_ReturnsBadRequest()
        {
            // Arrange
            var csvParserMock = new Mock<ICsvParser>();
            var aggregationCalculatorMock = new Mock<IAggregationCalculator>();
            var controller = new FileController(csvParserMock.Object, aggregationCalculatorMock.Object);

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("test.csv");
            fileMock.Setup(f => f.Length).Returns(0);

            var dto = new UploadFileDto { File = fileMock.Object };

            // Act
            var result = await controller.UploadFile(dto);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Файл не выбран или пуст", badRequest.Value);
        }

        [Fact]
        public async Task UploadFile_InvalidExtension_ReturnsBadRequest()
        {
            // Arrange
            var csvParserMock = new Mock<ICsvParser>();
            var aggregationCalculatorMock = new Mock<IAggregationCalculator>();
            var controller = new FileController(csvParserMock.Object, aggregationCalculatorMock.Object);

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("test.txt");
            fileMock.Setup(f => f.Length).Returns(100);

            var dto = new UploadFileDto { File = fileMock.Object };

            // Act
            var result = await controller.UploadFile(dto);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Недопустимое расширение файла", (string)badRequest.Value!);
            csvParserMock.Verify(p => p.ParseCsvAsync(It.IsAny<Stream>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task UploadFile_InvalidDataException_ReturnsBadRequest()
        {
            // Arrange
            var csvParserMock = new Mock<ICsvParser>();
            var aggregationCalculatorMock = new Mock<IAggregationCalculator>();

            csvParserMock.Setup(p => p.ParseCsvAsync(It.IsAny<Stream>(), It.IsAny<string>()))
                .ThrowsAsync(new InvalidDataException("Ошибка данных"));

            var controller = new FileController(csvParserMock.Object, aggregationCalculatorMock.Object);

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("test.csv");
            fileMock.Setup(f => f.Length).Returns(100);
            fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

            var dto = new UploadFileDto { File = fileMock.Object };

            // Act
            var result = await controller.UploadFile(dto);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Ошибка данных", badRequest.Value);
        }

        [Fact]
        public async Task UploadFile_ArgumentException_ReturnsBadRequest()
        {
            // Arrange
            var csvParserMock = new Mock<ICsvParser>();
            var aggregationCalculatorMock = new Mock<IAggregationCalculator>();

            csvParserMock.Setup(p => p.ParseCsvAsync(It.IsAny<Stream>(), It.IsAny<string>()))
                .ThrowsAsync(new ArgumentException("Ошибка аргумента"));

            var controller = new FileController(csvParserMock.Object, aggregationCalculatorMock.Object);

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("test.csv");
            fileMock.Setup(f => f.Length).Returns(100);
            fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

            var dto = new UploadFileDto { File = fileMock.Object };

            // Act
            var result = await controller.UploadFile(dto);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Ошибка аргумента", badRequest.Value);
        }

        [Fact]
        public async Task UploadFile_GenericException_ReturnsInternalServerError()
        {
            // Arrange
            var csvParserMock = new Mock<ICsvParser>();
            var aggregationCalculatorMock = new Mock<IAggregationCalculator>();

            csvParserMock.Setup(p => p.ParseCsvAsync(It.IsAny<Stream>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Внутренняя ошибка"));

            var controller = new FileController(csvParserMock.Object, aggregationCalculatorMock.Object);

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("test.csv");
            fileMock.Setup(f => f.Length).Returns(100);
            fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

            var dto = new UploadFileDto { File = fileMock.Object };

            // Act
            var result = await controller.UploadFile(dto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Contains("Внутренняя ошибка", (string)statusCodeResult.Value!);
        }
    }
}