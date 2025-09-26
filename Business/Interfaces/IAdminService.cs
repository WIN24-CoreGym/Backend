using Business.Dtos.Admin;
using Business.Models.Results;

namespace Business.Interfaces;
public interface IAdminService
{
    Task<AdminResult<AdminDto>> CreateAsync(CreateAdminDto dto);
    Task<AdminResult<string?>> DeleteAdminByIdAsync(string userId);
    Task<AdminResult<AdminDto>> GetAdminByEmailAsync(string email);
    Task<AdminResult<AdminDto>> GetAdminByIdAsync(string userId);
    Task<AdminResult<IEnumerable<AdminDto>>> GetAllAdminsAsync();
    Task<AdminResult<AdminDto>> UpdateAdminAsync(UpdateAdminDto dto);
}