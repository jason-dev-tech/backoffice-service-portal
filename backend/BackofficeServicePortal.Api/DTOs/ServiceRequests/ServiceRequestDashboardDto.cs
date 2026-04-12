namespace BackofficeServicePortal.Api.DTOs.ServiceRequests;

public class ServiceRequestStatusDistributionDto
{
    public string Status { get; set; } = string.Empty;

    public int Count { get; set; }

    public double Percentage { get; set; }
}

public class ServiceRequestDashboardDto
{
    public int TotalRequests { get; set; }

    public int OpenRequests { get; set; }

    public int InProgressRequests { get; set; }

    public int ClosedRequests { get; set; }

    public DateTime? OldestOpenRequestCreatedAt { get; set; }

    public DateTime? MostRecentRequestCreatedAt { get; set; }

    public double OpenSharePercentage { get; set; }

    public double ClosedSharePercentage { get; set; }

    public List<ServiceRequestStatusDistributionDto> StatusDistribution { get; set; } = [];
}
