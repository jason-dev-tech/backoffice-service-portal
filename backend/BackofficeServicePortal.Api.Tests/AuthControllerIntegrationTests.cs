using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BackofficeServicePortal.Api.Data;
using BackofficeServicePortal.Api.DTOs.Auth;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace BackofficeServicePortal.Api.Tests;

public sealed class AuthControllerIntegrationTests : IClassFixture<AuthControllerIntegrationTestsFixture>
{
    private readonly AuthControllerIntegrationTestsFixture _fixture;

    public AuthControllerIntegrationTests(AuthControllerIntegrationTestsFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Login_WithValidBootstrapAdminCredentials_ReturnsOkAndAccessToken()
    {
        using var client = _fixture.CreateClient();

        var response = await client.PostAsJsonAsync("/api/Auth/login", new LoginRequestDto
        {
            Username = _fixture.Factory.BootstrapAdminUsername,
            Password = _fixture.Factory.BootstrapAdminPassword
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<LoginResponseDto>();

        Assert.NotNull(payload);
        Assert.False(string.IsNullOrWhiteSpace(payload.AccessToken));
        Assert.True(payload.ExpiresAtUtc > DateTime.UtcNow);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        using var client = _fixture.CreateClient();

        var response = await client.PostAsJsonAsync("/api/Auth/login", new LoginRequestDto
        {
            Username = _fixture.Factory.BootstrapAdminUsername,
            Password = "InvalidPassword123!"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetServiceRequests_WithoutAuthorizationHeader_ReturnsUnauthorized()
    {
        using var client = _fixture.CreateClient();

        var response = await client.GetAsync("/api/ServiceRequests");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetServiceRequests_WithValidJwtFromLogin_ReturnsOk()
    {
        using var client = _fixture.CreateClient();
        var accessToken = await LoginAndGetAccessTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await client.GetAsync("/api/ServiceRequests");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private async Task<string> LoginAndGetAccessTokenAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/Auth/login", new LoginRequestDto
        {
            Username = _fixture.Factory.BootstrapAdminUsername,
            Password = _fixture.Factory.BootstrapAdminPassword
        });

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<LoginResponseDto>();

        Assert.NotNull(payload);
        Assert.False(string.IsNullOrWhiteSpace(payload.AccessToken));

        return payload.AccessToken;
    }
}

public sealed class AuthControllerIntegrationTestsFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("backoffice_service_portal_api_tests")
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

    public HttpClient CreateClient()
    {
        return Factory.CreateClient(new()
        {
            BaseAddress = new Uri("https://localhost")
        });
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
        SetEnvironmentVariable("MongoDbSettings__ConnectionString", "mongodb://127.0.0.1:1");
        SetEnvironmentVariable("MongoDbSettings__DatabaseName", "backoffice_service_portal_api_tests");
        SetEnvironmentVariable("MongoDbSettings__AuditLogsCollectionName", "service_request_audit_logs");
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
