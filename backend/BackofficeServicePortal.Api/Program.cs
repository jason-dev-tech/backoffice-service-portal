using BackofficeServicePortal.Api.Data;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using BackofficeServicePortal.Api.Models;
using BackofficeServicePortal.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddControllers();

builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));

builder.Services.AddSingleton<ServiceRequestAuditLogService>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Backoffice Service Portal API",
        Version = "v1",
        Description = "REST API for managing service requests in the Backoffice Service Portal."
    });

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);

    options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

// Configure middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

// Map controller endpoints
app.MapControllers();

app.Run();