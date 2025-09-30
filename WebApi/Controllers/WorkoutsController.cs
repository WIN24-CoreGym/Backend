using Business.Interfaces;
using Business.Dtos.Workouts;
using Microsoft.AspNetCore.Mvc;

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

        if (!result.Succeeded)
            return Problem(title: "Error", statusCode: 500);

        if (result.Data is null)
            return NotFound("Workout not found.");

        return Ok(result.Data);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWorkoutRequest request, CancellationToken ct)
    {
        try
        {
            var created = await _workoutService.CreateAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (KeyNotFoundException ex)
        {
            return Problem(title: "NotFound", detail: ex.Message, statusCode: 404);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(title: "ValidationError", detail: ex.Message, statusCode: 400);
        }
    }
}
