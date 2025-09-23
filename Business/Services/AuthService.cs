using Business.Dtos;
using Business.Interfaces;
using Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace Business.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;

    public AuthService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<IdentityResult> RegisterAsync(RegisterDto dto)
    {
        var user = new AppUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Created = DateTime.UtcNow
        };

        return await _userManager.CreateAsync(user, dto.Password);
    }

    public async Task<IdentityResult> UpdateUserAsync(string userId, UpdateUserDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return IdentityResult.Failed(new IdentityError
            {
                Description = "User not found"
            });
        }


        if (dto.Email != user.Email)
        {
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null && existingUser.Id != user.Id)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Description = "Email is already taken by another user"
                });
            }
        }


        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.Email = dto.Email;
        user.UserName = dto.Email;


        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return updateResult;
        }


        if (!string.IsNullOrWhiteSpace(dto.NewPassword))
        {

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var passwordResult = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);

            if (!passwordResult.Succeeded)
            {
                return passwordResult;
            }
        }

        return IdentityResult.Success;
    }

    public async Task<AppUser?> GetUserByIdAsync(string userId)
    {
        return await _userManager.FindByIdAsync(userId);
    }

    public async Task<SignInResult> LoginAsync(LoginDto dto)
    {
        return await _signInManager.PasswordSignInAsync(dto.Email, dto.Password, false, false);
    }
}
