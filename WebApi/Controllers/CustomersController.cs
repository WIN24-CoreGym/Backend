using Business.Dtos.Customer;
using Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;
[Route("api/[controller]")]
[ApiController]
//[Authorize(Roles = "Admin")]
public class CustomersController(ICustomerService customerService) : ControllerBase
{
    private readonly ICustomerService _customerService = customerService;

    [HttpGet]
    public async Task<IActionResult> GetAllCustomers()
    {
        var customers = await _customerService.GetAllCustomersAsync();
        if (!customers.Succeeded)
            return StatusCode(customers.StatusCode);

        return Ok(customers.Data);
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetCustomerById(string userId)
    {
        var customer = await _customerService.GetCustomerByIdAsync(userId);
        if (!customer.Succeeded)
            return StatusCode(customer.StatusCode);

        return Ok(customer.Data);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerDto dto)
    {
        var result = await _customerService.CreateAsync(dto);
        if (!result.Succeeded)
            return StatusCode(result.StatusCode);

        return Ok(result.Data);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateCustomer([FromBody] UpdateCustomerDto dto)
    {
        var result = await _customerService.UpdateCustomerAsync(dto);
        if (!result.Succeeded)
            return StatusCode(result.StatusCode);

        return Ok(result.Data);
    }

    [HttpDelete("{userId}")]
    public async Task<IActionResult> DeleteCustomer(string userId)
    {
        var result = await _customerService.DeleteCustomerByIdAsync(userId);
        if (!result.Succeeded)
            return StatusCode(result.StatusCode);

        return Ok(result.Data);
    }
}
