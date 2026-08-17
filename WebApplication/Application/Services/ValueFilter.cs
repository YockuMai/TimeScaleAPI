using Microsoft.EntityFrameworkCore;
using WebApplication.Application.DTOs;
using WebApplication.Application.Interfaces;
using WebApplication.Model;

namespace WebApplication.Application.Services
{
    public class ValueFilter : IValueFilter
    {
        private readonly AppDbContext _context;

        public ValueFilter(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ValuesResponseDto>> GetTenLastValues(string FileName)
        {
            return await _context.Values
                .Where(v => v.File != null && v.File.Name.Equals(FileName))
                .OrderByDescending(v => v.Date)
                .Take(10)
                .Select(v => new ValuesResponseDto
                {
                    Date = v.Date,
                    ExecutionTime = v.ExecutionTime,
                    Value = v.Value
                })
                .ToListAsync();
        }
    }
}