using Business.Dtos;
using Business.Interfaces;
using Data.Entities;
using Data.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Business.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IAppUserRepository _appUserRepository;

    public AuthService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IAppUserRepository appUserRepository)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _appUserRepository = appUserRepository;
    }

    public async Task<IdentityResult> RegisterAsync(RegisterDto dto)
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

            var createResult = await _userManager.CreateAsync(user, dto.Password);
            var createdUser = await _userManager.FindByEmailAsync(dto.Email);
            if (!createResult.Succeeded || createdUser == null)
            {
                await _appUserRepository.RollbackTransactionAsync();
                return IdentityResult.Failed(new IdentityError { Code = "CreateUserFailure", Description = "Failed creating user. Rolling back." });
            }

            var applyRoleResult = await _userManager.AddToRoleAsync(createdUser, "Customer");
            if (!applyRoleResult.Succeeded)
            {
                await _appUserRepository.RollbackTransactionAsync();
                return IdentityResult.Failed(new IdentityError { Code = "ApplyRoleFailure", Description = "Failed applying role after user creation. Rolling back." });
            }

            await _appUserRepository.CommitTransactionAsync();
            return IdentityResult.Success;
        }
        catch
        {
            await _appUserRepository.RollbackTransactionAsync();
            return IdentityResult.Failed(new IdentityError { Code = "UnexpectedError", Description = "Unexpected error occurred. Rolling back." });
        }
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
