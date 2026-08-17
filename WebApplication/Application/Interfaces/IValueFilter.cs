using WebApplication.Application.DTOs;

namespace WebApplication.Application.Interfaces
{
    public interface IValueFilter
    {
        Task<IEnumerable<ValuesResponseDto>> GetTenLastValues(string FileName);
    }
}