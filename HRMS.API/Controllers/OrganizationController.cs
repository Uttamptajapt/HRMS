using HRMS.Application.DTOs.Organization;
using HRMS.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HRMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrganizationController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        // In-memory list for demo
        private static List<Organization> _organizations = new List<Organization>();

        public OrganizationController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateOrganizationDto dto)
        {
            // 1️⃣ Read userId from JWT
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized("User not found in token");

            var userId = userIdClaim.Value;

            // 2️⃣ Find user
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Unauthorized("User not found");

            // 3️⃣ Prevent duplicate organization
            if (_organizations.Any(o => o.AdminUserId == userId))
                return BadRequest("You have already created an organization.");

            // 4️⃣ Create org
            var org = new Organization
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Address = dto.Address,
                AdminUserId = userId
            };

            _organizations.Add(org);

            // 5️⃣ Assign Admin role
            if (!await _userManager.IsInRoleAsync(user, "Admin"))
            {
                var roleResult = await _userManager.AddToRoleAsync(user, "Admin");
                if (!roleResult.Succeeded)
                    return BadRequest("Failed to assign Admin role");
            }

            return Ok(new { message = "Organization created successfully", organization = org });
        }

        // Simple in-memory Organization entity
        public class Organization
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string Address { get; set; }
            public string AdminUserId { get; set; }
        }
    }
}
