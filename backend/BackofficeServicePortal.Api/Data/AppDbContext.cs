using BackofficeServicePortal.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BackofficeServicePortal.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<ServiceRequest> ServiceRequests => Set<ServiceRequest>();

    public DbSet<User> Users => Set<User>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<UserRole> UserRoles => Set<UserRole>();

    public DbSet<ServiceRequestAuditLogEntry> ServiceRequestAuditLogEntries => Set<ServiceRequestAuditLogEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserRole>()
            .HasKey(ur => new { ur.UserId, ur.RoleId });

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Role>()
            .HasIndex(r => r.Name)
            .IsUnique();

        modelBuilder.Entity<Role>()
            .HasData(
                new Role { Id = 1, Name = "Admin" },
                new Role { Id = 2, Name = "Operator" },
                new Role { Id = 3, Name = "Viewer" });

        modelBuilder.Entity<ServiceRequestAuditLogEntry>()
            .HasIndex(log => log.ServiceRequestId);

        modelBuilder.Entity<ServiceRequestAuditLogEntry>()
            .Property(log => log.Action)
            .HasMaxLength(50);

        modelBuilder.Entity<ServiceRequestAuditLogEntry>()
            .Property(log => log.Details)
            .HasColumnType("jsonb");

        modelBuilder.Entity<ServiceRequestAuditLogEntry>()
            .HasOne<ServiceRequest>()
            .WithMany()
            .HasForeignKey(log => log.ServiceRequestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
