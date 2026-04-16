using System.ComponentModel.DataAnnotations;

namespace BackofficeServicePortal.Api.Models;

/// <summary>
/// Represents a PostgreSQL-backed audit log entry for service request operations.
/// </summary>
public class ServiceRequestAuditLogEntry
{
    /// <summary>
    /// Unique identifier for the audit log entry.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Identifier of the related service request in PostgreSQL.
    /// </summary>
    public int ServiceRequestId { get; set; }

    /// <summary>
    /// Type of action performed on the service request.
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// UTC timestamp when the action occurred.
    /// </summary>
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// JSON-backed detail payload for the audit entry.
    /// </summary>
    [Required]
    public string Details { get; set; } = string.Empty;
}
