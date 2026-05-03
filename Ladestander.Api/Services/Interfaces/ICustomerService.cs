using Ladestander.Api.DTOs.Customers;

namespace Ladestander.Api.Services.Interfaces
{
    public interface ICustomerService
    {
        Task<List<CustomerResponseDto>> GetAllAsync();
        Task<CustomerResponseDto?> GetByIdAsync(int customerId);
        Task<CustomerResponseDto> CreateAsync(CreateCustomerRequestDto request);
        Task<CustomerResponseDto?> UpdateAsync(int customerId, UpdateCustomerRequestDto request);
        Task<CustomerResponseDto?> SoftDeleteAsync(int customerId);
    }
}