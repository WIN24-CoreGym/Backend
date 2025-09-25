using Business.Factories;
using Business.Interfaces;
using Business.Models;
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


    // Get workout by id
    [HttpGet("{id}")]
    public async Task<IActionResult> GetWorkoutById(int id)
    {
        var result = await _workoutService.GetWorkoutByIdAsync(id);

        if (!result.Succeeded)
            return NotFound("Workout not found.");

        var data = WorkoutFactory.ToCustomerWorkoutsResponse(result.Data!);
        return Ok(data);
    }





    //  update workout

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

    //  delete workout




    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]

    public async Task<IActionResult> DeleteWorkout(int id)
    {
        var result = await _workoutService.DeleteWorkoutAsync(id);

        if (!result.Succeeded)
            return StatusCode(500, "Failed deleting workout.");
        return NoContent();
    }



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

    [HttpGet("Admin/GetWorkoutBookingStatus")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminGetWorkoutBookingStatus()
    {
        var result = await _workoutService.GetAllWorkoutsAsync();

        if (!result.Succeeded)
            return StatusCode(500, "Failed retrieving workouts.");

        var  bookingsStatus = result.Data!.Select(workout => new
        {
            GetWorkoutById = workout.Id,
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



