using WebApplication.Application.DTOs;

namespace WebApplication.Application.Interfaces
{
    public interface IAggregationFilter
    {
        Task<IEnumerable<AggregationResultDto>> Apply(ResultFilterDto filter);
    }
}