using Microsoft.AspNetCore.Mvc;
using WebApplication.Application.DTOs;
using WebApplication.Application.Interfaces;

namespace WebApplication.WebApi
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResultController : ControllerBase
    {
        private readonly IAggregationFilter _aggregationFilter;

        public ResultController(IAggregationFilter aggregationFilter)
        {
            _aggregationFilter = aggregationFilter;
        }

        [HttpGet("result")]
        public async Task<IActionResult> GetResult([FromQuery] ResultFilterDto filter)
        {
            var results = await _aggregationFilter.Apply(filter);
            return Ok(results);
        }
    }
}