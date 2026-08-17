using System.ComponentModel.DataAnnotations;

namespace WebApplication.Application.DTOs
{
    public class UploadFileDto
    {
        [Required(ErrorMessage = "Файл обязателен")]
        public IFormFile? File { get; set; }
    }
}