using HRMS.Application.Common.Responses;
 //using HRMS.Application.DTOs.Employee; // Make sure your DTOs are included
using HRMS.Domain.Entities;
using HRMS.Infrastructure.Data;
using HRMS.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

// This is a Employee Controller --

namespace HRMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "HR,Admin")]
    public class EmployeeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<EmployeeController> _logger;

        public EmployeeController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<EmployeeController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // ✅ GET Employees
        [HttpGet("all")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("GetAll Employees API called.");

            var userId = User.Claims
                .First(c => c.Type == ClaimTypes.NameIdentifier ||
                            c.Type == JwtRegisteredClaimNames.Sub)
                .Value;

            // Get logged-in user from database
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                _logger.LogWarning("User not found for Id: {UserId}", userId);
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = "User not found."
                });
            }

            // Use user's OrganizationId to fetch employees (secure SaaS)
            if (user.OrganizationId == null)
            {
                _logger.LogWarning("Organization not found for UserId: {UserId}", userId);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Organization not found for this user."
                });
            }

            var employees = await _context.Employees
                .Where(e => e.OrganizationId == user.OrganizationId) // ✅ Fixed multi-tenant
                .ToListAsync();

            _logger.LogInformation("Employees fetched successfully for UserId: {UserId}", userId);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Employees fetched successfully",
                Data = employees
            });
        }

        // ✅ CREATE Employee (HR only)
        [HttpPost("create")]
        [Authorize(Roles = "HR")]
        public async Task<IActionResult> Create(CreateEmployeeDto dto)
        {
            _logger.LogInformation("Create Employee API called for Email: {Email}", dto.Email);

            var hrUserId = User.Claims
                .First(c => c.Type == ClaimTypes.NameIdentifier ||
                            c.Type == JwtRegisteredClaimNames.Sub)
                .Value;

            var hrUser = await _userManager.FindByIdAsync(hrUserId);

            if (hrUser == null)
            {
                _logger.LogWarning("HR user not found for Id: {UserId}", hrUserId);
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = "HR user not found."
                });
            }

            if (hrUser.OrganizationId == null)
            {
                _logger.LogWarning("Organization not found for HR {UserId}", hrUserId);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Organization not found for this HR user."
                });
            }

            // ✅ Multi-tenant safe: always use HR's OrganizationId
            var employee = new Employee
            {
                Id = Guid.NewGuid(),
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Address = dto.Address,
                OrganizationId = hrUser.OrganizationId, // ✅ Fixed
                CreatedByUserId = hrUserId
            };

            try
            {
                _context.Employees.Add(employee);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Employee created successfully with Id: {EmployeeId}", employee.Id);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Employee created successfully",
                    Data = employee
                });
            }
            catch (DbUpdateException)
            {
                _logger.LogWarning("Duplicate email attempt for Email: {Email}", dto.Email);

                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Employee with this email already exists."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while creating employee.");

                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An unexpected error occurred."
                });
            }
        }

        // ✅ UPDATE Employee
        [HttpPut("update/{id}")]
        [Authorize(Roles = "HR")]
        public async Task<IActionResult> Update(Guid id, UpdateEmployeeDto dto)
        {
            _logger.LogInformation("Update Employee API called for Id: {Id}", id);

            var userId = User.Claims
                .First(c => c.Type == ClaimTypes.NameIdentifier || c.Type == JwtRegisteredClaimNames.Sub)
                .Value;

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Id == id && e.CreatedByUserId == userId);

            if (employee == null)
            {
                _logger.LogWarning("Update failed. Employee not found for Id: {Id}", id);

                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Employee not found or not owned by you"
                });
            }

            employee.FirstName = dto.FirstName;
            employee.LastName = dto.LastName;
            employee.PhoneNumber = dto.PhoneNumber;
            employee.Address = dto.Address;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Employee updated successfully for Id: {Id}", id);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Employee updated successfully",
                Data = employee
            });
        }

        // ✅ DELETE Employee using a (delete/{id})
        [HttpDelete("delete/{id}")]
        [Authorize(Roles = "HR")]
        public async Task<IActionResult> Delete(Guid id)
        {
            _logger.LogInformation("Delete Employee API called for Id: {Id}", id);

            var userId = User.Claims
                .First(c => c.Type == ClaimTypes.NameIdentifier || c.Type == JwtRegisteredClaimNames.Sub)
                .Value;

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Id == id && e.CreatedByUserId == userId);

            if (employee == null)
            {
                _logger.LogWarning("Delete failed. Employee not found for Id: {Id}", id);

                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Employee not found or not owned by you"
                });
            }

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Employee deleted successfully for Id: {Id}", id);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Employee deleted successfully"
            });
        }
    }
}
