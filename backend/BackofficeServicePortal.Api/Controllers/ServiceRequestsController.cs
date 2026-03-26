using BackofficeServicePortal.Api.Data;
using BackofficeServicePortal.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackofficeServicePortal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServiceRequestsController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public ServiceRequestsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ServiceRequest>>> GetServiceRequests()
    {
        var serviceRequests = await _dbContext.ServiceRequests.ToListAsync();
        return Ok(serviceRequests);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ServiceRequest>> GetServiceRequestById(int id)
    {
        var serviceRequest = await _dbContext.ServiceRequests.FindAsync(id);

        if (serviceRequest == null)
        {
            return NotFound();
        }

        return Ok(serviceRequest);
    }

    [HttpPost]
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

    [HttpPut("{id}")]
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

        // Update fields
        existingRequest.Title = updatedRequest.Title;
        existingRequest.Description = updatedRequest.Description;
        existingRequest.RequesterName = updatedRequest.RequesterName;
        existingRequest.Status = updatedRequest.Status;
        existingRequest.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return NoContent();
    }
}