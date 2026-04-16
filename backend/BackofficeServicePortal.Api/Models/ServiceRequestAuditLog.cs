namespace BackofficeServicePortal.Api.Models;

/// <summary>
/// Represents an audit log entry for service request operations.
/// </summary>
public class ServiceRequestAuditLog
{
    /// <summary>
    /// Unique identifier for the audit log entry.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Identifier of the related service request in PostgreSQL.
    /// </summary>
    public int ServiceRequestId { get; set; }

    /// <summary>
    /// Type of action performed on the service request.
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// UTC timestamp when the action occurred.
    /// </summary>
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Optional summary describing the operation.
    /// </summary>
    public string Details { get; set; } = string.Empty;
}
