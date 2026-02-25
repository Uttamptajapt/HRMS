using HRMS.Domain.Entities;
using HRMS.Infrastructure.Data;
using HRMS.Infrastructure.Identity;
using HRMS.Application.Common.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;   // ✅ Added
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace HRMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "HR,Admin")]
    public class EmployeeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<EmployeeController> _logger;  // ✅ Added

        public EmployeeController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<EmployeeController> logger)   // ✅ Added
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;   // ✅ Added
        }

        // ✅ GET Employees
        [HttpGet("all")]
        public IActionResult GetAll()
        {
            _logger.LogInformation("GetAll Employees API called.");

            var userId = User.Claims
                .First(c => c.Type == ClaimTypes.NameIdentifier || c.Type == JwtRegisteredClaimNames.Sub)
                .Value;

            var roles = User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(r => r.Value)
                .ToList();

            var result = roles.Contains("Admin")
                ? _context.Employees.ToList()
                : _context.Employees.Where(e => e.CreatedByUserId == userId).ToList();

            _logger.LogInformation("Employees fetched successfully by UserId: {UserId}", userId);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Employees fetched successfully",
                Data = result
            });
        }

        // ✅ CREATE Employee (HR only)
        [HttpPost("create")]
        [Authorize(Roles = "HR")]
        public async Task<IActionResult> Create(CreateEmployeeDto dto)
        {
            _logger.LogInformation("Create Employee API called for Email: {Email}", dto.Email);

            var hrUserId = User.Claims
                .First(c => c.Type == ClaimTypes.NameIdentifier || c.Type == JwtRegisteredClaimNames.Sub)
                .Value;

            var organizationExists = _context.Organizations
                .Any(o => o.Id == dto.OrganizationId);

            if (!organizationExists)
            {
                _logger.LogWarning("Invalid OrganizationId {OrgId} provided by HR {UserId}", dto.OrganizationId, hrUserId);

                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid OrganizationId. Organization does not exist."
                });
            }

            var employee = new Employee
            {
                Id = Guid.NewGuid(),
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Address = dto.Address,
                OrganizationId = dto.OrganizationId,
                CreatedByUserId = hrUserId
            };

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

        // ✅ UPDATE Employee
        [HttpPut("update/{id}")]
        [Authorize(Roles = "HR")]
        public async Task<IActionResult> Update(Guid id, UpdateEmployeeDto dto)
        {
            _logger.LogInformation("Update Employee API called for Id: {Id}", id);

            var userId = User.Claims
                .First(c => c.Type == ClaimTypes.NameIdentifier || c.Type == JwtRegisteredClaimNames.Sub)
                .Value;

            var employee = _context.Employees
                .FirstOrDefault(e => e.Id == id && e.CreatedByUserId == userId);

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

        // ✅ DELETE Employee
        [HttpDelete("delete/{id}")]
        [Authorize(Roles = "HR")]
        public async Task<IActionResult> Delete(Guid id)
        {
            _logger.LogInformation("Delete Employee API called for Id: {Id}", id);

            var userId = User.Claims
                .First(c => c.Type == ClaimTypes.NameIdentifier || c.Type == JwtRegisteredClaimNames.Sub)
                .Value;

            var employee = _context.Employees
                .FirstOrDefault(e => e.Id == id && e.CreatedByUserId == userId);

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