using TelemetryAPI.Models;

namespace TelemetryAPI.Interfaces;

public interface IEnergyRepository
{
    Task<EnergyReading?> GetByIdAsync(int id);
    Task<IEnumerable<EnergyReading>> GetAllAsync();
    Task<IEnumerable<EnergyReading>> GetByLocationAsync(string location);
    Task<IEnumerable<EnergyReading>> GetAnomaliesAsync();
    Task<EnergyReading> AddAsync(EnergyReading reading);
}
