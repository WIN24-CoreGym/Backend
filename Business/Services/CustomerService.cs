using Business.Dtos.Customer;
using Business.Interfaces;
using Business.Models;
using Business.Models.Results;
using Data.Entities;
using Data.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Business.Services;
public class CustomerService(UserManager<AppUser> userManager, IAppUserRepository appUserRepository) : ICustomerService
{
    private readonly UserManager<AppUser> _userManager = userManager;
    private readonly IAppUserRepository _appUserRepository = appUserRepository;

    // CREATE
    // Create customer without setting password
    public async Task<CustomerResult<CustomerDto>> CreateAsync(CreateCustomerDto dto)
    {
        await _appUserRepository.BeginTransactionAsync();
        try
        {
            var user = new AppUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Created = DateTime.UtcNow
            };

            var createResult = await _userManager.CreateAsync(user);
            var createdUser = await _userManager.FindByEmailAsync(dto.Email);
            if (!createResult.Succeeded || createdUser == null)
            {
                await _appUserRepository.RollbackTransactionAsync();
                return CustomerResult<CustomerDto>.InternalServerError("Failed creating customer. Rolling back.");
            }

            var applyRoleResult = await _userManager.AddToRoleAsync(createdUser, "Customer");
            if (!applyRoleResult.Succeeded)
            {
                await _appUserRepository.RollbackTransactionAsync();
                return CustomerResult<CustomerDto>.InternalServerError("Failed adding role to customer after creation. Rolling back.");
            }

            await _appUserRepository.CommitTransactionAsync();
            var createdUserDto = new CustomerDto
            {
                UserId = createdUser.Id,
                Email = createdUser.Email!,
                FirstName = createdUser.FirstName,
                LastName = createdUser.LastName,

            };

            return CustomerResult<CustomerDto>.Ok(createdUserDto);
        }
        catch
        {
            await _appUserRepository.RollbackTransactionAsync();
            return CustomerResult<CustomerDto>.InternalServerError("Unexpected error occured. Rolling back.");
        }
    }

    // READ
    public async Task<CustomerResult<IEnumerable<CustomerDto>>> GetAllCustomersAsync()
    {
        var customers = await _userManager.GetUsersInRoleAsync("Customer");
        var customersDto = customers.Select(x => new CustomerDto
        {
            UserId = x.Id,
            Email = x.Id,
            FirstName = x.FirstName,
            LastName = x.LastName
        });

        return CustomerResult<IEnumerable<CustomerDto>>.Ok(customersDto);
    }

    public async Task<CustomerResult<CustomerDto>> GetCustomerByIdAsync(string userId)
    {
        var customer = await _userManager.FindByIdAsync(userId);
        if (customer == null)
            return CustomerResult<CustomerDto>.NotFound($"Customer with id {userId} not found.");

        var customerDto = new CustomerDto
        {
            UserId = customer.Id,
            Email = customer.Email!,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
        };

        return CustomerResult<CustomerDto>.Ok(customerDto);
    }

    public async Task<CustomerResult<CustomerDto>> GetCustomerByEmailAsync(string email)
    {
        var customer = await _userManager.FindByEmailAsync(email);
        if (customer == null)
            return CustomerResult<CustomerDto>.NotFound($"Customer with email {email} not found.");

        var customerDto = new CustomerDto
        {
            UserId = customer.Id,
            Email = customer.Email!,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
        };

        return CustomerResult<CustomerDto>.Ok(customerDto);
    }


    // UPDATE
    public async Task<CustomerResult<CustomerDto>> UpdateCustomerAsync(UpdateCustomerDto dto)
    {
        var customer = await _userManager.FindByIdAsync(dto.UserId);
        if (customer == null)
            return CustomerResult<CustomerDto>.NotFound($"Customer with id {dto.UserId} not found.");

        customer.Email = dto.Email;
        customer.FirstName = dto.FirstName;
        customer.LastName = dto.LastName;

        var result = await _userManager.UpdateAsync(customer);
        if (!result.Succeeded)
            return CustomerResult<CustomerDto>.InternalServerError("Failed updating user.");

        var updatedCustomer = await _userManager.FindByIdAsync(dto.UserId);
        var updatedCustomerDto = new CustomerDto
        {
            UserId = dto.UserId,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName
        };

        return CustomerResult<CustomerDto>.Ok(updatedCustomerDto);
    }


    // DELETE
    public async Task<CustomerResult<string?>> DeleteCustomerByIdAsync(string userId)
    {
        var customer = await _userManager.FindByIdAsync(userId);
        if (customer == null)
            return CustomerResult<string?>.NotFound($"Customer with id {userId} not found.");

        var result = await _userManager.DeleteAsync(customer);
        if (!result.Succeeded)
            return CustomerResult<string?>.InternalServerError($"Failed deleting customer with id {userId}");

        return CustomerResult<string?>.Ok($"User with id {userId} deleted successfully.");
    }
}
