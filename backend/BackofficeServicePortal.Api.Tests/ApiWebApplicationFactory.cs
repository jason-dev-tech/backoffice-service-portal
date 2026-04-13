using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace BackofficeServicePortal.Api.Tests;

public sealed class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _postgresConnectionString;

    public ApiWebApplicationFactory(string postgresConnectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(postgresConnectionString);
        _postgresConnectionString = postgresConnectionString;
    }

    public string JwtKey => "integration-test-jwt-key-1234567890";

    public string JwtIssuer => "BackofficeServicePortal.IntegrationTests";

    public string JwtAudience => "BackofficeServicePortal.IntegrationTests.Client";

    public string BootstrapAdminUsername => "admin_test";

    public string BootstrapAdminPassword => "AdminTest123!";

    public string BootstrapAdminEmail => "admin_test@backoffice.local";

    public string BootstrapAdminFullName => "Admin Test";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            var overrides = new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _postgresConnectionString,
                ["Jwt:Key"] = JwtKey,
                ["Jwt:Issuer"] = JwtIssuer,
                ["Jwt:Audience"] = JwtAudience,
                ["BootstrapAdmin:Username"] = BootstrapAdminUsername,
                ["BootstrapAdmin:Password"] = BootstrapAdminPassword,
                ["BootstrapAdmin:Email"] = BootstrapAdminEmail,
                ["BootstrapAdmin:FullName"] = BootstrapAdminFullName
            };

            configBuilder.AddInMemoryCollection(overrides);
        });
    }
}
