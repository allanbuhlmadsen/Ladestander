using Ladestander.Api.Data;
using Ladestander.Api.Entities;
using Ladestander.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Ladestander.Api.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly AppDbContext _context;

        public CustomerRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Customer>> GetAllAsync()
        {
            return await _context.Customers
                .Where(c => c.IsActive)
                .ToListAsync();
        }

        public async Task<Customer?> GetByIdAsync(int customerId)
        {
            return await _context.Customers
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);
        }

        public async Task<Customer> AddAsync(Customer customer)
        {
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            return customer;
        }

        public async Task<bool> ExistsByRfidNumberAsync(string rfidNumber)
        {
            return await _context.Customers
                .AnyAsync(c => c.RfidNumber == rfidNumber);
        }

        public async Task<Customer> UpdateAsync(Customer customer)
        {
            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();
            return customer;
        }

        public async Task<Customer> SoftDeleteAsync(Customer customer)
        {
            customer.IsActive = false;

            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();

            return customer;
        }
    }
}