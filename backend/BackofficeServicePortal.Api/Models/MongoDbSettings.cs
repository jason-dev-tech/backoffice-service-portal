namespace BackofficeServicePortal.Api.Models;

/// <summary>
/// Configuration settings for MongoDB connection and collections.
/// </summary>
public class MongoDbSettings
{
    /// <summary>
    /// MongoDB connection string.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Name of the MongoDB database.
    /// </summary>
    public string DatabaseName { get; set; } = string.Empty;

    /// <summary>
    /// Name of the collection used for storing service request audit logs.
    /// </summary>
    public string AuditLogsCollectionName { get; set; } = string.Empty;
}