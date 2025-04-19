using Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Data.DbContext;

/// <summary>
/// Database context for greenhouse monitoring and control system
/// </summary>
public class GreenhouseDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    /// <summary>
    /// Sensor readings from the greenhouse
    /// </summary>
    public DbSet<SensorReading> SensorReadings { get; set; }

    /// <summary>
    /// Control states of greenhouse systems
    /// </summary>
    public DbSet<ControlState> ControlStates { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public GreenhouseDbContext()
    {
    }

    /// <summary>
    /// Constructor with options
    /// </summary>
    public GreenhouseDbContext(DbContextOptions<GreenhouseDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Configure the database connection
    /// </summary>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // // For local development
            // string connectionString = "Server=localhost,1433;Database=GreenhouseDb;User Id=sa;Password=!2StrongLocalPassword$@!;TrustServerCertificate=True;";
            //
            // // When running in Docker containers, use the container name
            // if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
            // {
            // }
            var connectionString = "Server=mssql,1433;Database=GreenhouseDb;User Id=sa;Password=!2StrongLocalPassword$@!;TrustServerCertificate=True;";
            
            optionsBuilder.UseSqlServer(connectionString);
        }
    }
}