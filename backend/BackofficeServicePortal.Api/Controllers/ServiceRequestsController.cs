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
}