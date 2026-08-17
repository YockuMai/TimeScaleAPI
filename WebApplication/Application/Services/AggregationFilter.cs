using Microsoft.EntityFrameworkCore;
using WebApplication.Application.DTOs;
using WebApplication.Application.Interfaces;
using WebApplication.Model;
using WebApplication.Model.Entities;

namespace WebApplication.Application.Services
{
    public class AggregationFilter : IAggregationFilter
    {
        private readonly AppDbContext _context;

        public AggregationFilter(AppDbContext context)
        {
            _context = context;
        }

        private IQueryable<AggregationResult> Filter(ResultFilterDto filter)
        {
            IQueryable<AggregationResult> query = _context.Results.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.FileName))
            {
                query = query.Where(r => r.File != null && r.File.Name.Equals(filter.FileName));
            }

            if (filter.FirstOperationFrom.HasValue)
            {
                query = query.Where(r => r.MinDate >= filter.FirstOperationFrom.Value);
            }

            if (filter.FirstOperationTo.HasValue)
            {
                query = query.Where(r => r.MinDate <= filter.FirstOperationTo.Value);
            }

            if (filter.MeanValueFrom.HasValue)
            {
                query = query.Where(r => r.MeanValue >= filter.MeanValueFrom.Value);
            }

            if (filter.MeanValueTo.HasValue)
            {
                query = query.Where(r => r.MeanValue <= filter.MeanValueTo.Value);
            }

            if (filter.MeanExecutionTimeFrom.HasValue)
            {
                query = query.Where(r => r.MeanExecutionTime >= filter.MeanExecutionTimeFrom.Value);
            }

            if (filter.MeanExecutionTimeTo.HasValue)
            {
                query = query.Where(r => r.MeanExecutionTime <= filter.MeanExecutionTimeTo.Value);
            }

            return query;
        }

        public async Task<IEnumerable<AggregationResultDto>> Apply(ResultFilterDto filter)
        {
            return await Filter(filter)
                .Select(r => new AggregationResultDto
                {
                    FileId = r.FileId,
                    FileName = r.File != null ? r.File.Name : string.Empty,
                    DeltaDate = r.DeltaDate,
                    MinDate = r.MinDate,
                    MeanExecutionTime = r.MeanExecutionTime,
                    MeanValue = r.MeanValue,
                    MedianValue = r.MedianValue,
                    MaxValue = r.MaxValue,
                    MinValue = r.MinValue
                })
                .ToListAsync();
        }
    }
}