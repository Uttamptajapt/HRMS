using HRMS.Application.DTOs.Auth;
using HRMS.Application.DTOs.HR;
using HRMS.Application.Common.Responses;
using HRMS.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class HRController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public HRController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // ✅ GET ALL HR USERS
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

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "HR users fetched successfully",
                Data = hrUsers
            });
        }

        // ✅ CREATE HR
        [HttpPost("create")]
        public async Task<IActionResult> CreateHr(RegisterRequestDto dto)
        {
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);

            if (existingUser != null)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "User already exists"
                });
            }

            var hrUser = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email
            };

            var result = await _userManager.CreateAsync(hrUser, dto.Password);

            if (!result.Succeeded)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "HR creation failed",
                    Errors = result.Errors.Select(e => e.Description).ToList()
                });
            }

            await _userManager.AddToRoleAsync(hrUser, "HR");

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "HR created successfully",
                Data = new
                {
                    hrUser.Id,
                    hrUser.Email
                }
            });
        }

        // ✅ UPDATE HR
        [HttpPut("update/{Id}")]
        public async Task<IActionResult> UpdateHR(string hrId, UpdateHrDto dto)
        {
            var hrUser = await _userManager.FindByIdAsync(hrId);

            if (hrUser == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "HR user not found"
                });
            }

            var isHR = await _userManager.IsInRoleAsync(hrUser, "HR");

            if (!isHR)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "User is not an HR"
                });
            }

            // Update Email & Username
            hrUser.Email = dto.Email;
            hrUser.UserName = dto.Email;

            var updateResult = await _userManager.UpdateAsync(hrUser);

            if (!updateResult.Succeeded)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "HR update failed",
                    Errors = updateResult.Errors.Select(e => e.Description).ToList()
                });
            }

            // Update Password (if provided)
            if (!string.IsNullOrEmpty(dto.NewPassword))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(hrUser);
                var passwordResult = await _userManager.ResetPasswordAsync(hrUser, token, dto.NewPassword);

                if (!passwordResult.Succeeded)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Password update failed",
                        Errors = passwordResult.Errors.Select(e => e.Description).ToList()
                    });
                }
            }

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "HR updated successfully",
                Data = new
                {
                    hrUser.Id,
                    hrUser.Email
                }
            });
        }

        // ✅ DELETE HR
        [HttpDelete("delete/{hId}")]
        public async Task<IActionResult> DeleteHR(string hrId)
        {
            var hrUser = await _userManager.FindByIdAsync(hrId);

            if (hrUser == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "HR user not found"
                });
            }

            var isHR = await _userManager.IsInRoleAsync(hrUser, "HR");

            if (!isHR)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "User is not an HR"
                });
            }

            var result = await _userManager.DeleteAsync(hrUser);

            if (!result.Succeeded)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "HR deletion failed",
                    Errors = result.Errors.Select(e => e.Description).ToList()
                });
            }

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "HR deleted successfully"
            });
        }
    }
}