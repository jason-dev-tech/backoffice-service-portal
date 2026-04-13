using BackofficeServicePortal.Api.Models;
using BackofficeServicePortal.Api.Services;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Testcontainers.MongoDb;
using Xunit;

namespace BackofficeServicePortal.Api.Tests;

public sealed class ServiceRequestAuditLogServiceTests : IClassFixture<MongoDbFixture>
{
    private readonly MongoDbFixture _fixture;

    public ServiceRequestAuditLogServiceTests(MongoDbFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task LogAsync_PersistsAuditLogDocument()
    {
        await ResetCollectionAsync();
        var service = CreateService();
        var expectedTimestamp = new DateTime(2026, 4, 10, 0, 0, 0, DateTimeKind.Utc);

        await service.LogAsync(new ServiceRequestAuditLog
        {
            ServiceRequestId = 101,
            Action = "Created",
            TimestampUtc = expectedTimestamp,
            Details = "Service request 'Sales report discrepancy' was created."
        });

        var persistedLogs = await GetCollection()
            .Find(FilterDefinition<ServiceRequestAuditLog>.Empty)
            .ToListAsync();

        var persistedLog = Assert.Single(persistedLogs);
        Assert.False(string.IsNullOrWhiteSpace(persistedLog.Id));
        Assert.Equal(101, persistedLog.ServiceRequestId);
        Assert.Equal("Created", persistedLog.Action);
        Assert.Equal(expectedTimestamp, persistedLog.TimestampUtc);
        Assert.Equal("Service request 'Sales report discrepancy' was created.", persistedLog.Details);
    }

    [Fact]
    public async Task LogAsync_PersistsMultipleAuditLogDocuments()
    {
        await ResetCollectionAsync();
        var service = CreateService();

        await service.LogAsync(new ServiceRequestAuditLog
        {
            ServiceRequestId = 201,
            Action = "Created",
            TimestampUtc = new DateTime(2026, 4, 10, 0, 0, 0, DateTimeKind.Utc),
            Details = "Service request 'Invoice processing delay' was created."
        });

        await service.LogAsync(new ServiceRequestAuditLog
        {
            ServiceRequestId = 201,
            Action = "Updated",
            TimestampUtc = new DateTime(2026, 4, 11, 0, 0, 0, DateTimeKind.Utc),
            Details = "Service request 'Invoice processing delay' was updated."
        });

        var persistedLogs = await GetCollection()
            .Find(FilterDefinition<ServiceRequestAuditLog>.Empty)
            .SortBy(log => log.TimestampUtc)
            .ToListAsync();

        Assert.Equal(2, persistedLogs.Count);

        Assert.Equal(201, persistedLogs[0].ServiceRequestId);
        Assert.Equal("Created", persistedLogs[0].Action);
        Assert.Equal(new DateTime(2026, 4, 10, 0, 0, 0, DateTimeKind.Utc), persistedLogs[0].TimestampUtc);
        Assert.Equal("Service request 'Invoice processing delay' was created.", persistedLogs[0].Details);

        Assert.Equal(201, persistedLogs[1].ServiceRequestId);
        Assert.Equal("Updated", persistedLogs[1].Action);
        Assert.Equal(new DateTime(2026, 4, 11, 0, 0, 0, DateTimeKind.Utc), persistedLogs[1].TimestampUtc);
        Assert.Equal("Service request 'Invoice processing delay' was updated.", persistedLogs[1].Details);
    }

    private ServiceRequestAuditLogService CreateService()
    {
        return new ServiceRequestAuditLogService(
            Options.Create(new MongoDbSettings
            {
                ConnectionString = _fixture.ConnectionString,
                DatabaseName = MongoDbFixture.DatabaseName,
                AuditLogsCollectionName = MongoDbFixture.CollectionName
            }));
    }

    private async Task ResetCollectionAsync()
    {
        await GetCollection().DeleteManyAsync(FilterDefinition<ServiceRequestAuditLog>.Empty);
    }

    private IMongoCollection<ServiceRequestAuditLog> GetCollection()
    {
        var client = new MongoClient(_fixture.ConnectionString);
        var database = client.GetDatabase(MongoDbFixture.DatabaseName);
        return database.GetCollection<ServiceRequestAuditLog>(MongoDbFixture.CollectionName);
    }
}

public sealed class MongoDbFixture : IAsyncLifetime
{
    public const string DatabaseName = "backoffice_service_portal_audit_tests";
    public const string CollectionName = "service_request_audit_logs";

    private readonly MongoDbContainer _container = new MongoDbBuilder()
        .WithImage("mongo:7.0")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}
