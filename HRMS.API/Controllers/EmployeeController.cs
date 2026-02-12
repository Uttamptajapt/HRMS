using HRMS.Domain.Entities;
using HRMS.Infrastructure.Data;
using HRMS.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

        public EmployeeController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        // GET Employees
        [HttpGet("all")]
        public IActionResult GetAll()
        {
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

            return Ok(result);
        }
        // CREATE Employee (HR only)
        [HttpPost("create")]
        [Authorize(Roles = "HR")]
        public async Task<IActionResult> Create(CreateEmployeeDto dto)
        {
            // Get logged-in HR user
            var hrUserId = User.Claims
                .First(c => c.Type == ClaimTypes.NameIdentifier || c.Type == JwtRegisteredClaimNames.Sub)
                .Value;

            // Validate Organization exists
            var organizationExists = _context.Organizations
                .Any(o => o.Id == dto.OrganizationId);

            if (!organizationExists)
                return BadRequest("Invalid OrganizationId. Organization does not exist.");

            // Create Employee entity
            var employee = new Employee
            {
                Id = Guid.NewGuid(),
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Address = dto.Address,
                OrganizationId = dto.OrganizationId,  // 🔥 REQUIRED
                CreatedByUserId = hrUserId
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync(); // 🔥 MUST

            return Ok(new { message = "Employee created successfully", employee });
        }

        // UPDATE Employee
        [HttpPut("update/{id}")]
        [Authorize(Roles = "HR")]
        public async Task<IActionResult> Update(Guid id, UpdateEmployeeDto dto)
        {
            var userId = User.Claims
                .First(c => c.Type == ClaimTypes.NameIdentifier || c.Type == JwtRegisteredClaimNames.Sub)
                .Value;

            var employee = _context.Employees
                .FirstOrDefault(e => e.Id == id && e.CreatedByUserId == userId);

            if (employee == null)
                return NotFound("Employee not found or not owned by you");

            employee.FirstName = dto.FirstName;
            employee.LastName = dto.LastName;
            employee.PhoneNumber = dto.PhoneNumber;
            employee.Address = dto.Address;

            await _context.SaveChangesAsync(); // 🔥 SAVE DB changes

            return Ok(new { message = "Employee updated successfully", employee });
        }

        // DELETE Employee
        [HttpDelete("delete/{id}")]
        [Authorize(Roles = "HR")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = User.Claims
                .First(c => c.Type == ClaimTypes.NameIdentifier || c.Type == JwtRegisteredClaimNames.Sub)
                .Value;

            var employee = _context.Employees
                .FirstOrDefault(e => e.Id == id && e.CreatedByUserId == userId);

            if (employee == null)
                return NotFound("Employee not found or not owned by you");

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            return Ok("Employee deleted successfully");
        }
    }
}
      