using Ladestander.Api.DTOs.Customers;
using Ladestander.Api.Entities;
using Ladestander.Api.Repositories.Interfaces;
using Ladestander.Api.Services.Interfaces;

namespace Ladestander.Api.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;

        public CustomerService(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<List<CustomerResponseDto>> GetAllAsync()
        {
            var customers = await _customerRepository.GetAllAsync();

            return customers.Select(c => new CustomerResponseDto(
                c.CustomerId,
                c.RfidNumber,
                c.FullName,
                c.Email,
                c.Street,
                c.HouseNumber,
                c.PostalCode,
                c.City,
                c.IsActive
            )).ToList();
        }

        public async Task<CustomerResponseDto?> GetByIdAsync(int customerId)
        {
            var customer = await _customerRepository.GetByIdAsync(customerId);

            if (customer is null)
            {
                return null;
            }

            return new CustomerResponseDto(
                customer.CustomerId,
                customer.RfidNumber,
                customer.FullName,
                customer.Email,
                customer.Street,
                customer.HouseNumber,
                customer.PostalCode,
                customer.City,
                customer.IsActive
            );
        }

        public async Task<CustomerResponseDto> CreateAsync(CreateCustomerRequestDto request)
        {
            bool rfidAlreadyExists = await _customerRepository.ExistsByRfidNumberAsync(request.RfidNumber);

            if (rfidAlreadyExists)
            {
                throw new InvalidOperationException($"Customer with RFID number {request.RfidNumber} already exists.");
            }

            var customer = new Customer
            {
                RfidNumber = request.RfidNumber,
                FirstName = request.FirstName,
                MiddleName = request.MiddleName,
                LastName = request.LastName,
                FullName = $"{request.FirstName} {request.MiddleName} {request.LastName}".Replace("  ", " "),
                Email = request.Email,
                Street = request.Street,
                HouseNumber = request.HouseNumber,
                PostalCode = request.PostalCode,
                City = request.City,
                IsActive = true
            };

            var created = await _customerRepository.AddAsync(customer);

            return new CustomerResponseDto(
                created.CustomerId,
                created.RfidNumber,
                created.FullName,
                created.Email,
                created.Street,
                created.HouseNumber,
                created.PostalCode,
                created.City,
                created.IsActive
            );
        }

        public async Task<CustomerResponseDto?> UpdateAsync(int customerId, UpdateCustomerRequestDto request)
        {
            var customer = await _customerRepository.GetByIdAsync(customerId);

            if (customer is null)
            {
                return null;
            }

            customer.RfidNumber = request.RfidNumber;
            customer.FirstName = request.FirstName;
            customer.MiddleName = request.MiddleName;
            customer.LastName = request.LastName;
            customer.FullName = $"{request.FirstName} {request.MiddleName} {request.LastName}".Replace("  ", " ");
            customer.Email = request.Email;
            customer.Street = request.Street;
            customer.HouseNumber = request.HouseNumber;
            customer.PostalCode = request.PostalCode;
            customer.City = request.City;
            customer.IsActive = request.IsActive;

            var updated = await _customerRepository.UpdateAsync(customer);

            return new CustomerResponseDto(
                updated.CustomerId,
                updated.RfidNumber,
                updated.FullName,
                updated.Email,
                updated.Street,
                updated.HouseNumber,
                updated.PostalCode,
                updated.City,
                updated.IsActive
            );
        }

        public async Task<CustomerResponseDto?> SoftDeleteAsync(int customerId)
        {
            var customer = await _customerRepository.GetByIdAsync(customerId);

            if (customer is null)
            {
                return null;
            }

            var deleted = await _customerRepository.SoftDeleteAsync(customer);

            return new CustomerResponseDto(
                deleted.CustomerId,
                deleted.RfidNumber,
                deleted.FullName,
                deleted.Email,
                deleted.Street,
                deleted.HouseNumber,
                deleted.PostalCode,
                deleted.City,
                deleted.IsActive
            );
        }
    }
}