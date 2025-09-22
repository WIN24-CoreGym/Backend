using Business.Dtos;
using Business.Interfaces;
using Business.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.RegisterAsync(dto);
        if (result.Succeeded)
        {
            return Ok(new { Message = "User registered successfully" });
        }

        var errors = result.Errors.Select(e => e.Description).ToList();
        return BadRequest(new { Errors = errors });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.LoginAsync(dto);
        if (!result.Succeeded)
        {
            return Unauthorized(new { message = "Fel e-post eller lösenord."});
        }
        return Ok(new { token = "jwt-token-here" });
    }

    [HttpPut("update/{userId}")]
    public async Task<IActionResult> UpdateUser(string userId, [FromBody] UpdateUserDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (string.IsNullOrEmpty(userId))
            return BadRequest(new { Message = "User ID is required" });

        var result = await _authService.UpdateUserAsync(userId, dto);
        if (result.Succeeded)
        {
            return Ok(new { Message = "User updated successfully" });
        }

        var errors = result.Errors.Select(e => e.Description).ToList();
        return BadRequest(new { Errors = errors });
    }

    [HttpGet("profile/{userId}")]
    public async Task<IActionResult> GetUserProfile(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            return BadRequest(new { Message = "User ID is required" });

        var user = await _authService.GetUserByIdAsync(userId);
        if (user == null)
            return NotFound(new { Message = "User not found" });

        return Ok(new
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Created = user.Created
        });
    }
}
