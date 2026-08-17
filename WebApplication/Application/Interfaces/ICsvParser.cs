using WebApplication.Model.Entities;

namespace WebApplication.Application.Interfaces
{
    public interface ICsvParser
    {
        Task<IEnumerable<DataValue>> ParseCsvAsync(Stream stream, string fileName);
    }
}