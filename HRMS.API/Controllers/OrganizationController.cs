using HRMS.Application.DTOs.Organization;
using HRMS.Domain.Entities;
using HRMS.Infrastructure.Data;
using HRMS.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HRMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrganizationController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public OrganizationController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateOrganizationDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized("User not found in token");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Unauthorized("User not found");

            //// Prevent duplicate organization
            ///
            //if (_context.Organizations.Any(o => o.CreatedByUserId == userId))
            //    return BadRequest("You have already created an organization.");

            //var org = new Organization
            //{
            //    Id = Guid.NewGuid(),
            //    Name = dto.Name,
            //    Address = dto.Address,
            //    CreatedByUserId = userId
            //};

            //_context.Organizations.Add(org);
            //await _context.SaveChangesAsync(); 

            // Assign Admin role
            if (!await _userManager.IsInRoleAsync(user, "Admin"))
            {
                var roleResult = await _userManager.AddToRoleAsync(user, "Admin");
                if (!roleResult.Succeeded)
                    return BadRequest("Failed to assign Admin role");
            }

            return Ok(new { message = "Organization created successfully",  });
        }
    }
}
