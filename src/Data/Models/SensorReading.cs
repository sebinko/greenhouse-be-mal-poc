using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Models;

/// <summary>
/// Represents a collection of greenhouse sensor readings
/// </summary>
public class SensorReading
{
    /// <summary>
    /// Unique identifier for the sensor reading
    /// </summary>
    [Key]
    public Guid Id { get; set; }
    
    /// <summary>
    /// Air temperature in degrees Celsius
    /// </summary>
    [Required]
    public double AirTemperature { get; set; }
    
    /// <summary>
    /// Air humidity percentage
    /// </summary>
    [Required]
    public double AirHumidity { get; set; }
    
    /// <summary>
    /// Soil humidity percentage
    /// </summary>
    [Required]
    public double SoilHumidity { get; set; }
    
    /// <summary>
    /// Light level in lux
    /// </summary>
    [Required]
    public int LightLevel { get; set; }
    
    /// <summary>
    /// When the reading was taken
    /// </summary>
    [Required]
    public DateTime Timestamp { get; set; }
    
    /// <summary>
    /// Default constructor
    /// </summary>
    public SensorReading()
    {
        Id = Guid.NewGuid();
        Timestamp = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Creates a new sensor reading with specified values
    /// </summary>
    public SensorReading(double airTemperature, double airHumidity, double soilHumidity, int lightLevel)
    {
        Id = Guid.NewGuid();
        AirTemperature = airTemperature;
        AirHumidity = airHumidity;
        SoilHumidity = soilHumidity;
        LightLevel = lightLevel;
        Timestamp = DateTime.UtcNow;
    }
}