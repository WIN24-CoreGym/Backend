using Business.Factories;
using Business.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;
[Route("api/[controller]")]
[ApiController]
public class WorkoutsController(IWorkoutService workoutService) : ControllerBase
{
    private readonly IWorkoutService _workoutService = workoutService;

    // Customer, get available workouts (upcoming & not fully booked)
    // localhost/api/Workouts/Customer/GetAvailableWorkouts
    [HttpGet("Customer/GetAvailableWorkouts")]
    public async Task<IActionResult> CustomerGetAvailableWorkouts()
    {
        var result = await _workoutService.GetAvailableWorkoutsAsync();
        // Map data to API dto
        var data = result.Data!.Select(x => WorkoutFactory.ToCustomerWorkoutsResponse(x));

        return result.Succeeded
            ? Ok(data)
            : StatusCode(500, "Failed retrieving workouts.");
    }
}
