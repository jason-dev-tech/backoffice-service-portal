using BackofficeServicePortal.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BackofficeServicePortal.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<ServiceRequest> ServiceRequests => Set<ServiceRequest>();
}