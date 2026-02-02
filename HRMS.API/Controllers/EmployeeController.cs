using HRMS.Application.DTOs.Employee;
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
        private static List<Employee> _employees = new List<Employee>();
        private readonly UserManager<ApplicationUser> _userManager;

        public EmployeeController(UserManager<ApplicationUser> userManager) => _userManager = userManager;

        // CREATE Employee (HR only)
        [HttpPost("create")]
        [Authorize(Roles = "HR")]
        public IActionResult Create(CreateEmployeeDto dto)
        {
            var hrUserId = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier || c.Type == JwtRegisteredClaimNames.Sub).Value;

            var employee = new Employee
            {
                Id = Guid.NewGuid(),
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Address = dto.Address,
                CreatedByUserId = hrUserId
            };

            _employees.Add(employee);
            return Ok(new { message = "Employee created successfully", employee });
        }

        // GET Employees
        [HttpGet("all")]
        public IActionResult GetAll()
        {
            var userId = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier || c.Type == JwtRegisteredClaimNames.Sub).Value;
            var roles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(r => r.Value).ToList();

            var result = roles.Contains("Admin") ? _employees : _employees.Where(e => e.CreatedByUserId == userId).ToList();
            return Ok(result);
        }

        // UPDATE Employee
        [HttpPut("update/{id}")]
        [Authorize(Roles = "HR")]
        public IActionResult Update(Guid id, UpdateEmployeeDto dto)
        {
            var userId = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier || c.Type == JwtRegisteredClaimNames.Sub).Value;
            var employee = _employees.FirstOrDefault(e => e.Id == id && e.CreatedByUserId == userId);
            if (employee == null) return NotFound("Employee not found or not owned by you");

            employee.FirstName = dto.FirstName;
            employee.LastName = dto.LastName;
            employee.PhoneNumber = dto.PhoneNumber;
            employee.Address = dto.Address;

            return Ok(new { message = "Employee updated successfully", employee });
        }

        // DELETE Employee
        [HttpDelete("delete/{id}")]
        [Authorize(Roles = "HR")]
        public IActionResult Delete(Guid id)
        {
            var userId = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier || c.Type == JwtRegisteredClaimNames.Sub).Value;
            var employee = _employees.FirstOrDefault(e => e.Id == id && e.CreatedByUserId == userId);
            if (employee == null) return NotFound("Employee not found or not owned by you");

            _employees.Remove(employee);
            return Ok("Employee deleted successfully");
        }

        // Employee entity
        public class Employee
        {
            public Guid Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public string PhoneNumber { get; set; }
            public string Address { get; set; }
            public string CreatedByUserId { get; set; } // HR who created
        }
    }
}
