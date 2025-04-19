using Api.Controllers;
using Data.DbContext;
using Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SensorReadingsController : ControllerBase
{
    private readonly GreenhouseDbContext _dbContext;
    private readonly ILogger<SensorReadingsController> _logger;

    public SensorReadingsController(GreenhouseDbContext dbContext, ILogger<SensorReadingsController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SensorReading>>> GetSensorReadings([FromQuery] int limit = 20)
    {
        return await _dbContext.SensorReadings
            .OrderByDescending(r => r.Timestamp)
            .Take(limit)
            .ToListAsync();
    }

    [HttpGet("latest")]
    public async Task<ActionResult<SensorReading>> GetLatestSensorReading()
    {
        var latest = await _dbContext.SensorReadings
            .OrderByDescending(r => r.Timestamp)
            .FirstOrDefaultAsync();

        if (latest == null)
            return NotFound();

        return latest;
    }
}