namespace BackofficeServicePortal.Api.DTOs.ServiceRequests;

public class ServiceRequestDashboardDto
{
    public int TotalRequests { get; set; }

    public int OpenRequests { get; set; }

    public int InProgressRequests { get; set; }

    public int ClosedRequests { get; set; }
}
