using BackofficeServicePortal.Api.DTOs.ServiceRequests;

namespace BackofficeServicePortal.Api.Services.Interfaces;

public interface IServiceRequestService
{
    Task<IEnumerable<ServiceRequestResponseDto>> GetAllAsync();
    Task<ServiceRequestResponseDto?> GetByIdAsync(int id);
    Task<ServiceRequestResponseDto> CreateAsync(CreateServiceRequestDto dto);
    Task<IEnumerable<ServiceRequestResponseDto>> CreateBatchAsync(IEnumerable<CreateServiceRequestDto> dtos);
    Task<ServiceRequestResponseDto?> UpdateAsync(int id, UpdateServiceRequestDto dto);
    Task<bool> DeleteAsync(int id);
}
