using HRMS.Application.DTOs.Auth;
using HRMS.Application.DTOs.HR;
using HRMS.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] // 🔐 Admin only
    public class HRController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public HRController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // GET all HR users
        [HttpGet("all")]
        public async Task<IActionResult> GetAllHR()
        {
            var users = _userManager.Users.ToList();
            var hrUsers = new List<object>();

            foreach (var user in users)
            {
                if (await _userManager.IsInRoleAsync(user, "HR"))
                {
                    hrUsers.Add(new
                    {
                        user.Id,
                        user.UserName,
                        user.Email
                    });
                }
            }

            return Ok(hrUsers);
        }

        // CREATE HR
        // 🔹 ADMIN ONLY → CREATE HR
        [Authorize(Roles = "Admin")]
        [HttpPost("create-hr")]
        public async Task<IActionResult> CreateHr(RegisterRequestDto dto)
        {
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                return BadRequest("User already exists");

            var hrUser = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email
            };

            var result = await _userManager.CreateAsync(hrUser, dto.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            await _userManager.AddToRoleAsync(hrUser, "HR");
            return Ok("HR created successfully");
        }

        // DELETE HR
        [HttpDelete("delete/{hrId}")]
        public async Task<IActionResult> DeleteHR(string hrId)
        {
            var hrUser = await _userManager.FindByIdAsync(hrId);
            if (hrUser == null) return NotFound("HR user not found");

            var isHR = await _userManager.IsInRoleAsync(hrUser, "HR");
            if (!isHR) return BadRequest("User is not an HR");

            var result = await _userManager.DeleteAsync(hrUser);
            if (!result.Succeeded) return BadRequest(result.Errors);

            return Ok("HR deleted successfully");
        }
    }
}
