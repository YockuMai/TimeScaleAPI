using WebApplication.Application.Interfaces;
using WebApplication.Model;
using WebApplication.Model.Entities;

namespace WebApplication.Application.Services
{
    public class AggregationCalculator : IAggregationCalculator
    {
        private readonly AppDbContext _context;

        public AggregationCalculator(AppDbContext context)
        {
            _context = context;
        }

        public async Task CalculateAggregationAsync(IEnumerable<DataValue> values)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var list = values.ToList();
                if (list.Count == 0)
                    throw new ArgumentException("Значения пусты");
    
                var result = new AggregationResult
                {
                    FileId = list[0].FileId,
                    DeltaDate = (int)(list.Max(v => v.Date) - list.Min(v => v.Date)).TotalSeconds,
                    MinDate = list.Min(v => v.Date),
                    MeanExecutionTime = list.Average(v => v.ExecutionTime),
                    MeanValue = list.Average(v => v.Value),
                    MedianValue = CalculateMedian(list),
                    MinValue = list.Min(v => v.Value),
                    MaxValue = list.Max(v => v.Value)
                };
                _context.Results.Add(result);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    
        private static double CalculateMedian(List<DataValue> values)
        {
            var sorted = values.OrderBy(v => v.Value).Select(v => v.Value).ToList();
            var count = sorted.Count;
            if (count % 2 == 1)
                return sorted[count / 2];
    
            return (sorted[count / 2 - 1] + sorted[count / 2]) / 2.0;
        }
    }
}