using Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;
[Route("api/[controller]")]
[ApiController]
public class WorkoutLocationsController(IWorkoutLocationService workoutLocationService) : ControllerBase
{
    private readonly IWorkoutLocationService _workoutLocationService = workoutLocationService;

    // GET /api/workoutlocations
    [HttpGet]
    //[Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _workoutLocationService.GetAll();

        return result.Succeeded
            ? Ok(result.Data)
            : StatusCode(500, "Failed retrieving workout locations.");
    }
}
