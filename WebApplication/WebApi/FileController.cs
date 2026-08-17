using Microsoft.AspNetCore.Mvc;
using WebApplication.Application.DTOs;
using WebApplication.Application.Interfaces;

namespace WebApplication.WebApi
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileController : ControllerBase
    {
        private const string AllowedExtension = ".csv";
    
        private readonly ICsvParser _csvParser;
        private readonly IAggregationCalculator _aggregationCalculator;
    
        public FileController(ICsvParser csvParser, IAggregationCalculator aggregationCalculator)
        {
            _csvParser = csvParser;
            _aggregationCalculator = aggregationCalculator;
        }
    
        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile([FromForm] UploadFileDto dto)
        {
            if (dto.File == null || dto.File.Length == 0)
            {
                return BadRequest("Файл не выбран или пуст");
            }
    
            var extension = Path.GetExtension(dto.File.FileName);
            if (!string.Equals(extension, AllowedExtension, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest($"Недопустимое расширение файла. Ожидается: {AllowedExtension}");
            }
    
            try
            {
                using var stream = dto.File.OpenReadStream();
                var values = (await _csvParser.ParseCsvAsync(stream, dto.File.FileName)).ToList();
                await _aggregationCalculator.CalculateAggregationAsync(values);
    
                return Ok("Файл успешно сохранён");
            }
            catch (InvalidDataException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Произошла внутренняя ошибка сервера: " + ex.Message);
            }
        }
    }
}