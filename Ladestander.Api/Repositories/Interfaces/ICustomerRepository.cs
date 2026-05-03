using Ladestander.Api.Entities;

namespace Ladestander.Api.Repositories.Interfaces
{
    public interface ICustomerRepository
    {
        Task<List<Customer>> GetAllAsync();
        Task<Customer?> GetByIdAsync(int customerId);
        Task<Customer> AddAsync(Customer customer);
        Task<bool> ExistsByRfidNumberAsync(string rfidNumber);
        Task<Customer> UpdateAsync(Customer customer);
        Task<Customer> SoftDeleteAsync(Customer customer);
        Task<Customer?> GetByFullNameAsync(string fullName);
    }
}