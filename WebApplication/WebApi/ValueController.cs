using Microsoft.AspNetCore.Mvc;
using WebApplication.Application.Interfaces;

namespace WebApplication.WebApi
{
    [ApiController]
    [Route("api/[controller]")]
    public class ValueController : ControllerBase
    {
        private readonly IValueFilter _valueFilter;

        public ValueController(IValueFilter valueFilter)
        {
            _valueFilter = valueFilter;
        }

        [HttpGet("values")]
        public async Task<IActionResult> GetTenValues([FromQuery] string FileName)
        {
            var result = await _valueFilter.GetTenLastValues(FileName);
            return Ok(result);
        }
    }
}