using Data.DbContext;
using Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ControlController : ControllerBase
{
    private readonly GreenhouseDbContext _dbContext;
    private readonly ILogger<ControlController> _logger;

    public ControlController(GreenhouseDbContext dbContext, ILogger<ControlController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    [HttpGet("state")]
    public async Task<ActionResult<ControlState>> GetCurrentState()
    {
        var latest = await _dbContext.ControlStates
            .OrderByDescending(r => r.Timestamp)
            .FirstOrDefaultAsync();

        if (latest == null)
            return NotFound();

        return latest;
    }

    [HttpGet("history")]
    public async Task<ActionResult<IEnumerable<ControlState>>> GetControlHistory([FromQuery] int limit = 20)
    {
        return await _dbContext.ControlStates
            .OrderByDescending(r => r.Timestamp)
            .Take(limit)
            .ToListAsync();
    }
}