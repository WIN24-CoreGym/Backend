using Business.Dtos.Instructor;
using Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class InstructorsController(IInstructorService instructorService) : ControllerBase
{
    private readonly IInstructorService _instructorService = instructorService;

    [HttpGet]
    public async Task<IActionResult> GetAllInstructors()
    {
        var result = await _instructorService.GetAllAsync();

        return result.Succeeded
            ? Ok(result.Data)
            : StatusCode(result.StatusCode, "Failed retrieving instructors.");
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetInstructorById(int id)
    {
        var result = await _instructorService.GetByIdAsync(id);

        return result.Succeeded
            ? Ok(result.Data)
            : StatusCode(result.StatusCode, "Failed retrieving instructor.");
    }

    [HttpPost]
    public async Task<IActionResult> CreateInstructor([FromBody] CreateInstructorDto dto)
    {
        var result = await _instructorService.CreateAsync(dto);

        return result.Succeeded
            ? Ok(result.Data)
            : StatusCode(result.StatusCode, "Failed creating instructor.");
    }

    [HttpPut]
    public async Task<IActionResult> UpdateInstructor([FromBody] UpdateInstructorDto dto)
    {
        var result = await _instructorService.UpdateAsync(dto);

        return result.Succeeded
            ? Ok(result.Data)
            : StatusCode(result.StatusCode, "Failed updating instructor.");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteInstructor(int id)
    {
        var result = await _instructorService.DeleteByIdAsync(id);

        return result.Succeeded
            ? Ok(result.Data)
            : StatusCode(result.StatusCode, "Failed deleting instructor.");
    }
}
