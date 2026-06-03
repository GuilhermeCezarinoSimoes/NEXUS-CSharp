using Microsoft.EntityFrameworkCore;
using TelemetryAPI.Data;
using TelemetryAPI.Interfaces;
using TelemetryAPI.Models;

namespace TelemetryAPI.Repositories;

public class EnergyRepository : IEnergyRepository
{
    private readonly TelemetryDbContext _context;

    public EnergyRepository(TelemetryDbContext context) => _context = context;

    public async Task<EnergyReading?> GetByIdAsync(int id) =>
        await _context.EnergyReadings.FindAsync(id);

    public async Task<IEnumerable<EnergyReading>> GetAllAsync() =>
        await _context.EnergyReadings.OrderByDescending(r => r.Timestamp).ToListAsync();

    public async Task<IEnumerable<EnergyReading>> GetByLocationAsync(string location) =>
        await _context.EnergyReadings
            .Where(r => r.BaseLocation == location)
            .OrderByDescending(r => r.Timestamp)
            .ToListAsync();

    public async Task<IEnumerable<EnergyReading>> GetAnomaliesAsync() =>
        await _context.EnergyReadings
            .Where(r => r.EnergyLevel < 15.0 || !r.GeneratorOnline || r.VoltageV < 110.0 || r.VoltageV > 240.0)
            .OrderByDescending(r => r.Timestamp)
            .ToListAsync();

    public async Task<EnergyReading> AddAsync(EnergyReading reading)
    {
        reading.Timestamp = DateTime.UtcNow;
        _context.EnergyReadings.Add(reading);
        await _context.SaveChangesAsync();
        return reading;
    }
}
