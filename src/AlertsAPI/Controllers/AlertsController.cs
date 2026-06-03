using AlertsAPI.Interfaces;
using AlertsAPI.Models;
using AlertsAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace AlertsAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class AlertsController : ControllerBase
{
    private readonly AlertService _service;
    private readonly IAlertRepository _repo;
    private readonly ILogger<AlertsController> _logger;

    public AlertsController(AlertService service, IAlertRepository repo, ILogger<AlertsController> logger)
    {
        _service = service;
        _repo = repo;
        _logger = logger;
    }

    /// <summary>Status geral do sistema de alertas da base NEXUS.</summary>
    [HttpGet("status")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatus()
    {
        var summary = await _service.GetStatusSummaryAsync();
        return Ok(summary);
    }

    /// <summary>Lista todos os alertas.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Alert>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var alerts = await _repo.GetAllAsync();
        return Ok(alerts);
    }

    /// <summary>Retorna um alerta pelo ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Alert), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var alert = await _repo.GetByIdAsync(id);
        return alert is null ? NotFound(new { error = $"Alerta {id} não encontrado." }) : Ok(alert);
    }

    /// <summary>Lista apenas alertas ativos (Ativo ou EmAtendimento).</summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(IEnumerable<Alert>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActive()
    {
        var active = await _repo.GetActiveAlertsAsync();
        return Ok(active);
    }

    /// <summary>Lista alertas críticos.</summary>
    [HttpGet("critical")]
    [ProducesResponseType(typeof(IEnumerable<Alert>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCritical()
    {
        var critical = await _repo.GetBySeverityAsync("CRITICO");
        return Ok(critical);
    }

    /// <summary>Filtra alertas por base/localização.</summary>
    [HttpGet("location/{location}")]
    [ProducesResponseType(typeof(IEnumerable<Alert>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByLocation(string location)
    {
        var alerts = await _service.GetByLocationAsync(location);
        return Ok(alerts);
    }

    /// <summary>Filtra alertas por período.</summary>
    [HttpGet("history")]
    [ProducesResponseType(typeof(IEnumerable<Alert>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetHistory([FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        try
        {
            var alerts = await _service.GetByDateRangeAsync(from, to);
            return Ok(alerts);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Operador confirma o alerta (acknowledges) e assume responsabilidade.</summary>
    [HttpPatch("{id:int}/acknowledge")]
    [ProducesResponseType(typeof(Alert), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Acknowledge(int id, [FromQuery] string operatorId)
    {
        if (string.IsNullOrWhiteSpace(operatorId))
            return BadRequest(new { error = "operatorId é obrigatório." });
        try
        {
            var alert = await _service.AcknowledgeAlertAsync(id, operatorId);
            return Ok(alert);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Marca alerta como resolvido.</summary>
    [HttpPatch("{id:int}/resolve")]
    [ProducesResponseType(typeof(Alert), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Resolve(int id)
    {
        try
        {
            var alert = await _service.ResolveAlertAsync(id);
            return Ok(alert);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}
