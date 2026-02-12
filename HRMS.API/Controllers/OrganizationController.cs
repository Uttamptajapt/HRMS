using HRMS.Application.DTOs.Organization;
using HRMS.Domain.Entities;
using HRMS.Infrastructure.Data;
using HRMS.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HRMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]    
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

        // ✅ GET ALL
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var organizations = await _context.Organizations.ToListAsync();
            return Ok(organizations);
        }

        // ✅ CREATE
        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateOrganizationDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            if (_context.Organizations.Any(o => o.CreatedByUserId == userId))
                return BadRequest("You have already created an organization.");

            var org = new Organization
            {  
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Address = dto.Address,
                CreatedByUserId = userId
            };

            _context.Organizations.Add(org);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Organization created successfully",
                id = org.Id,
                data = org
            });
        }


        // ✅ UPDATE
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdateOrganizationDto dto)
        {
            var org = await _context.Organizations.FindAsync(id);
            if (org == null)
                return NotFound("Organization not found");

            org.Name = dto.Name;
            org.Address = dto.Address;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Organization updated successfully",
                data = org
            });
        }

        // ✅ DELETE
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var org = await _context.Organizations.FindAsync(id);
            if (org == null)
                return NotFound("Organization not found");

            _context.Organizations.Remove(org);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Organization deleted successfully" });
        }
    }
}
