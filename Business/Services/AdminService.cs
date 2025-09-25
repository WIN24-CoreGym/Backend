using Business.Dtos.Admin;
using Business.Interfaces;
using Business.Models;
using Business.Models.Results;
using Data.Entities;
using Data.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Business.Services;
public class AdminService(UserManager<AppUser> userManager, IAppUserRepository appUserRepository) : IAdminService
{
    private readonly UserManager<AppUser> _userManager = userManager;
    private readonly IAppUserRepository _appUserRepository = appUserRepository;

    // CREATE
    // Create admin without setting password
    public async Task<AdminResult<AdminDto>> CreateAsync(CreateAdminDto dto)
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
                return AdminResult<AdminDto>.InternalServerError("Failed creating admin. Rolling back.");
            }

            var applyRoleResult = await _userManager.AddToRoleAsync(createdUser, "Admin");
            if (!applyRoleResult.Succeeded)
            {
                await _appUserRepository.RollbackTransactionAsync();
                return AdminResult<AdminDto>.InternalServerError("Failed adding role to admin after creation. Rolling back.");
            }

            await _appUserRepository.CommitTransactionAsync();
            var createdUserDto = new AdminDto
            {
                UserId = createdUser.Id,
                Email = createdUser.Email!,
                FirstName = createdUser.FirstName,
                LastName = createdUser.LastName,

            };

            return AdminResult<AdminDto>.Ok(createdUserDto);
        }
        catch
        {
            await _appUserRepository.RollbackTransactionAsync();
            return AdminResult<AdminDto>.InternalServerError("Unexpected error occured. Rolling back.");
        }
    }

    // READ
    public async Task<AdminResult<IEnumerable<AdminDto>>> GetAllAdminsAsync()
    {
        var admins = await _userManager.GetUsersInRoleAsync("Admin");
        var adminsDto = admins.Select(x => new AdminDto
        {
            UserId = x.Id,
            Email = x.Email!,
            FirstName = x.FirstName,
            LastName = x.LastName
        });

        return AdminResult<IEnumerable<AdminDto>>.Ok(adminsDto);
    }

    public async Task<AdminResult<AdminDto>> GetAdminByIdAsync(string userId)
    {
        var admin = await _userManager.FindByIdAsync(userId);
        if (admin == null)
            return AdminResult<AdminDto>.NotFound($"Admin with id {userId} not found.");

        var adminDto = new AdminDto
        {
            UserId = admin.Id,
            Email = admin.Email!,
            FirstName = admin.FirstName,
            LastName = admin.LastName,
        };

        return AdminResult<AdminDto>.Ok(adminDto);
    }

    public async Task<AdminResult<AdminDto>> GetAdminByEmailAsync(string email)
    {
        var admin = await _userManager.FindByEmailAsync(email);
        if (admin == null)
            return AdminResult<AdminDto>.NotFound($"Admin with email {email} not found.");

        var adminDto = new AdminDto
        {
            UserId = admin.Id,
            Email = admin.Email!,
            FirstName = admin.FirstName,
            LastName = admin.LastName,
        };

        return AdminResult<AdminDto>.Ok(adminDto);
    }


    // UPDATE
    public async Task<AdminResult<AdminDto>> UpdateAdminAsync(UpdateAdminDto dto)
    {
        var admin = await _userManager.FindByIdAsync(dto.UserId);
        if (admin == null)
            return AdminResult<AdminDto>.NotFound($"Admin with id {dto.UserId} not found.");

        admin.Email = dto.Email;
        admin.FirstName = dto.FirstName;
        admin.LastName = dto.LastName;

        var result = await _userManager.UpdateAsync(admin);
        if (!result.Succeeded)
            return AdminResult<AdminDto>.InternalServerError("Failed updating user.");

        var updatedadmin = await _userManager.FindByIdAsync(dto.UserId);
        var updatedadminDto = new AdminDto
        {
            UserId = dto.UserId,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName
        };

        return AdminResult<AdminDto>.Ok(updatedadminDto);
    }


    // DELETE
    public async Task<AdminResult<string?>> DeleteAdminByIdAsync(string userId)
    {
        var admin = await _userManager.FindByIdAsync(userId);
        if (admin == null)
            return AdminResult<string?>.NotFound($"Admin with id {userId} not found.");

        var result = await _userManager.DeleteAsync(admin);
        if (!result.Succeeded)
            return AdminResult<string?>.InternalServerError($"Failed deleting Admin with id {userId}");

        return AdminResult<string?>.Ok($"User with id {userId} deleted successfully.");
    }
}
