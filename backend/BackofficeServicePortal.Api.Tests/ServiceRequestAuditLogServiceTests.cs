using BackofficeServicePortal.Api.Data;
using BackofficeServicePortal.Api.Models;
using BackofficeServicePortal.Api.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BackofficeServicePortal.Api.Tests;

public sealed class ServiceRequestAuditLogServiceTests : IClassFixture<PostgreSqlFixture>
{
    private readonly PostgreSqlFixture _fixture;

    public ServiceRequestAuditLogServiceTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task LogAsync_PersistsAuditLogEntry()
    {
        await ResetDatabaseAsync();
        await SeedServiceRequestAsync(101);
        var service = CreateService();
        var expectedTimestamp = new DateTime(2026, 4, 10, 0, 0, 0, DateTimeKind.Utc);

        await service.LogAsync(new ServiceRequestAuditLog
        {
            ServiceRequestId = 101,
            Action = "Created",
            TimestampUtc = expectedTimestamp,
            Details = "Service request 'Sales report discrepancy' was created."
        });

        await using var dbContext = CreateDbContext();
        var persistedLog = await dbContext.ServiceRequestAuditLogEntries.SingleAsync();

        Assert.True(persistedLog.Id > 0);
        Assert.Equal(101, persistedLog.ServiceRequestId);
        Assert.Equal("Created", persistedLog.Action);
        Assert.Equal(expectedTimestamp, persistedLog.TimestampUtc);
        Assert.Equal("Service request 'Sales report discrepancy' was created.", persistedLog.Details);
    }

    [Fact]
    public async Task LogAsync_PersistsMultipleAuditLogEntries()
    {
        await ResetDatabaseAsync();
        await SeedServiceRequestAsync(201);
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

        await using var dbContext = CreateDbContext();
        var persistedLogs = await dbContext.ServiceRequestAuditLogEntries
            .OrderBy(log => log.TimestampUtc)
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

    [Fact]
    public async Task GetLogsByServiceRequestIdAsync_ReturnsMappedAuditLogs()
    {
        await ResetDatabaseAsync();
        await SeedServiceRequestAsync(301);
        var service = CreateService();

        await service.LogAsync(new ServiceRequestAuditLog
        {
            ServiceRequestId = 301,
            Action = "Created",
            TimestampUtc = new DateTime(2026, 4, 10, 0, 0, 0, DateTimeKind.Utc),
            Details = "Created details"
        });

        var logs = await service.GetLogsByServiceRequestIdAsync(301);

        var persistedLog = Assert.Single(logs);
        Assert.False(string.IsNullOrWhiteSpace(persistedLog.Id));
        Assert.Equal(301, persistedLog.ServiceRequestId);
        Assert.Equal("Created", persistedLog.Action);
        Assert.Equal(new DateTime(2026, 4, 10, 0, 0, 0, DateTimeKind.Utc), persistedLog.TimestampUtc);
        Assert.Equal("Created details", persistedLog.Details);
    }

    private ServiceRequestAuditLogService CreateService()
    {
        return new ServiceRequestAuditLogService(CreateDbContext());
    }

    private async Task ResetDatabaseAsync()
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
    }

    private async Task SeedServiceRequestAsync(int id)
    {
        await using var dbContext = CreateDbContext();
        dbContext.ServiceRequests.Add(new ServiceRequest
        {
            Id = id,
            Title = $"Title {id}",
            Description = $"Description {id}",
            RequesterName = "Audit Test",
            Status = "Open",
            CreatedAt = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc)
        });
        await dbContext.SaveChangesAsync();
    }

    private AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_fixture.ConnectionString)
            .Options;

        return new AppDbContext(options);
    }
}
