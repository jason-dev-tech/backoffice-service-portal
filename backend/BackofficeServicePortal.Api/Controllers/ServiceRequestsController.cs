using BackofficeServicePortal.Api.Data;
using BackofficeServicePortal.Api.Models;
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

    public ServiceRequestsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Gets all service requests ordered by creation date in descending order.
    /// </summary>
    /// <returns>A list of service requests.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ServiceRequest>>> GetServiceRequests()
    {
        var serviceRequests = await _dbContext.ServiceRequests
            .OrderByDescending(sr => sr.CreatedAt)
            .ToListAsync();

        return Ok(serviceRequests);
    }

    /// <summary>
    /// Gets a single service request by its identifier.
    /// </summary>
    /// <param name="id">The ID of the service request.</param>
    /// <returns>The matching service request if found.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServiceRequest>> GetServiceRequestById(int id)
    {
        var serviceRequest = await _dbContext.ServiceRequests.FindAsync(id);

        if (serviceRequest == null)
        {
            return NotFound();
        }

        return Ok(serviceRequest);
    }

    /// <summary>
    /// Creates a new service request.
    /// </summary>
    /// <param name="serviceRequest">The service request to create.</param>
    /// <returns>The created service request.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ServiceRequest>> CreateServiceRequest(ServiceRequest serviceRequest)
    {
        serviceRequest.CreatedAt = DateTime.UtcNow;
        serviceRequest.UpdatedAt = null;

        _dbContext.ServiceRequests.Add(serviceRequest);
        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetServiceRequestById),
            new { id = serviceRequest.Id },
            serviceRequest
        );
    }

    /// <summary>
    /// Updates an existing service request.
    /// </summary>
    /// <param name="id">The ID of the service request to update.</param>
    /// <param name="updatedRequest">The updated service request payload.</param>
    /// <returns>No content if the update is successful.</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateServiceRequest(int id, ServiceRequest updatedRequest)
    {
        if (id != updatedRequest.Id)
        {
            return BadRequest();
        }

        var existingRequest = await _dbContext.ServiceRequests.FindAsync(id);

        if (existingRequest == null)
        {
            return NotFound();
        }

        existingRequest.Title = updatedRequest.Title;
        existingRequest.Description = updatedRequest.Description;
        existingRequest.RequesterName = updatedRequest.RequesterName;
        existingRequest.Status = updatedRequest.Status;
        existingRequest.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return NoContent();
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

        _dbContext.ServiceRequests.Remove(serviceRequest);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }
}