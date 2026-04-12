using BackofficeServicePortal.Api.DTOs.ServiceRequests;
using BackofficeServicePortal.Api.Models;
using BackofficeServicePortal.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BackofficeServicePortal.Api.Services.Interfaces;

namespace BackofficeServicePortal.Api.Controllers;

/// <summary>
/// Provides CRUD operations for service requests.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class ServiceRequestsController : ControllerBase
{
    private readonly IServiceRequestService _serviceRequestService;
    private readonly ServiceRequestAuditLogService _auditLogService;

    public ServiceRequestsController(
        IServiceRequestService serviceRequestService,
        ServiceRequestAuditLogService auditLogService)
    {
        _serviceRequestService = serviceRequestService;
        _auditLogService = auditLogService;
    }

    /// <summary>
    /// Gets dashboard summary counts for service requests.
    /// </summary>
    /// <returns>Summary counts grouped for the dashboard.</returns>
    [HttpGet("dashboard")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ServiceRequestDashboardDto>> GetDashboard()
    {
        var response = await _serviceRequestService.GetDashboardAsync();
        return Ok(response);
    }

    /// <summary>
    /// Gets all service requests ordered by creation date in descending order.
    /// </summary>
    /// <param name="status">Optional status filter.</param>
    /// <param name="sort">Optional sort order.</param>
    /// <param name="page">Page number.</param>
    /// <param name="pageSize">Page size.</param>
    /// <returns>A paginated list of service requests.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedServiceRequestsResponseDto>> GetServiceRequests(
        [FromQuery] string? status,
        [FromQuery] string? sort,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var response = await _serviceRequestService.GetAllAsync(status, sort, page, pageSize);
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
        var response = await _serviceRequestService.GetByIdAsync(id);

        if (response == null)
        {
            return NotFound();
        }

        return Ok(response);
    }

    /// <summary>
    /// Creates a new service request.
    /// </summary>
    /// <param name="dto">The service request payload to create.</param>
    /// <returns>The created service request.</returns>
    [Authorize(Roles = $"{ApplicationRoles.Admin},{ApplicationRoles.Operator}")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ServiceRequestResponseDto>> CreateServiceRequest(CreateServiceRequestDto dto)
    {
        var response = await _serviceRequestService.CreateAsync(dto);

        return CreatedAtAction(
            nameof(GetServiceRequestById),
            new { id = response.Id },
            response
        );
    }

    /// <summary>
    /// Creates multiple service requests.
    /// </summary>
    /// <param name="dtos">The service request payloads to create.</param>
    /// <returns>The created service requests.</returns>
    [Authorize(Roles = $"{ApplicationRoles.Admin},{ApplicationRoles.Operator}")]
    [HttpPost("batch")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<ServiceRequestResponseDto>>> CreateServiceRequestsBatch(
        [FromBody] IEnumerable<CreateServiceRequestDto> dtos)
    {
        var response = await _serviceRequestService.CreateBatchAsync(dtos);

        return StatusCode(StatusCodes.Status201Created, response);
    }

    /// <summary>
    /// Updates an existing service request.
    /// </summary>
    /// <param name="id">The ID of the service request to update.</param>
    /// <param name="dto">The updated service request payload.</param>
    /// <returns>The updated service request.</returns>
    [Authorize(Roles = $"{ApplicationRoles.Admin},{ApplicationRoles.Operator}")]
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServiceRequestResponseDto>> UpdateServiceRequest(
        int id,
        UpdateServiceRequestDto dto)
    {
        var response = await _serviceRequestService.UpdateAsync(id, dto);

        if (response == null)
        {
            return NotFound();
        }

        return Ok(response);
    }

    /// <summary>
    /// Deletes a service request by its identifier.
    /// </summary>
    /// <param name="id">The ID of the service request to delete.</param>
    /// <returns>No content if the deletion is successful.</returns>
    [Authorize(Roles = ApplicationRoles.Admin)]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteServiceRequest(int id)
    {
        var deleted = await _serviceRequestService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound();
        }

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
