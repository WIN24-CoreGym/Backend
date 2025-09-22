using Business.Factories;
using Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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


    // Customer, get my workouts
    [HttpGet("Customer/GetMyWorkouts")]
    [Authorize]
    public async Task<IActionResult> CustomerGetMyWorkouts()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
            return Unauthorized();

        var result = await _workoutService.GetWorkoutsByCustomerIdAsync(userId);
        // Map data to API dto
        var data = result.Data!.Select(x => WorkoutFactory.ToCustomerWorkoutsResponse(x));

        return result.Succeeded
            ? Ok(data)
            : StatusCode(500, "Failed retrieving workouts.");
    }
}
