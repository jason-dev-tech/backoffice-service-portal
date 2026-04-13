using BackofficeServicePortal.Api.Data;
using BackofficeServicePortal.Api.DTOs.ServiceRequests;
using BackofficeServicePortal.Api.Models;
using BackofficeServicePortal.Api.Services;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace BackofficeServicePortal.Api.Tests;

public sealed class ServiceRequestServiceTests : IClassFixture<PostgreSqlFixture>
{
    private readonly PostgreSqlFixture _fixture;

    public ServiceRequestServiceTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetAllAsync_FiltersByStatus_CaseInsensitively()
    {
        await ResetDatabaseAsync();
        await SeedServiceRequestsAsync(
            CreateRequest(1, "Printer issue", "Office printer stopped working.", "Jason", "Open", new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc)),
            CreateRequest(2, "VPN access", "Need VPN access for a contractor.", "Ava", "open", new DateTime(2026, 4, 2, 0, 0, 0, DateTimeKind.Utc)),
            CreateRequest(3, "Laptop replacement", "Replace device for new starter.", "Mia", "Closed", new DateTime(2026, 4, 3, 0, 0, 0, DateTimeKind.Utc)));

        var service = CreateService();

        var result = await service.GetAllAsync(new ServiceRequestQueryParams
        {
            Status = "OPEN"
        });

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
        Assert.All(result.Items, item => Assert.Equal("open", item.Status, ignoreCase: true));
    }

    [Fact]
    public async Task GetAllAsync_SearchesAcrossTitleDescriptionAndRequesterName()
    {
        await ResetDatabaseAsync();
        await SeedServiceRequestsAsync(
            CreateRequest(1, "Printer issue", "Office printer stopped working.", "Jason", "Open", new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc)),
            CreateRequest(2, "VPN access", "Need secure remote access.", "Ava", "Open", new DateTime(2026, 4, 2, 0, 0, 0, DateTimeKind.Utc)),
            CreateRequest(3, "New starter equipment", "Provision laptop and monitor.", "Mia", "Closed", new DateTime(2026, 4, 3, 0, 0, 0, DateTimeKind.Utc)));

        var service = CreateService();

        var titleMatch = await service.GetAllAsync(new ServiceRequestQueryParams { Search = "printer" });
        var descriptionMatch = await service.GetAllAsync(new ServiceRequestQueryParams { Search = "remote" });
        var requesterMatch = await service.GetAllAsync(new ServiceRequestQueryParams { Search = "mia" });

        Assert.Single(titleMatch.Items);
        Assert.Equal("Printer issue", titleMatch.Items[0].Title);

        Assert.Single(descriptionMatch.Items);
        Assert.Equal("VPN access", descriptionMatch.Items[0].Title);

        Assert.Single(requesterMatch.Items);
        Assert.Equal("New starter equipment", requesterMatch.Items[0].Title);
    }

    [Fact]
    public async Task GetAllAsync_AppliesRequestedSorting()
    {
        await ResetDatabaseAsync();
        await SeedServiceRequestsAsync(
            CreateRequest(1, "Bravo request", "Second item", "Jason", "Open", new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc)),
            CreateRequest(2, "Alpha request", "First by title", "Ava", "Open", new DateTime(2026, 4, 3, 0, 0, 0, DateTimeKind.Utc)),
            CreateRequest(3, "Charlie request", "Third item", "Mia", "Closed", new DateTime(2026, 4, 2, 0, 0, 0, DateTimeKind.Utc)));

        var service = CreateService();

        var oldestFirst = await service.GetAllAsync(new ServiceRequestQueryParams { Sort = "createdAt_asc" });
        var titleDescending = await service.GetAllAsync(new ServiceRequestQueryParams { Sort = "title_desc" });

        Assert.Equal(new[] { 1, 3, 2 }, oldestFirst.Items.Select(item => item.Id));
        Assert.Equal(new[] { 3, 1, 2 }, titleDescending.Items.Select(item => item.Id));
    }

    [Fact]
    public async Task GetAllAsync_ReturnsPaginationMetadata()
    {
        await ResetDatabaseAsync();
        await SeedServiceRequestsAsync(
            CreateRequest(1, "Request 1", "Description 1", "User 1", "Open", new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc)),
            CreateRequest(2, "Request 2", "Description 2", "User 2", "Open", new DateTime(2026, 4, 2, 0, 0, 0, DateTimeKind.Utc)),
            CreateRequest(3, "Request 3", "Description 3", "User 3", "Closed", new DateTime(2026, 4, 3, 0, 0, 0, DateTimeKind.Utc)));

        var service = CreateService();

        var result = await service.GetAllAsync(new ServiceRequestQueryParams
        {
            Page = 2,
            PageSize = 2,
            Sort = "createdAt_asc"
        });

        Assert.Equal(2, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
        Assert.Single(result.Items);
        Assert.Equal(3, result.Items[0].Id);
    }

    [Fact]
    public async Task GetAllAsync_ClampsPageOverflowToLastPage()
    {
        await ResetDatabaseAsync();
        await SeedServiceRequestsAsync(
            CreateRequest(1, "Request 1", "Description 1", "User 1", "Open", new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc)),
            CreateRequest(2, "Request 2", "Description 2", "User 2", "Open", new DateTime(2026, 4, 2, 0, 0, 0, DateTimeKind.Utc)),
            CreateRequest(3, "Request 3", "Description 3", "User 3", "Closed", new DateTime(2026, 4, 3, 0, 0, 0, DateTimeKind.Utc)));

        var service = CreateService();

        var result = await service.GetAllAsync(new ServiceRequestQueryParams
        {
            Page = 99,
            PageSize = 2
        });

        Assert.Equal(2, result.Page);
        Assert.Equal(2, result.TotalPages);
        Assert.Single(result.Items);
        Assert.Equal(1, result.Items[0].Id);
    }

    private ServiceRequestService CreateService()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_fixture.ConnectionString)
            .Options;

        var dbContext = new AppDbContext(options);

        return new ServiceRequestService(dbContext, null!);
    }

    private async Task ResetDatabaseAsync()
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
    }

    private async Task SeedServiceRequestsAsync(params ServiceRequest[] serviceRequests)
    {
        await using var dbContext = CreateDbContext();
        dbContext.ServiceRequests.AddRange(serviceRequests);
        await dbContext.SaveChangesAsync();
    }

    private AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_fixture.ConnectionString)
            .Options;

        return new AppDbContext(options);
    }

    private static ServiceRequest CreateRequest(
        int id,
        string title,
        string description,
        string requesterName,
        string status,
        DateTime createdAtUtc)
    {
        return new ServiceRequest
        {
            Id = id,
            Title = title,
            Description = description,
            RequesterName = requesterName,
            Status = status,
            CreatedAt = createdAtUtc
        };
    }
}

public sealed class PostgreSqlFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("backoffice_service_portal_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
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
