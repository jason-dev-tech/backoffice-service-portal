using System.Reflection;
using System.Text;
using BackofficeServicePortal.Api.Configuration;
using BackofficeServicePortal.Api.Data;
using BackofficeServicePortal.Api.Models;
using BackofficeServicePortal.Api.Services;
using BackofficeServicePortal.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddControllers();
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("database", tags: ["ready"]);

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(x => x.Value?.Errors.Count > 0)
            .Select(x => new
            {
                Field = x.Key,
                Errors = x.Value!.Errors.Select(e => e.ErrorMessage)
            });

        return new BadRequestObjectResult(new
        {
            Message = "Validation failed",
            Errors = errors
        });
    };
});

builder.Services.Configure<BootstrapAdminOptions>(
    builder.Configuration.GetSection("BootstrapAdmin"));

builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection("Jwt"));

var jwtOptions = builder.Configuration
    .GetSection("Jwt")
    .Get<JwtOptions>() ?? throw new InvalidOperationException("JWT configuration is missing.");

jwtOptions.Key = builder.Configuration["Jwt:Key"] ?? jwtOptions.Key;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtOptions.Key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddScoped<ServiceRequestAuditLogService>();
builder.Services.AddScoped<IServiceRequestService, ServiceRequestService>();
builder.Services.AddScoped<IPasswordHasherService, PasswordHasherService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var allowedOrigins = builder.Configuration
    .GetSection("AllowedOrigins")
    .Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Backoffice Service Portal API",
        Version = "v1",
        Description = "REST API for managing service requests in the Backoffice Service Portal."
    });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter a valid JWT bearer token."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);

    options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

// Configure middleware pipeline
app.UseSwagger();
app.UseSwaggerUI();

if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseCors("FrontendPolicy");

app.UseAuthentication();
app.UseAuthorization();

// Map controller endpoints
app.MapControllers();
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/live", new()
{
    Predicate = registration => registration.Tags.Count == 0
});
app.MapHealthChecks("/health/ready", new()
{
    Predicate = registration => registration.Tags.Contains("ready")
});

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var passwordHasherService = scope.ServiceProvider.GetRequiredService<IPasswordHasherService>();
    var bootstrapAdminOptions = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<BootstrapAdminOptions>>();

    var migrationTimeout = TimeSpan.FromSeconds(30);
    var retryDelay = TimeSpan.FromSeconds(2);
    var startedAtUtc = DateTime.UtcNow;
    var attempt = 0;

    while (true)
    {
        attempt++;

        try
        {
            await dbContext.Database.MigrateAsync();
            break;
        }
        catch (Exception ex) when (DateTime.UtcNow - startedAtUtc < migrationTimeout)
        {
            logger.LogWarning(
                ex,
                "Database migration attempt {Attempt} failed. Retrying in {DelaySeconds} seconds.",
                attempt,
                retryDelay.TotalSeconds);

            await Task.Delay(retryDelay);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Database migration failed after {AttemptCount} attempts over {TimeoutSeconds} seconds.",
                attempt,
                migrationTimeout.TotalSeconds);

            throw;
        }
    }

    async Task BootstrapUserAsync(BootstrapAdminOptions options, string roleName)
    {
        if (string.IsNullOrWhiteSpace(options.Username) ||
            string.IsNullOrWhiteSpace(options.Email) ||
            string.IsNullOrWhiteSpace(options.FullName) ||
            string.IsNullOrWhiteSpace(options.Password))
        {
            return;
        }

        var existingUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Username == options.Username);

        if (existingUser is not null)
        {
            return;
        }

        var role = await dbContext.Roles.FirstOrDefaultAsync(r => r.Name == roleName);

        if (role is null)
        {
            return;
        }

        var user = new User
        {
            Username = options.Username,
            Email = options.Email,
            FullName = options.FullName,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        user.PasswordHash = passwordHasherService.HashPassword(user, options.Password);

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        dbContext.UserRoles.Add(new UserRole
        {
            UserId = user.Id,
            RoleId = role.Id
        });

        await dbContext.SaveChangesAsync();
    }

    await BootstrapUserAsync(bootstrapAdminOptions.Value, ApplicationRoles.Admin);
    await BootstrapUserAsync(
        app.Configuration.GetSection("BootstrapOperator").Get<BootstrapAdminOptions>() ?? new BootstrapAdminOptions(),
        ApplicationRoles.Operator);
    await BootstrapUserAsync(
        app.Configuration.GetSection("BootstrapViewer").Get<BootstrapAdminOptions>() ?? new BootstrapAdminOptions(),
        ApplicationRoles.Viewer);
}

app.Run();

public partial class Program;
