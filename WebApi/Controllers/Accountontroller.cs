using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Data.Entities; // <-- här finns AppUser

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/account")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IPasswordHasher<AppUser> _passwordHasher;

        public AccountController(
            UserManager<AppUser> userManager,
            IPasswordHasher<AppUser> passwordHasher)
        {
            _userManager = userManager;
            _passwordHasher = passwordHasher;
        }

        // 0) Hälsokoll
        [HttpGet("ping")]
        public IActionResult Ping() => Ok(new { message = "account ok" });

        // 1) Hjälp-endpoint för att hitta id via e-post (underlättar test)
        //    GET /api/account/find-by-email?email=admin.test@example.com
        [HttpGet("find-by-email")]
        public async Task<ActionResult<object>> FindByEmail([FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest(new { message = "Email is required" });

            var user = await _userManager.FindByEmailAsync(email.Trim());
            if (user == null) return NotFound(new { message = "User not found" });

            return Ok(new
            {
                id = user.Id,
                firstName = user.FirstName,
                lastName = user.LastName,
                email = user.Email
            });
        }

        // 2) Hämta profil
        //    GET /api/account/profile/{userId}
        [HttpGet("profile/{userId}")]
        public async Task<ActionResult<MeDto>> GetProfile([FromRoute] string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            return Ok(new MeDto
            {
                Id = user.Id,
                FirstName = user.FirstName ?? "",
                LastName = user.LastName ?? "",
                Email = user.Email ?? ""
            });
        }

        // 3) Uppdatera profil (för user storyn: namn/e-post + ev. nytt lösenord)
        //    PUT /api/account/profile/{userId}
        [HttpPut("profile/{userId}")]
        public async Task<IActionResult> UpdateProfile([FromRoute] string userId, [FromBody] UpdateProfileDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            // Namn: om Name är ifyllt kan vi splitta, annars använd FirstName/LastName
            if (!string.IsNullOrWhiteSpace(dto.Name))
            {
                var (first, last) = SplitName(dto.Name);
                user.FirstName = first;
                user.LastName = last;
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(dto.FirstName))
                    user.FirstName = dto.FirstName.Trim();

                if (!string.IsNullOrWhiteSpace(dto.LastName))
                    user.LastName = dto.LastName.Trim();
            }

            // E-post (kolla dublett)
            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                var newEmail = dto.Email.Trim();
                var existing = await _userManager.FindByEmailAsync(newEmail);
                if (existing != null && existing.Id != user.Id)
                    return BadRequest(new { message = "Email already in use" });

                user.Email = newEmail;
                // Om ni vill använda e-post som username:
                user.UserName = newEmail;
            }

            // Nytt lösenord (valfritt) – hashas
            if (!string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                var pw = dto.NewPassword.Trim();
                if (pw.Length < 8)
                    return BadRequest(new { message = "Password must be at least 8 characters" });

                user.PasswordHash = _passwordHasher.HashPassword(user, pw);
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded) return BadRequest(result.Errors);

            return NoContent();
        }

        // Enkel namnsplit: "Förnamn Efternamn"
        private static (string First, string Last) SplitName(string name)
        {
            var trimmed = (name ?? "").Trim();
            if (string.IsNullOrEmpty(trimmed)) return ("", "");
            var parts = trimmed.Split(' ', 2, System.StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return (parts[0], "");
            return (parts[0], parts[1]);
        }
    }

    // ---- DTOs ----
    public class MeDto
    {
        public string Id { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
    }

    public class UpdateProfileDto
    {
        // Du kan använda antingen Name, eller FirstName/LastName – båda funkar.
        public string? Name { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string? Email { get; set; }

        public string? NewPassword { get; set; } // valfritt
    }
}
