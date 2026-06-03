using AlertsAPI.Data;
using AlertsAPI.Interfaces;
using AlertsAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace AlertsAPI.Repositories;

public class AlertRepository : IAlertRepository
{
    private readonly AlertsDbContext _context;

    public AlertRepository(AlertsDbContext context) => _context = context;

    public async Task<Alert?> GetByIdAsync(int id) =>
        await _context.Alerts.FindAsync(id);

    public async Task<Alert?> GetByAlertIdAsync(string alertId) =>
        await _context.Alerts.FirstOrDefaultAsync(a => a.AlertId == alertId);

    public async Task<IEnumerable<Alert>> GetAllAsync() =>
        await _context.Alerts.OrderByDescending(a => a.ReceivedAt).ToListAsync();

    public async Task<IEnumerable<Alert>> GetActiveAlertsAsync() =>
        await _context.Alerts
            .Where(a => a.Status == EmergencyStatus.Ativo || a.Status == EmergencyStatus.EmAtendimento)
            .OrderByDescending(a => a.ReceivedAt)
            .ToListAsync();

    public async Task<IEnumerable<Alert>> GetBySeverityAsync(string severity) =>
        await _context.Alerts
            .Where(a => a.Severity == severity)
            .OrderByDescending(a => a.ReceivedAt)
            .ToListAsync();

    public async Task<IEnumerable<Alert>> GetByLocationAsync(string location) =>
        await _context.Alerts
            .Where(a => a.BaseLocation == location)
            .OrderByDescending(a => a.ReceivedAt)
            .ToListAsync();

    public async Task<IEnumerable<Alert>> GetByDateRangeAsync(DateTime from, DateTime to) =>
        await _context.Alerts
            .Where(a => a.ReceivedAt >= from && a.ReceivedAt <= to)
            .OrderBy(a => a.ReceivedAt)
            .ToListAsync();

    public async Task<Alert> AddAsync(Alert alert)
    {
        _context.Alerts.Add(alert);
        await _context.SaveChangesAsync();
        return alert;
    }

    public async Task UpdateAsync(Alert alert)
    {
        _context.Alerts.Update(alert);
        await _context.SaveChangesAsync();
    }

    public async Task<int> CountActiveAsync() =>
        await _context.Alerts.CountAsync(a =>
            a.Status == EmergencyStatus.Ativo || a.Status == EmergencyStatus.EmAtendimento);
}
