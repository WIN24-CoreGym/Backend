using Business.Dtos.Customer;
using Business.Models.Results;

namespace Business.Interfaces;
public interface ICustomerService
{
    Task<CustomerResult<CustomerDto>> CreateAsync(CreateCustomerDto dto);
    Task<CustomerResult<string?>> DeleteCustomerByIdAsync(string userId);
    Task<CustomerResult<IEnumerable<CustomerDto>>> GetAllCustomersAsync();
    Task<CustomerResult<CustomerDto>> GetCustomerByEmailAsync(string email);
    Task<CustomerResult<CustomerDto>> GetCustomerByIdAsync(string userId);
    Task<CustomerResult<CustomerDto>> UpdateCustomerAsync(UpdateCustomerDto dto);
}