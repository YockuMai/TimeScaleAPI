using WebApplication.Model.Entities;

namespace WebApplication.Application.Interfaces
{
    public interface IAggregationCalculator
    {
        Task CalculateAggregationAsync(IEnumerable<DataValue> values);
    }
}