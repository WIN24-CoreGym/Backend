using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Business.Interfaces;
using Business.Models;
using Business.Dtos.Workouts;
using Business.Factories;
using Microsoft.AspNetCore.Http.HttpResults;
using Business.Models.Mappers;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkoutsController : ControllerBase
{
    private readonly IWorkoutService _workoutService;
    public WorkoutsController(IWorkoutService workoutService) => _workoutService = workoutService;

    // PUBLIC ENDPOINTS --------------------------------------------------------------------------------
    // Get workout details, without customers included
    [HttpGet("Customer/{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _workoutService.GetWorkoutByIdAsync(id);

        if (!result.Succeeded || result.Data is null)
            return NotFound("Workout not found.");

        return Ok(result.Data.ToCustomerResponse());
    }

    // /api/Workouts/Customer/GetAvailableWorkouts
    [HttpGet("Customer/AvailableWorkouts")]
    public async Task<IActionResult> CustomerGetAvailableWorkouts()
    {
        var result = await _workoutService.GetAvailableWorkoutsAsync();
        // Map data to API dto
        var data = result.Data!.Select(x => x.ToCustomerResponse());

        return result.Succeeded
            ? Ok(data)
            : StatusCode(500, "Failed retrieving workouts.");
    }

    // ADMIN ONLY ENDPOINTS ----------------------------------------------------------------------------

    // POST /api/workouts/admin
    [HttpPost("Admin")]
    //[Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateWorkoutRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var response = await _workoutService.CreateAsync(request);
        if (!response.Succeeded || response.Data is null)
            return StatusCode(500, "Failed creating workout.");

        var data = response.Data.ToAdminResponse();

        return CreatedAtAction(nameof(GetById), new { id = response.Data!.Id }, response.Data);
    }

    // PUT: api/workouts/admin/{id}
    [HttpPut("Admin/{id}")]
    //[Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateWorkout(int id, UpdateWorkoutRequest req)
    {
        var result = await _workoutService.UpdateWorkoutAsync(id, req);

        if (!result.Succeeded || result.Data == null)
            return StatusCode(500, "Failed updating workout.");

        var data = result.Data.ToAdminResponse();
        return Ok(data);
    }

    // DELETE: api/workouts/admin/{id}
    [HttpDelete("Admin/{id}")]
    //[Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteWorkout(int id)
    {
        var result = await _workoutService.DeleteWorkoutAsync(id);

        if (!result.Succeeded)
            return StatusCode(500, "Failed deleting workout.");
        
        return NoContent();
    }

    // GET: api/workouts/admin
    [HttpGet("Admin")]
    //[Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminGetAllWorkoutsAsync()
    {
        var result = await _workoutService.GetAllWorkoutsAsync();

        if (!result.Succeeded)
            return StatusCode(500, "Failed retrieving workouts.");

        var data = result.Data!.Select(x => x.ToAdminResponse());
        return Ok(data);
    }

    // GET /api/workouts/admin/{id}
    [HttpGet("Admin/{id:int}")]
    public async Task<IActionResult> GetByIdForAdmin(int id, CancellationToken ct)
    {
        var result = await _workoutService.GetWorkoutByIdAsync(id);

        if (!result.Succeeded || result.Data is null)
            return NotFound("Workout not found.");

        return Ok(result.Data.ToAdminResponse());
    }

    // GET: api/workouts/Admin/GetWorkoutBookingStatus
    [HttpGet("Admin/WorkoutBookingStatus")]
    //[Authorize(Roles = "Admin")]
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