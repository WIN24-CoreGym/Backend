using Business.Dtos;
using Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace Business.Interfaces;
public interface IAuthService
{
    Task<AppUser?> GetUserByIdAsync(string userId);
    Task<SignInResult> LoginAsync(LoginDto dto);
    Task<IdentityResult> RegisterAsync(RegisterDto dto);
    Task<IdentityResult> UpdateUserAsync(string userId, UpdateUserDto dto);
}