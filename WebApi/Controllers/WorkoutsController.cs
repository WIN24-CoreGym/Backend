using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Business.Interfaces;
using Business.Models;
using Business.Dtos.Workouts;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkoutsController(IWorkoutService workoutService) : ControllerBase
{
    private readonly IWorkoutService _workoutService = workoutService;

    // GET: api/workouts
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _workoutService.GetAllWorkoutsAsync();

        if (result?.Data is null)
            return NotFound();

        return Ok(result.Data);
    }

    // GET: api/workouts/available
    [HttpGet("available")]
    public async Task<IActionResult> GetAvailable()
    {
        var result = await _workoutService.GetAvailableWorkoutsAsync();

        if (result?.Data is null)
            return NotFound();

        return Ok(result.Data);
    }

    // GET: api/workouts/by-customer/{userId}
    [HttpGet("by-customer/{userId}")]
    public async Task<IActionResult> GetByCustomer(string userId)
    {
        var result = await _workoutService.GetWorkoutsByCustomerIdAsync(userId);

        if (result?.Data is null)
            return NotFound();

        return Ok(result.Data);
    }

    // GET: api/workouts/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _workoutService.GetWorkoutByIdAsync(id);

        if (result?.Data is null)
            return NotFound();

        return Ok(result.Data);
    }

    // POST: api/workouts
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWorkoutRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);


        var response = await _workoutService.CreateAsync(request);


        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpPost("workouts")]
    public async Task<IActionResult> CreateWorkout([FromBody] WorkoutCreateRequest req)
    {

        if (req.Date < DateOnly.FromDateTime(DateTime.UtcNow.Date))
            return Problem(detail: "Date must be today or later.", statusCode: 400, title: "ValidationError");

        var result = await _workoutService.CreateAsync(req);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

}
