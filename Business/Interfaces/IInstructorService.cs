using Business.Dtos.Instructor;
using Business.Models.Results;

namespace Business.Interfaces;
public interface IInstructorService
{
    Task<InstructorResult<InstructorDto>> CreateAsync(CreateInstructorDto dto);
    Task<InstructorResult<InstructorDto>> DeleteByIdAsync(int id);
    Task<InstructorResult<IEnumerable<InstructorDto>>> GetAllAsync();
    Task<InstructorResult<InstructorDto>> GetByIdAsync(int id);
    Task<InstructorResult<IEnumerable<InstructorDto>>> GetByWorkoutIdAsync(int id);
    Task<InstructorResult<InstructorDto>> UpdateAsync(UpdateInstructorDto dto);
}