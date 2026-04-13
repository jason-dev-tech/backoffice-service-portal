using BackofficeServicePortal.Api.Data;
using BackofficeServicePortal.Api.DTOs.ServiceRequests;
using BackofficeServicePortal.Api.Models;
using BackofficeServicePortal.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BackofficeServicePortal.Api.Services;

public class ServiceRequestService : IServiceRequestService
{
    private sealed class ServiceRequestStatusCount
    {
        public string Status { get; set; } = string.Empty;

        public int Count { get; set; }
    }

    private readonly AppDbContext _dbContext;
    private readonly ServiceRequestAuditLogService _auditLogService;

    public ServiceRequestService(
        AppDbContext dbContext,
        ServiceRequestAuditLogService auditLogService)
    {
        _dbContext = dbContext;
        _auditLogService = auditLogService;
    }

    public async Task<ServiceRequestDashboardDto> GetDashboardAsync()
    {
        var statusCounts = await _dbContext.ServiceRequests
            .AsNoTracking()
            .GroupBy(sr => sr.Status.ToLower())
            .Select(group => new ServiceRequestStatusCount
            {
                Status = group.Key,
                Count = group.Count()
            })
            .ToListAsync();

        var totalRequests = statusCounts.Sum(item => item.Count);
        var openRequests = GetStatusCount(statusCounts, "Open");
        var inProgressRequests = GetStatusCount(statusCounts, "In Progress");
        var closedRequests = GetStatusCount(statusCounts, "Closed");
        var oldestOpenRequestCreatedAt = await _dbContext.ServiceRequests
            .AsNoTracking()
            .Where(sr => EF.Functions.ILike(sr.Status, "Open"))
            .OrderBy(sr => sr.CreatedAt)
            .Select(sr => (DateTime?)sr.CreatedAt)
            .FirstOrDefaultAsync();
        var mostRecentRequestCreatedAt = await _dbContext.ServiceRequests
            .AsNoTracking()
            .OrderByDescending(sr => sr.CreatedAt)
            .Select(sr => (DateTime?)sr.CreatedAt)
            .FirstOrDefaultAsync();

        return new ServiceRequestDashboardDto
        {
            TotalRequests = totalRequests,
            OpenRequests = openRequests,
            InProgressRequests = inProgressRequests,
            ClosedRequests = closedRequests,
            OldestOpenRequestCreatedAt = oldestOpenRequestCreatedAt,
            MostRecentRequestCreatedAt = mostRecentRequestCreatedAt,
            OpenSharePercentage = GetPercentage(openRequests, totalRequests),
            ClosedSharePercentage = GetPercentage(closedRequests, totalRequests),
            StatusDistribution = statusCounts
                .OrderByDescending(item => item.Count)
                .Select(item => new ServiceRequestStatusDistributionDto
                {
                    Status = NormalizeDashboardStatus(item.Status),
                    Count = item.Count,
                    Percentage = GetPercentage(item.Count, totalRequests)
                })
                .ToList()
        };
    }

    public async Task<PagedServiceRequestsResponseDto> GetAllAsync(ServiceRequestQueryParams query)
    {
        var normalizedPage = Math.Max(query.Page, 1);
        var normalizedPageSize = Math.Max(query.PageSize, 1);

        var serviceRequestsQuery = _dbContext.ServiceRequests
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            var normalizedStatus = query.Status.Trim();
            serviceRequestsQuery = serviceRequestsQuery.Where(
                sr => EF.Functions.ILike(sr.Status, normalizedStatus));
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var normalizedSearch = $"%{query.Search.Trim()}%";
            serviceRequestsQuery = serviceRequestsQuery.Where(sr =>
                EF.Functions.ILike(sr.Title, normalizedSearch) ||
                EF.Functions.ILike(sr.Description, normalizedSearch) ||
                EF.Functions.ILike(sr.RequesterName, normalizedSearch));
        }

        var totalCount = await serviceRequestsQuery.CountAsync();
        var totalPages = totalCount == 0
            ? 0
            : (int)Math.Ceiling((double)totalCount / normalizedPageSize);
        var effectivePage = totalPages > 0 && normalizedPage > totalPages
            ? totalPages
            : normalizedPage;

        serviceRequestsQuery = ApplySorting(serviceRequestsQuery, query.Sort);

        var items = await serviceRequestsQuery
            .Skip((effectivePage - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .Select(sr => sr.ToResponseDto())
            .ToListAsync();

        return new PagedServiceRequestsResponseDto
        {
            Items = items,
            Page = effectivePage,
            PageSize = normalizedPageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
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
            Status = string.IsNullOrWhiteSpace(dto.Status) ? "Open" : dto.Status,
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

    public async Task<IEnumerable<ServiceRequestResponseDto>> CreateBatchAsync(IEnumerable<CreateServiceRequestDto> dtos)
    {
        var serviceRequests = dtos.Select(dto => new ServiceRequest
        {
            Title = dto.Title,
            Description = dto.Description,
            RequesterName = dto.RequesterName,
            Status = string.IsNullOrWhiteSpace(dto.Status) ? "Open" : dto.Status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        }).ToList();

        await _dbContext.ServiceRequests.AddRangeAsync(serviceRequests);
        await _dbContext.SaveChangesAsync();

        return serviceRequests.Select(sr => sr.ToResponseDto());
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

    private static int GetStatusCount(
        IEnumerable<ServiceRequestStatusCount> statusCounts,
        string status)
    {
        foreach (var item in statusCounts)
        {
            if (string.Equals(item.Status, status, StringComparison.OrdinalIgnoreCase))
            {
                return item.Count;
            }
        }

        return 0;
    }

    private static double GetPercentage(int count, int total)
    {
        if (total == 0)
        {
            return 0;
        }

        return Math.Round((double)count / total * 100, 1);
    }

    private static string NormalizeDashboardStatus(string status)
    {
        return status switch
        {
            "open" => "Open",
            "in progress" => "In Progress",
            "closed" => "Closed",
            _ => status
        };
    }

    private static IQueryable<ServiceRequest> ApplySorting(
        IQueryable<ServiceRequest> query,
        string? sort)
    {
        return sort?.Trim() switch
        {
            "createdAt_asc" => query.OrderBy(sr => sr.CreatedAt),
            "title_asc" => query.OrderBy(sr => sr.Title).ThenByDescending(sr => sr.CreatedAt),
            "title_desc" => query.OrderByDescending(sr => sr.Title).ThenByDescending(sr => sr.CreatedAt),
            "status_asc" => query.OrderBy(sr => sr.Status).ThenByDescending(sr => sr.CreatedAt),
            "status_desc" => query.OrderByDescending(sr => sr.Status).ThenByDescending(sr => sr.CreatedAt),
            _ => query.OrderByDescending(sr => sr.CreatedAt)
        };
    }
}
