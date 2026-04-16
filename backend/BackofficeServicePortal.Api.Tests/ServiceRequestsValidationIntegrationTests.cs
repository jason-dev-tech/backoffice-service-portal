using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using BackofficeServicePortal.Api.Data;
using BackofficeServicePortal.Api.DTOs.Auth;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace BackofficeServicePortal.Api.Tests;

public sealed class ServiceRequestsValidationIntegrationTests : IClassFixture<ServiceRequestsValidationIntegrationTestsFixture>
{
    private readonly ServiceRequestsValidationIntegrationTestsFixture _fixture;

    public ServiceRequestsValidationIntegrationTests(ServiceRequestsValidationIntegrationTestsFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateServiceRequest_WithInvalidPayload_ReturnsBadRequestWithValidationShape()
    {
        using var client = await _fixture.CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/ServiceRequests", new
        {
            title = "",
            description = "",
            requesterName = ""
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        AssertValidationResponseShape(document.RootElement, "Title", "Description", "RequesterName");
    }

    [Fact]
    public async Task UpdateServiceRequest_WithInvalidPayload_ReturnsBadRequestWithValidationShape()
    {
        using var client = await _fixture.CreateAuthenticatedClientAsync();

        var response = await client.PutAsJsonAsync("/api/ServiceRequests/999", new
        {
            title = "",
            description = "",
            requesterName = "",
            status = ""
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        AssertValidationResponseShape(document.RootElement, "Title", "Description", "RequesterName", "Status");
    }

    private static void AssertValidationResponseShape(JsonElement root, params string[] expectedFields)
    {
        Assert.Equal("Validation failed", root.GetProperty("message").GetString());

        var errors = root.GetProperty("errors");
        Assert.Equal(JsonValueKind.Array, errors.ValueKind);

        var fields = errors.EnumerateArray()
            .Select(error => new
            {
                Field = error.GetProperty("field").GetString(),
                Errors = error.GetProperty("errors")
            })
            .ToList();

        foreach (var expectedField in expectedFields)
        {
            var fieldEntry = Assert.Single(fields.Where(item => item.Field == expectedField));
            Assert.Equal(JsonValueKind.Array, fieldEntry.Errors.ValueKind);
            Assert.NotEmpty(fieldEntry.Errors.EnumerateArray());
        }
    }
}

public sealed class ServiceRequestsValidationIntegrationTestsFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("backoffice_service_portal_validation_api_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();
    private readonly Dictionary<string, string?> _originalEnvironmentValues = new();

    public ApiWebApplicationFactory Factory { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        await InitializeDatabaseAsync();

        Factory = new ApiWebApplicationFactory(_container.GetConnectionString());
        ApplyTestEnvironmentOverrides();
    }

    public async Task DisposeAsync()
    {
        Factory.Dispose();
        RestoreEnvironmentOverrides();
        await _container.DisposeAsync();
    }

    public async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = Factory.CreateClient(new()
        {
            BaseAddress = new Uri("https://localhost")
        });

        var response = await client.PostAsJsonAsync("/api/Auth/login", new LoginRequestDto
        {
            Username = Factory.BootstrapAdminUsername,
            Password = Factory.BootstrapAdminPassword
        });

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<LoginResponseDto>();

        Assert.NotNull(payload);
        Assert.False(string.IsNullOrWhiteSpace(payload.AccessToken));

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", payload.AccessToken);
        return client;
    }

    private async Task InitializeDatabaseAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .Options;

        await using var dbContext = new AppDbContext(options);
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.MigrateAsync();
    }

    private void ApplyTestEnvironmentOverrides()
    {
        SetEnvironmentVariable("ConnectionStrings__DefaultConnection", _container.GetConnectionString());
        SetEnvironmentVariable("Jwt__Key", Factory.JwtKey);
        SetEnvironmentVariable("Jwt__Issuer", Factory.JwtIssuer);
        SetEnvironmentVariable("Jwt__Audience", Factory.JwtAudience);
        SetEnvironmentVariable("BootstrapAdmin__Username", Factory.BootstrapAdminUsername);
        SetEnvironmentVariable("BootstrapAdmin__Password", Factory.BootstrapAdminPassword);
        SetEnvironmentVariable("BootstrapAdmin__Email", Factory.BootstrapAdminEmail);
        SetEnvironmentVariable("BootstrapAdmin__FullName", Factory.BootstrapAdminFullName);
    }

    private void RestoreEnvironmentOverrides()
    {
        foreach (var pair in _originalEnvironmentValues)
        {
            Environment.SetEnvironmentVariable(pair.Key, pair.Value);
        }
    }

    private void SetEnvironmentVariable(string key, string value)
    {
        _originalEnvironmentValues.TryAdd(key, Environment.GetEnvironmentVariable(key));
        Environment.SetEnvironmentVariable(key, value);
    }
}
