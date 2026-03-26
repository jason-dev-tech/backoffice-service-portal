using System.ComponentModel.DataAnnotations;

namespace BackofficeServicePortal.Api.Models;

/// <summary>
/// Represents a service request submitted by a user.
/// </summary>
public class ServiceRequest
{
    /// <summary>
    /// Unique identifier for the service request.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Title of the service request.
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the issue or request.
    /// </summary>
    [Required]
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Name of the person who submitted the request.
    /// </summary>
    [Required]
    [StringLength(100)]
    public string RequesterName { get; set; } = string.Empty;

    /// <summary>
    /// Current status of the service request (e.g., Open, In Progress, Closed).
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "Open";

    /// <summary>
    /// Timestamp when the service request was created (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the service request was last updated (UTC).
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}