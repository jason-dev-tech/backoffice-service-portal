using System.Text.Json;
using BackofficeServicePortal.Api.Data;
using BackofficeServicePortal.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BackofficeServicePortal.Api.Services;

/// <summary>
/// Service responsible for writing audit logs to PostgreSQL.
/// </summary>
public class ServiceRequestAuditLogService
{
    private readonly AppDbContext _dbContext;

    public ServiceRequestAuditLogService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Inserts a new audit log entry into PostgreSQL.
    /// </summary>
    public async Task LogAsync(ServiceRequestAuditLog log)
    {
        try
        {
            _dbContext.ServiceRequestAuditLogEntries.Add(new ServiceRequestAuditLogEntry
            {
                ServiceRequestId = log.ServiceRequestId,
                Action = log.Action,
                TimestampUtc = log.TimestampUtc,
                Details = JsonSerializer.Serialize(log.Details)
            });

            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Log error but do not break the main flow
            Console.WriteLine($"[PostgreSQL] Failed to write audit log: {ex.Message}");
        }
    }

    /// <summary>
     /// Gets audit log entries for a specific service request.
    /// </summary>
    public async Task<List<ServiceRequestAuditLog>> GetLogsByServiceRequestIdAsync(int serviceRequestId)
    {
        var entries = await _dbContext.ServiceRequestAuditLogEntries
            .AsNoTracking()
            .Where(log => log.ServiceRequestId == serviceRequestId)
            .OrderBy(log => log.TimestampUtc)
            .ToListAsync();

        return entries
            .Select(log => new ServiceRequestAuditLog
            {
                Id = log.Id.ToString(),
                ServiceRequestId = log.ServiceRequestId,
                Action = log.Action,
                TimestampUtc = log.TimestampUtc,
                Details = JsonSerializer.Deserialize<string>(log.Details) ?? string.Empty
            })
            .ToList();
    }
}
