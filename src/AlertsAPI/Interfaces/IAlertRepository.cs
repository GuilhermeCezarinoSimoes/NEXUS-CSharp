using AlertsAPI.Models;

namespace AlertsAPI.Interfaces;

public interface IAlertRepository
{
    Task<Alert?> GetByIdAsync(int id);
    Task<Alert?> GetByAlertIdAsync(string alertId);
    Task<IEnumerable<Alert>> GetAllAsync();
    Task<IEnumerable<Alert>> GetActiveAlertsAsync();
    Task<IEnumerable<Alert>> GetBySeverityAsync(string severity);
    Task<IEnumerable<Alert>> GetByLocationAsync(string location);
    Task<IEnumerable<Alert>> GetByDateRangeAsync(DateTime from, DateTime to);
    Task<Alert> AddAsync(Alert alert);
    Task UpdateAsync(Alert alert);
    Task<int> CountActiveAsync();
}
