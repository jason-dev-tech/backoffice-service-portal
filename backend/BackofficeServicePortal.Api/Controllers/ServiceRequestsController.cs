using BackofficeServicePortal.Api.Data;
using BackofficeServicePortal.Api.DTOs.ServiceRequests;
using BackofficeServicePortal.Api.Models;
using BackofficeServicePortal.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackofficeServicePortal.Api.Controllers;

/// <summary>
/// Provides CRUD operations for service requests.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ServiceRequestsController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly ServiceRequestAuditLogService _auditLogService;

    public ServiceRequestsController(
        AppDbContext dbContext,
        ServiceRequestAuditLogService auditLogService)
    {
        _dbContext = dbContext;
        _auditLogService = auditLogService;
    }

    /// <summary>
    /// Gets all service requests ordered by creation date in descending order.
    /// </summary>
    /// <returns>A list of service requests.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ServiceRequestResponseDto>>> GetServiceRequests()
    {
        var serviceRequests = await _dbContext.ServiceRequests
            .OrderByDescending(sr => sr.CreatedAt)
            .ToListAsync();

        var response = serviceRequests
            .Select(sr => sr.ToResponseDto())
            .ToList();

        return Ok(response);
    }

    /// <summary>
    /// Gets a single service request by its identifier.
    /// </summary>
    /// <param name="id">The ID of the service request.</param>
    /// <returns>The matching service request if found.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServiceRequestResponseDto>> GetServiceRequestById(int id)
    {
        var serviceRequest = await _dbContext.ServiceRequests.FindAsync(id);

        if (serviceRequest == null)
        {
            return NotFound();
        }

        return Ok(serviceRequest.ToResponseDto());
    }

    /// <summary>
    /// Creates a new service request.
    /// </summary>
    /// <param name="dto">The service request payload to create.</param>
    /// <returns>The created service request.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ServiceRequestResponseDto>> CreateServiceRequest(CreateServiceRequestDto dto)
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

        return CreatedAtAction(
            nameof(GetServiceRequestById),
            new { id = serviceRequest.Id },
            serviceRequest.ToResponseDto()
        );
    }

    /// <summary>
    /// Updates an existing service request.
    /// </summary>
    /// <param name="id">The ID of the service request to update.</param>
    /// <param name="dto">The updated service request payload.</param>
    /// <returns>The updated service request.</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServiceRequestResponseDto>> UpdateServiceRequest(
        int id,
        UpdateServiceRequestDto dto)
    {
        var existingRequest = await _dbContext.ServiceRequests.FindAsync(id);

        if (existingRequest == null)
        {
            return NotFound();
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

        return Ok(existingRequest.ToResponseDto());
    }

    /// <summary>
    /// Deletes a service request by its identifier.
    /// </summary>
    /// <param name="id">The ID of the service request to delete.</param>
    /// <returns>No content if the deletion is successful.</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteServiceRequest(int id)
    {
        var serviceRequest = await _dbContext.ServiceRequests.FindAsync(id);

        if (serviceRequest == null)
        {
            return NotFound();
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

        return NoContent();
    }

    /// <summary>
    /// Gets audit logs for a specific service request.
    /// </summary>
    /// <param name="id">The ID of the service request.</param>
    /// <returns>A list of audit logs related to the service request.</returns>
    [HttpGet("{id}/audit-logs")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ServiceRequestAuditLog>>> GetAuditLogs(int id)
    {
        var logs = await _auditLogService.GetLogsByServiceRequestIdAsync(id);
        return Ok(logs);
    }
}