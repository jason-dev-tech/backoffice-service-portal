namespace BackofficeServicePortal.Api.DTOs.ServiceRequests;

public class PagedServiceRequestsResponseDto
{
    public List<ServiceRequestResponseDto> Items { get; set; } = [];

    public int Page { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }

    public int TotalPages { get; set; }
}
