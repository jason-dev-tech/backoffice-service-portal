using BackofficeServicePortal.Api.Data;
using BackofficeServicePortal.Api.DTOs.ServiceRequests;
using BackofficeServicePortal.Api.Models;
using BackofficeServicePortal.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BackofficeServicePortal.Api.Services;

public class ServiceRequestService : IServiceRequestService
{
    private readonly AppDbContext _dbContext;
    private readonly ServiceRequestAuditLogService _auditLogService;

    public ServiceRequestService(
        AppDbContext dbContext,
        ServiceRequestAuditLogService auditLogService)
    {
        _dbContext = dbContext;
        _auditLogService = auditLogService;
    }

    public async Task<IEnumerable<ServiceRequestResponseDto>> GetAllAsync()
    {
        var serviceRequests = await _dbContext.ServiceRequests
            .OrderByDescending(sr => sr.CreatedAt)
            .ToListAsync();

        return serviceRequests.Select(sr => sr.ToResponseDto());
    }

    public async Task<ServiceRequestResponseDto?> GetByIdAsync(int id)
    {
        var serviceRequest = await _dbContext.ServiceRequests.FindAsync(id);

        return serviceRequest?.ToResponseDto();
    }

    public async Task<ServiceRequestResponseDto> CreateAsync(CreateServiceRequestDto dto)
    {
        var serviceRequest = new ServiceRequest
        {
            Title = dto.Title,
            Description = dto.Description,
            RequesterName = dto.RequesterName,
            Status = "Open",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };

        _dbContext.ServiceRequests.Add(serviceRequest);
        await _dbContext.SaveChangesAsync();

        await _auditLogService.LogAsync(new ServiceRequestAuditLog
        {
            ServiceRequestId = serviceRequest.Id,
            Action = "Created",
            TimestampUtc = DateTime.UtcNow,
            Details = $"Service request '{serviceRequest.Title}' was created."
        });

        return serviceRequest.ToResponseDto();
    }

    public async Task<ServiceRequestResponseDto?> UpdateAsync(int id, UpdateServiceRequestDto dto)
    {
        var existingRequest = await _dbContext.ServiceRequests.FindAsync(id);

        if (existingRequest == null)
        {
            return null;
        }

        existingRequest.Title = dto.Title;
        existingRequest.Description = dto.Description;
        existingRequest.RequesterName = dto.RequesterName;
        existingRequest.Status = dto.Status;
        existingRequest.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        await _auditLogService.LogAsync(new ServiceRequestAuditLog
        {
            ServiceRequestId = existingRequest.Id,
            Action = "Updated",
            TimestampUtc = DateTime.UtcNow,
            Details = $"Service request '{existingRequest.Title}' was updated."
        });

        return existingRequest.ToResponseDto();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var serviceRequest = await _dbContext.ServiceRequests.FindAsync(id);

        if (serviceRequest == null)
        {
            return false;
        }

        var deletedRequestId = serviceRequest.Id;
        var deletedRequestTitle = serviceRequest.Title;

        _dbContext.ServiceRequests.Remove(serviceRequest);
        await _dbContext.SaveChangesAsync();

        await _auditLogService.LogAsync(new ServiceRequestAuditLog
        {
            ServiceRequestId = deletedRequestId,
            Action = "Deleted",
            TimestampUtc = DateTime.UtcNow,
            Details = $"Service request '{deletedRequestTitle}' was deleted."
        });

        return true;
    }
}