using BackofficeServicePortal.Api.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace BackofficeServicePortal.Api.Services;

/// <summary>
/// Service responsible for writing audit logs to MongoDB.
/// </summary>
public class ServiceRequestAuditLogService
{
    private readonly IMongoCollection<ServiceRequestAuditLog> _collection;

    public ServiceRequestAuditLogService(IOptions<MongoDbSettings> settings)
    {
        var mongoSettings = settings.Value;

        var client = new MongoClient(mongoSettings.ConnectionString);
        var database = client.GetDatabase(mongoSettings.DatabaseName);

        _collection = database.GetCollection<ServiceRequestAuditLog>(
            mongoSettings.AuditLogsCollectionName);
    }

    /// <summary>
    /// Inserts a new audit log entry into MongoDB.
    /// </summary>
    public async Task LogAsync(ServiceRequestAuditLog log)
    {
        try
        {
            await _collection.InsertOneAsync(log);
        }
        catch (Exception ex)
        {
            // Log error but do not break the main flow
            Console.WriteLine($"[MongoDB] Failed to write audit log: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets audit log entries for a specific service request.
    /// </summary>
    public async Task<List<ServiceRequestAuditLog>> GetLogsByServiceRequestIdAsync(int serviceRequestId)
    {
        return await _collection
            .Find(log => log.ServiceRequestId == serviceRequestId)
            .SortBy(log => log.TimestampUtc)
            .ToListAsync();
    }
}