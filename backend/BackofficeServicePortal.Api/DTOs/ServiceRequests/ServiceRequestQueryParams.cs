namespace BackofficeServicePortal.Api.DTOs.ServiceRequests;

public class ServiceRequestQueryParams
{
    public string? Status { get; set; }

    public string? Search { get; set; }

    public string? Sort { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}
