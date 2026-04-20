using BackofficeServicePortal.Api.Data;
using BackofficeServicePortal.Api.DTOs.ServiceRequests;
using BackofficeServicePortal.Api.Models;
using BackofficeServicePortal.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
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
            CreateRequest(1, "Sales report discrepancy", "Review a mismatch in the weekly sales report totals.", "Bianca", "Open", new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc)),
            CreateRequest(2, "Invoice processing delay", "Investigate delayed posting for supplier invoices.", "Daniel", "open", new DateTime(2026, 4, 2, 0, 0, 0, DateTimeKind.Utc)),
            CreateRequest(3, "POS settlement correction", "Correct the end-of-day POS settlement record.", "Farah", "Closed", new DateTime(2026, 4, 3, 0, 0, 0, DateTimeKind.Utc)));

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
            CreateRequest(1, "Sales report discrepancy", "Review a mismatch in the weekly sales report totals.", "Bianca", "Open", new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc)),
            CreateRequest(2, "Invoice processing delay", "Investigate delayed posting for supplier invoices.", "Daniel", "Open", new DateTime(2026, 4, 2, 0, 0, 0, DateTimeKind.Utc)),
            CreateRequest(3, "Customer record correction", "Update an incorrect billing contact on a customer account.", "Farah", "Closed", new DateTime(2026, 4, 3, 0, 0, 0, DateTimeKind.Utc)));

        var service = CreateService();

        var titleMatch = await service.GetAllAsync(new ServiceRequestQueryParams { Search = "sales" });
        var descriptionMatch = await service.GetAllAsync(new ServiceRequestQueryParams { Search = "supplier" });
        var requesterMatch = await service.GetAllAsync(new ServiceRequestQueryParams { Search = "farah" });

        Assert.Single(titleMatch.Items);
        Assert.Equal("Sales report discrepancy", titleMatch.Items[0].Title);

        Assert.Single(descriptionMatch.Items);
        Assert.Equal("Invoice processing delay", descriptionMatch.Items[0].Title);

        Assert.Single(requesterMatch.Items);
        Assert.Equal("Customer record correction", requesterMatch.Items[0].Title);
    }

    [Fact]
    public async Task GetAllAsync_AppliesRequestedSorting()
    {
        await ResetDatabaseAsync();
        await SeedServiceRequestsAsync(
            CreateRequest(1, "Month-end close review", "Validate the finance close checklist for the branch.", "Bianca", "Open", new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc)),
            CreateRequest(2, "Accounts payable exception", "Review a blocked supplier payment batch.", "Daniel", "Open", new DateTime(2026, 4, 3, 0, 0, 0, DateTimeKind.Utc)),
            CreateRequest(3, "Sales settlement variance", "Investigate a mismatch in POS settlement totals.", "Farah", "Closed", new DateTime(2026, 4, 2, 0, 0, 0, DateTimeKind.Utc)));

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
            CreateRequest(1, "Sales report discrepancy", "Review a mismatch in the weekly sales report totals.", "Bianca", "Open", new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc)),
            CreateRequest(2, "Invoice processing delay", "Investigate delayed posting for supplier invoices.", "Daniel", "Open", new DateTime(2026, 4, 2, 0, 0, 0, DateTimeKind.Utc)),
            CreateRequest(3, "Customer record correction", "Update an incorrect billing contact on a customer account.", "Farah", "Closed", new DateTime(2026, 4, 3, 0, 0, 0, DateTimeKind.Utc)));

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
            CreateRequest(1, "Sales report discrepancy", "Review a mismatch in the weekly sales report totals.", "Bianca", "Open", new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc)),
            CreateRequest(2, "Invoice processing delay", "Investigate delayed posting for supplier invoices.", "Daniel", "Open", new DateTime(2026, 4, 2, 0, 0, 0, DateTimeKind.Utc)),
            CreateRequest(3, "Customer record correction", "Update an incorrect billing contact on a customer account.", "Farah", "Closed", new DateTime(2026, 4, 3, 0, 0, 0, DateTimeKind.Utc)));

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

    [Fact]
    public async Task GetAllAsync_FallsBackToDefaultSortingWhenSortIsInvalid()
    {
        await ResetDatabaseAsync();
        await SeedServiceRequestsAsync(
            CreateRequest(1, "Sales report discrepancy", "Review a mismatch in the weekly sales report totals.", "Bianca", "Open", new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc)),
            CreateRequest(2, "Invoice processing delay", "Investigate delayed posting for supplier invoices.", "Daniel", "Open", new DateTime(2026, 4, 3, 0, 0, 0, DateTimeKind.Utc)),
            CreateRequest(3, "Customer record correction", "Update an incorrect billing contact on a customer account.", "Farah", "Closed", new DateTime(2026, 4, 2, 0, 0, 0, DateTimeKind.Utc)));

        var service = CreateService();

        var result = await service.GetAllAsync(new ServiceRequestQueryParams
        {
            Sort = "not_a_valid_sort"
        });

        Assert.Equal(new[] { 2, 3, 1 }, result.Items.Select(item => item.Id));
    }

    [Fact]
    public async Task GetAllAsync_NormalizesInvalidPageAndPageSizeInputs()
    {
        await ResetDatabaseAsync();
        await SeedServiceRequestsAsync(
            CreateRequest(1, "Sales report discrepancy", "Review a mismatch in the weekly sales report totals.", "Bianca", "Open", new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc)),
            CreateRequest(2, "Invoice processing delay", "Investigate delayed posting for supplier invoices.", "Daniel", "Open", new DateTime(2026, 4, 2, 0, 0, 0, DateTimeKind.Utc)),
            CreateRequest(3, "Customer record correction", "Update an incorrect billing contact on a customer account.", "Farah", "Closed", new DateTime(2026, 4, 3, 0, 0, 0, DateTimeKind.Utc)));

        var service = CreateService();

        var result = await service.GetAllAsync(new ServiceRequestQueryParams
        {
            Page = 0,
            PageSize = 0
        });

        Assert.Equal(1, result.Page);
        Assert.Equal(1, result.PageSize);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
        Assert.Single(result.Items);
        Assert.Equal(3, result.Items[0].Id);
    }

    [Fact]
    public async Task GetDashboardAsync_ReturnsSummaryCountsAndDates()
    {
        await ResetDatabaseAsync();
        await SeedServiceRequestsAsync(
            CreateRequest(1, "Sales report discrepancy", "Review a mismatch in the weekly sales report totals.", "Bianca", "Open", new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc)),
            CreateRequest(2, "Invoice exception review", "Review supplier invoices blocked by validation rules.", "Sofia", "In Progress", new DateTime(2026, 4, 3, 0, 0, 0, DateTimeKind.Utc)),
            CreateRequest(3, "Quarterly revenue adjustment", "Apply a correction to the quarter-end revenue summary.", "Elena", "Closed", new DateTime(2026, 4, 5, 0, 0, 0, DateTimeKind.Utc)),
            CreateRequest(4, "Invoice processing delay", "Investigate delayed posting for supplier invoices.", "Daniel", "Open", new DateTime(2026, 4, 2, 0, 0, 0, DateTimeKind.Utc)));

        var service = CreateService();

        var result = await service.GetDashboardAsync();

        Assert.Equal(4, result.TotalRequests);
        Assert.Equal(2, result.OpenRequests);
        Assert.Equal(1, result.InProgressRequests);
        Assert.Equal(1, result.ClosedRequests);
        Assert.Equal(new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc), result.OldestOpenRequestCreatedAt);
        Assert.Equal(new DateTime(2026, 4, 5, 0, 0, 0, DateTimeKind.Utc), result.MostRecentRequestCreatedAt);
        Assert.Equal(50.0, result.OpenSharePercentage);
        Assert.Equal(25.0, result.ClosedSharePercentage);
    }

    [Fact]
    public async Task GetDashboardAsync_NormalizesStatusCasingInCountsAndDistribution()
    {
        await ResetDatabaseAsync();
        await SeedServiceRequestsAsync(
            CreateRequest(1, "Sales report discrepancy", "Review a mismatch in the weekly sales report totals.", "Bianca", "Open", new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc)),
            CreateRequest(2, "Supplier payment variance", "Investigate an unexpected difference in supplier payment totals.", "Amina", "open", new DateTime(2026, 4, 2, 0, 0, 0, DateTimeKind.Utc)),
            CreateRequest(3, "Inventory update request", "Apply a product quantity adjustment before month-end reporting.", "Marcus", "OPEN", new DateTime(2026, 4, 3, 0, 0, 0, DateTimeKind.Utc)),
            CreateRequest(4, "Invoice exception review", "Review supplier invoices blocked by validation rules.", "Sofia", "In Progress", new DateTime(2026, 4, 4, 0, 0, 0, DateTimeKind.Utc)),
            CreateRequest(5, "Quarterly revenue adjustment", "Apply a correction to the quarter-end revenue summary.", "Elena", "closed", new DateTime(2026, 4, 5, 0, 0, 0, DateTimeKind.Utc)));

        var service = CreateService();

        var result = await service.GetDashboardAsync();

        Assert.Equal(5, result.TotalRequests);
        Assert.Equal(3, result.OpenRequests);
        Assert.Equal(1, result.InProgressRequests);
        Assert.Equal(1, result.ClosedRequests);

        var distribution = result.StatusDistribution.ToDictionary(item => item.Status);

        Assert.Equal(3, distribution["Open"].Count);
        Assert.Equal(60.0, distribution["Open"].Percentage);
        Assert.Equal(1, distribution["In Progress"].Count);
        Assert.Equal(20.0, distribution["In Progress"].Percentage);
        Assert.Equal(1, distribution["Closed"].Count);
        Assert.Equal(20.0, distribution["Closed"].Percentage);
    }

    [Fact]
    public async Task CreateAsync_ReturnsCreatedDtoAndPersistsRecord()
    {
        await ResetDatabaseAsync();
        var service = CreateService();

        var result = await service.CreateAsync(new CreateServiceRequestDto
        {
            Title = "Sales report discrepancy",
            Description = "Review a mismatch in the monthly sales report totals.",
            RequesterName = "Jordan",
            Status = "In Progress"
        });

        Assert.True(result.Id > 0);
        Assert.Equal("Sales report discrepancy", result.Title);
        Assert.Equal("Review a mismatch in the monthly sales report totals.", result.Description);
        Assert.Equal("Jordan", result.RequesterName);
        Assert.Equal("In Progress", result.Status);
        Assert.NotEqual(default, result.CreatedAt);
        Assert.Null(result.UpdatedAt);

        await using var dbContext = CreateDbContext();
        var persistedRequest = await dbContext.ServiceRequests.SingleAsync(sr => sr.Id == result.Id);

        Assert.Equal("Sales report discrepancy", persistedRequest.Title);
        Assert.Equal("Review a mismatch in the monthly sales report totals.", persistedRequest.Description);
        Assert.Equal("Jordan", persistedRequest.RequesterName);
        Assert.Equal("In Progress", persistedRequest.Status);
        Assert.InRange(
            (persistedRequest.CreatedAt - result.CreatedAt).Duration(),
            TimeSpan.Zero,
            TimeSpan.FromMilliseconds(1));
        Assert.Null(persistedRequest.UpdatedAt);
    }

    [Fact]
    public async Task CreateAsync_DefaultsBlankStatusToOpenAndLeavesUpdatedAtNull()
    {
        await ResetDatabaseAsync();
        var service = CreateService();
        var beforeCreate = DateTime.UtcNow;

        var result = await service.CreateAsync(new CreateServiceRequestDto
        {
            Title = "Invoice processing delay",
            Description = "Investigate delayed approval for a supplier invoice batch.",
            RequesterName = "Taylor",
            Status = "   "
        });

        var afterCreate = DateTime.UtcNow;

        Assert.Equal("Open", result.Status);
        Assert.Null(result.UpdatedAt);
        Assert.InRange(result.CreatedAt, beforeCreate, afterCreate);

        await using var dbContext = CreateDbContext();
        var persistedRequest = await dbContext.ServiceRequests.SingleAsync(sr => sr.Id == result.Id);

        Assert.Equal("Open", persistedRequest.Status);
        Assert.Null(persistedRequest.UpdatedAt);
        Assert.InRange(persistedRequest.CreatedAt, beforeCreate, afterCreate);
    }

    [Fact]
    public async Task CreateAsync_DefaultsNullStatusToOpen()
    {
        await ResetDatabaseAsync();
        var service = CreateService();

        var result = await service.CreateAsync(new CreateServiceRequestDto
        {
            Title = "Sales report discrepancy",
            Description = "Review a mismatch in the monthly sales report totals.",
            RequesterName = "Jordan",
            Status = null
        });

        Assert.Equal("Open", result.Status);
        Assert.Null(result.UpdatedAt);

        await using var dbContext = CreateDbContext();
        var persistedRequest = await dbContext.ServiceRequests.SingleAsync(sr => sr.Id == result.Id);

        Assert.Equal("Open", persistedRequest.Status);
        Assert.Null(persistedRequest.UpdatedAt);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesEditableFieldsSetsUpdatedAtAndPersistsChanges()
    {
        await ResetDatabaseAsync();
        await SeedServiceRequestsAsync(
            CreateRequest(1, "Sales report discrepancy", "Review a mismatch in the weekly sales report totals.", "Bianca", "Open", new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc)));

        var service = CreateService();
        var beforeUpdate = DateTime.UtcNow;

        var result = await service.UpdateAsync(1, new UpdateServiceRequestDto
        {
            Title = "Sales report discrepancy resolved",
            Description = "The reporting totals were reconciled and confirmed.",
            RequesterName = "Jordan",
            Status = "Closed"
        });

        var afterUpdate = DateTime.UtcNow;

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Sales report discrepancy resolved", result.Title);
        Assert.Equal("The reporting totals were reconciled and confirmed.", result.Description);
        Assert.Equal("Jordan", result.RequesterName);
        Assert.Equal("Closed", result.Status);
        Assert.Equal(new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc), result.CreatedAt);
        Assert.NotNull(result.UpdatedAt);
        Assert.InRange(result.UpdatedAt!.Value, beforeUpdate, afterUpdate);

        await using var dbContext = CreateDbContext();
        var persistedRequest = await dbContext.ServiceRequests.SingleAsync(sr => sr.Id == 1);

        Assert.Equal("Sales report discrepancy resolved", persistedRequest.Title);
        Assert.Equal("The reporting totals were reconciled and confirmed.", persistedRequest.Description);
        Assert.Equal("Jordan", persistedRequest.RequesterName);
        Assert.Equal("Closed", persistedRequest.Status);
        Assert.Equal(new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc), persistedRequest.CreatedAt);
        Assert.NotNull(persistedRequest.UpdatedAt);
        Assert.InRange(persistedRequest.UpdatedAt!.Value, beforeUpdate, afterUpdate);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNullWhenServiceRequestDoesNotExist()
    {
        await ResetDatabaseAsync();
        var service = CreateService();

        var result = await service.UpdateAsync(999, new UpdateServiceRequestDto
        {
            Title = "Revenue reconciliation update",
            Description = "Apply an updated note for the reconciliation review.",
            RequesterName = "Jordan",
            Status = "Closed"
        });

        Assert.Null(result);

        await using var dbContext = CreateDbContext();
        Assert.Empty(await dbContext.ServiceRequests.ToListAsync());
    }

    [Fact]
    public async Task UpdateAsync_PersistsWhitespaceStatusAsIsAndSetsUpdatedAt()
    {
        await ResetDatabaseAsync();
        await SeedServiceRequestsAsync(
            CreateRequest(1, "Sales report discrepancy", "Review a mismatch in the weekly sales report totals.", "Bianca", "Open", new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc)));

        var service = CreateService();
        var beforeUpdate = DateTime.UtcNow;

        var result = await service.UpdateAsync(1, new UpdateServiceRequestDto
        {
            Title = "Sales report discrepancy",
            Description = "Review a mismatch in the weekly sales report totals.",
            RequesterName = "Bianca",
            Status = "   "
        });

        var afterUpdate = DateTime.UtcNow;

        Assert.NotNull(result);
        Assert.Equal("   ", result.Status);
        Assert.NotNull(result.UpdatedAt);
        Assert.InRange(result.UpdatedAt!.Value, beforeUpdate, afterUpdate);

        await using var dbContext = CreateDbContext();
        var persistedRequest = await dbContext.ServiceRequests.SingleAsync(sr => sr.Id == 1);

        Assert.Equal("   ", persistedRequest.Status);
        Assert.NotNull(persistedRequest.UpdatedAt);
        Assert.InRange(persistedRequest.UpdatedAt!.Value, beforeUpdate, afterUpdate);
    }

    [Fact]
    public async Task DeleteAsync_RemovesRecordAndGetByIdAsyncReturnsNull()
    {
        await ResetDatabaseAsync();
        await SeedServiceRequestsAsync(
            CreateRequest(1, "Sales report discrepancy", "Review a mismatch in the weekly sales report totals.", "Bianca", "Open", new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc)));

        var service = CreateService();

        var deleted = await service.DeleteAsync(1);
        var deletedRequest = await service.GetByIdAsync(1);

        Assert.True(deleted);
        Assert.Null(deletedRequest);

        await using var dbContext = CreateDbContext();
        Assert.Empty(await dbContext.ServiceRequests.ToListAsync());
    }

    [Fact]
    public async Task DeleteAsync_RemovesRecordWithoutAuditLogDependency()
    {
        await ResetDatabaseAsync();
        await SeedServiceRequestsAsync(
            CreateRequest(1, "Sales report discrepancy", "Review a mismatch in the weekly sales report totals.", "Bianca", "Open", new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc)));

        var service = CreateService();

        var deleted = await service.DeleteAsync(1);

        Assert.True(deleted);

        await using var dbContext = CreateDbContext();
        Assert.Empty(await dbContext.ServiceRequests.ToListAsync());
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalseWhenServiceRequestDoesNotExist()
    {
        await ResetDatabaseAsync();
        var service = CreateService();

        var deleted = await service.DeleteAsync(999);

        Assert.False(deleted);

        await using var dbContext = CreateDbContext();
        Assert.Empty(await dbContext.ServiceRequests.ToListAsync());
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsDtoForExistingRecord()
    {
        await ResetDatabaseAsync();
        await SeedServiceRequestsAsync(
            CreateRequest(1, "Sales report discrepancy", "Review a mismatch in the weekly sales report totals.", "Bianca", "Open", new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc)));

        var service = CreateService();

        var result = await service.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Sales report discrepancy", result.Title);
        Assert.Equal("Review a mismatch in the weekly sales report totals.", result.Description);
        Assert.Equal("Bianca", result.RequesterName);
        Assert.Equal("Open", result.Status);
        Assert.Equal(new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc), result.CreatedAt);
        Assert.Null(result.UpdatedAt);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNullWhenRecordDoesNotExist()
    {
        await ResetDatabaseAsync();
        var service = CreateService();

        var result = await service.GetByIdAsync(999);

        Assert.Null(result);
    }

    private ServiceRequestService CreateService()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_fixture.ConnectionString)
            .Options;

        var dbContext = new AppDbContext(options);
        return new ServiceRequestService(dbContext);
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
