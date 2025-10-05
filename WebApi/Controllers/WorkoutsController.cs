using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Business.Interfaces;
using Business.Models;
using Business.Dtos.Workouts;
using Business.Factories;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkoutsController : ControllerBase
{
    private readonly IWorkoutService _workoutService;
    public WorkoutsController(IWorkoutService workoutService) => _workoutService = workoutService;

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _workoutService.GetWorkoutByIdAsync(id);

        if (!result.Succeeded || result.Data is null)
            return NotFound("Workout not found.");

        var data = WorkoutFactory.ToCustomerWorkoutsResponse(result.Data);
        return Ok(data);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWorkoutRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var response = await _workoutService.CreateAsync(request);

        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    // PUT: api/workouts/{id}
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateWorkout(int id, WorkoutModel workoutModel)
    {
        workoutModel.Id = id;

        var result = await _workoutService.UpdateWorkoutAsync(id, workoutModel);

        if (!result.Succeeded)
            return StatusCode(500, "Failed updating workout.");

        var data = WorkoutFactory.ToCustomerWorkoutsResponse(result.Data!);
        return Ok(data);
    }

    // DELETE: api/workouts/{id}
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteWorkout(int id)
    {
        var result = await _workoutService.DeleteWorkoutAsync(id);

        if (!result.Succeeded)
            return StatusCode(500, "Failed deleting workout.");
        
        return NoContent();
    }

    // GET: api/workouts/Admin/GetAllWorkouts
    [HttpGet("Admin/GetAllWorkouts")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminGetAllWorkoutsAsync()
    {
        var result = await _workoutService.GetAllWorkoutsAsync();

        if (!result.Succeeded)
            return StatusCode(500, "Failed retrieving workouts.");

        var data = result.Data!.Select(x => WorkoutFactory.ToCustomerWorkoutsResponse(x));
        return Ok(data);
    }

    // GET: api/workouts/Admin/GetWorkoutBookingStatus
    [HttpGet("Admin/GetWorkoutBookingStatus")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminGetWorkoutBookingStatus()
    {
        var result = await _workoutService.GetAllWorkoutsAsync();

        if (!result.Succeeded)
            return StatusCode(500, "Failed retrieving workouts.");

        var bookingsStatus = result.Data!.Select(workout => new
        {
            WorkoutId = workout.Id,
            Title = workout.Title,
            StartDate = workout.StartDate,
            MaxBookings = workout.MaxBookings,
            CurrentBookings = workout.WorkoutCustomers?.Count() ?? 0,
            AvailableSpots = workout.MaxBookings - (workout.WorkoutCustomers?.Count() ?? 0),
            IsFullyBooked = (workout.WorkoutCustomers?.Count() ?? 0) >= workout.MaxBookings
        });
        
        return Ok(bookingsStatus);
    }
}