using TelemetryAPI.Models;

namespace TelemetryAPI.Interfaces;

public interface ITelemetryRepository
{
    Task<TelemetryReading?> GetByIdAsync(int id);
    Task<IEnumerable<TelemetryReading>> GetAllAsync();
    Task<IEnumerable<TelemetryReading>> GetByOperatorAsync(string operatorId);
    Task<IEnumerable<TelemetryReading>> GetByDateRangeAsync(DateTime from, DateTime to);
    Task<IEnumerable<TelemetryReading>> GetAnomaliesAsync();
    Task<TelemetryReading> AddAsync(TelemetryReading reading);
    Task UpdateAsync(TelemetryReading reading);
    Task DeleteAsync(int id);
}
