using Microsoft.EntityFrameworkCore;
using TelemetryAPI.Data;
using TelemetryAPI.Interfaces;
using TelemetryAPI.Models;

namespace TelemetryAPI.Repositories;

public class TelemetryRepository : ITelemetryRepository
{
    private readonly TelemetryDbContext _context;

    public TelemetryRepository(TelemetryDbContext context)
    {
        _context = context;
    }

    public async Task<TelemetryReading?> GetByIdAsync(int id) =>
        await _context.TelemetryReadings.FindAsync(id);

    public async Task<IEnumerable<TelemetryReading>> GetAllAsync() =>
        await _context.TelemetryReadings
            .OrderByDescending(r => r.Timestamp)
            .ToListAsync();

    public async Task<IEnumerable<TelemetryReading>> GetByOperatorAsync(string operatorId) =>
        await _context.TelemetryReadings
            .Where(r => r.OperatorId == operatorId)
            .OrderByDescending(r => r.Timestamp)
            .ToListAsync();

    public async Task<IEnumerable<TelemetryReading>> GetByDateRangeAsync(DateTime from, DateTime to) =>
        await _context.TelemetryReadings
            .Where(r => r.Timestamp >= from && r.Timestamp <= to)
            .OrderBy(r => r.Timestamp)
            .ToListAsync();

    public async Task<IEnumerable<TelemetryReading>> GetAnomaliesAsync() =>
        await _context.TelemetryReadings
            .Where(r =>
                r.Temperature < -40.0 || r.Temperature > 60.0 ||
                r.HeartRate < 40 || r.HeartRate > 180 ||
                r.OxygenLevel < 90.0)
            .OrderByDescending(r => r.Timestamp)
            .ToListAsync();

    public async Task<TelemetryReading> AddAsync(TelemetryReading reading)
    {
        reading.Timestamp = DateTime.UtcNow;
        _context.TelemetryReadings.Add(reading);
        await _context.SaveChangesAsync();
        return reading;
    }

    public async Task UpdateAsync(TelemetryReading reading)
    {
        _context.TelemetryReadings.Update(reading);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var reading = await _context.TelemetryReadings.FindAsync(id);
        if (reading != null)
        {
            _context.TelemetryReadings.Remove(reading);
            await _context.SaveChangesAsync();
        }
    }
}
