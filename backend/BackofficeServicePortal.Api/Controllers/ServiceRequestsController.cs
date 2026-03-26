using BackofficeServicePortal.Api.Data;
using Microsoft.AspNetCore.Mvc;

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
}