using System.ComponentModel.DataAnnotations;

namespace BackofficeServicePortal.Api.DTOs.ServiceRequests;

public class CreateServiceRequestDto
{
    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string RequesterName { get; set; } = string.Empty;
}