using BackofficeServicePortal.Api.Models;

namespace BackofficeServicePortal.Api.DTOs.ServiceRequests;

public static class ServiceRequestMappings
{
    public static ServiceRequestResponseDto ToResponseDto(this ServiceRequest entity)
    {
        return new ServiceRequestResponseDto
        {
            Id = entity.Id,
            Title = entity.Title,
            Description = entity.Description,
            RequesterName = entity.RequesterName,
            Status = entity.Status,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}