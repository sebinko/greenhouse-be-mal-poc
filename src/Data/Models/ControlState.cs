using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Models;

/// <summary>
/// Represents the control states for greenhouse systems
/// </summary>
public class ControlState
{
    /// <summary>
    /// Unique identifier for the control state
    /// </summary>
    [Key]
    public Guid Id { get; set; }
    
    /// <summary>
    /// State of the watering system (true = ON, false = OFF)
    /// </summary>
    [Required]
    public bool Watering { get; set; }

    /// <summary>
    /// State of the lighting system (true = ON, false = OFF)
    /// </summary>
    [Required]
    public bool Light { get; set; }

    /// <summary>
    /// State of the fertilizer system (true = ON, false = OFF)
    /// </summary>
    [Required]
    public bool Fertilizer { get; set; }

    /// <summary>
    /// When the control state was recorded
    /// </summary>
    [Required]
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public ControlState()
    {
        Id = Guid.NewGuid();
        Timestamp = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a new control state with specified values
    /// </summary>
    public ControlState(bool watering, bool light, bool fertilizer)
    {
        Id = Guid.NewGuid();
        Watering = watering;
        Light = light;
        Fertilizer = fertilizer;
        Timestamp = DateTime.UtcNow;
    }
}