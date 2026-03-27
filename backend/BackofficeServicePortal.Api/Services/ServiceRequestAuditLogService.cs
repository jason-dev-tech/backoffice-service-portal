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
        await _collection.InsertOneAsync(log);
    }
}