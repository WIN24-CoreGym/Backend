using Business.Dtos.Admin;
using Business.Dtos.Customer;
using Business.Interfaces;
using Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;
[Route("api/[controller]")]
[ApiController]
//[Authorize(Roles = "Admin")]
public class AdminsController(IAdminService adminService) : ControllerBase
{
    private readonly IAdminService _adminService = adminService;

    [HttpGet]
    public async Task<IActionResult> GetAllAdmins()
    {
        var result = await _adminService.GetAllAdminsAsync();

        return result.Succeeded
            ? Ok(result.Data)
            : StatusCode(result.StatusCode, "Failed retrieving users.");
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetAdminById(string userId)
    {
        var result = await _adminService.GetAdminByIdAsync(userId);

        return result.Succeeded
            ? Ok(result.Data)
            : StatusCode(result.StatusCode, "Failed retrieving user.");
    }

    [HttpPost]
    public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminDto dto)
    {
        var result = await _adminService.CreateAsync(dto);

        return result.Succeeded
            ? Ok(result.Data)
            : StatusCode(result.StatusCode, "Failed creating admin.");
    }

    [HttpPut]
    public async Task<IActionResult> UpdateAdmin([FromBody] UpdateAdminDto dto)
    {
        var result = await _adminService.UpdateAdminAsync(dto);

        return result.Succeeded
            ? Ok(result.Data)
            : StatusCode(result.StatusCode, "Failed updating admin.");
    }

    [HttpDelete("{userId}")]
    public async Task<IActionResult> DeleteAdmin(string userId)
    {
        var result = await _adminService.DeleteAdminByIdAsync(userId);

        return result.Succeeded
            ? Ok(result.Data)
            : StatusCode(result.StatusCode, "Failed deleting admin.");
    }
}
