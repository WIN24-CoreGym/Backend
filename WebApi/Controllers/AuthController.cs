using Business.Dtos;
using Business.Interfaces;
using Business.Services;
using Data.Entities;
using Data.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly UserManager<AppUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly SignInManager<AppUser> _signInManager;

    public AuthController(IAuthService authService, UserManager<AppUser> userManager, IEmailService emailService, SignInManager<AppUser> signInManager)
    {
        _authService = authService;
        _userManager = userManager;
        _emailService = emailService;
        _signInManager = signInManager;
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

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            return Ok("If account exists, a reset email has been sent.");
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetLink = $"http://localhost:5173/reset-password?token={Uri.EscapeDataString(token)}&email={user.Email}";

        await _emailService.SendAsync(user.Email, "Password Reset",
            $"Klicka här för att återställa ditt lösenord: {resetLink}");
        return Ok("If account exists, a reset email has been sent.");
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            return BadRequest("Invalid request.");

        var decodedToken = Uri.UnescapeDataString(dto.Token);
        var result = await _userManager.ResetPasswordAsync(user, decodedToken, dto.NewPassword);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok("Password has been reset.");
    }
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return NoContent();
    }
}
