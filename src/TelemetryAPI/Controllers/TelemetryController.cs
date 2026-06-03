using Microsoft.AspNetCore.Mvc;
using TelemetryAPI.Interfaces;
using TelemetryAPI.Models;
using TelemetryAPI.Services;

namespace TelemetryAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class TelemetryController : ControllerBase
{
    private readonly TelemetryService _service;
    private readonly ITelemetryRepository _repo;
    private readonly ILogger<TelemetryController> _logger;

    public TelemetryController(
        TelemetryService service,
        ITelemetryRepository repo,
        ILogger<TelemetryController> logger)
    {
        _service = service;
        _repo = repo;
        _logger = logger;
    }

    /// <summary>Retorna resumo do dashboard com status geral da base.</summary>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboard()
    {
        try
        {
            var summary = await _service.GetDashboardSummaryAsync();
            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar dashboard");
            return StatusCode(500, new { error = "Erro interno ao gerar dashboard." });
        }
    }

    /// <summary>Lista todas as leituras de telemetria de saúde.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TelemetryReading>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var readings = await _repo.GetAllAsync();
        return Ok(readings);
    }

    /// <summary>Retorna uma leitura específica pelo ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TelemetryReading), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var reading = await _repo.GetByIdAsync(id);
        return reading is null ? NotFound(new { error = $"Leitura {id} não encontrada." }) : Ok(reading);
    }

    /// <summary>Registra nova leitura de saúde do operador. Publica alerta se anomalia for detectada.</summary>
    [HttpPost("health")]
    [ProducesResponseType(typeof(TelemetryReading), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PostHealthReading([FromBody] TelemetryReading reading)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var saved = await _service.ProcessHealthReadingAsync(reading);
            return CreatedAtAction(nameof(GetById), new { id = saved.Id }, saved);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar leitura de saúde");
            return StatusCode(500, new { error = "Erro interno ao processar leitura." });
        }
    }

    /// <summary>Retorna histórico de leituras de um operador, com filtro opcional de datas.</summary>
    [HttpGet("operator/{operatorId}")]
    [ProducesResponseType(typeof(IEnumerable<TelemetryReading>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByOperator(
        string operatorId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        try
        {
            var history = await _service.GetHistoryByOperatorAsync(operatorId, from, to);
            return Ok(history);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Lista todas as leituras com anomalia detectada.</summary>
    [HttpGet("anomalies")]
    [ProducesResponseType(typeof(IEnumerable<TelemetryReading>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAnomalies()
    {
        var anomalies = await _repo.GetAnomaliesAsync();
        return Ok(anomalies);
    }
}
