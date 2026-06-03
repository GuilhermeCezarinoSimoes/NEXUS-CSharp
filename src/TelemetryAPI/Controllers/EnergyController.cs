using Microsoft.AspNetCore.Mvc;
using TelemetryAPI.Models;
using TelemetryAPI.Services;

namespace TelemetryAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class EnergyController : ControllerBase
{
    private readonly TelemetryService _service;
    private readonly ILogger<EnergyController> _logger;

    public EnergyController(TelemetryService service, ILogger<EnergyController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>Registra nova leitura dos sensores de energia. Publica alerta se nível crítico.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(EnergyReading), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PostEnergyReading([FromBody] EnergyReading reading)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var saved = await _service.ProcessEnergyReadingAsync(reading);
            return CreatedAtAction(nameof(PostEnergyReading), new { id = saved.Id }, saved);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar leitura de energia");
            return StatusCode(500, new { error = "Erro interno." });
        }
    }

    /// <summary>Simula emergência de queda de energia total na base.</summary>
    [HttpPost("simulate-emergency")]
    [ProducesResponseType(typeof(EnergyReading), StatusCodes.Status200OK)]
    public async Task<IActionResult> SimulateEmergency(
        [FromQuery] string sensorId = "ENERGY-SIM-01",
        [FromQuery] string location = "Base-Antarctica-Alpha")
    {
        var emergency = EnergyReading.CreateEmergencyReading(sensorId, location);
        var result = await _service.ProcessEnergyReadingAsync(emergency);
        return Ok(result);
    }
}
