using HRMS.Application.Common.Responses;
using HRMS.Application.DTOs.Organization;
using HRMS.Domain.Entities;
using HRMS.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[Route("api/[controller]")]
[ApiController]
public class OrganizationController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public OrganizationController(ApplicationDbContext context)
    {
        _context = context;
    }

    // ✅ CREATE ORGANIZATION
    [HttpPost("create")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<Organization>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(CreateOrganizationDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new ApiResponse<string>
            {
                Success = false,
                Message = "User not authorized"
            });
        }

        var alreadyExists = await _context.Organizations
            .AnyAsync(o => o.CreatedByUserId == userId);

        if (alreadyExists)
        {
            return BadRequest(new ApiResponse<string>
            {
                Success = false,
                Message = "You have already created an organization."
            });
        }

        var organization = new Organization
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Address = dto.Address,
            CreatedByUserId = userId
        };

        _context.Organizations.Add(organization);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = organization.Id },
            new ApiResponse<Organization>
            {
                Success = true,
                Message = "Organization created successfully",
                Data = organization
            });
    }

    // ✅ GET ALL ORGANIZATIONS
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<Organization>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var organizations = await _context.Organizations.ToListAsync();

        return Ok(new ApiResponse<IEnumerable<Organization>>
        {
            Success = true,
            Message = "Organizations fetched successfully",
            Data = organizations
        });
    }

    // ✅ GET ORGANIZATION BY ID
    [HttpGet("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<Organization>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var organization = await _context.Organizations.FindAsync(id);

        if (organization == null)
        {
            return NotFound(new ApiResponse<string>
            {
                Success = false,
                Message = "Organization not found"
            });
        }

        return Ok(new ApiResponse<Organization>
        {
            Success = true,
            Message = "Organization fetched successfully",
            Data = organization
        });
    }

    // ✅ UPDATE ORGANIZATION
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<Organization>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, UpdateOrganizationDto dto)
    {
        var organization = await _context.Organizations.FindAsync(id);

        if (organization == null)
        {
            return NotFound(new ApiResponse<string>
            {
                Success = false,
                Message = "Organization not found"
            });
        }

        organization.Name = dto.Name;
        organization.Address = dto.Address;

        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<Organization>
        {
            Success = true,
            Message = "Organization updated successfully",
            Data = organization
        });
    }

    // ✅ DELETE ORGANIZATION
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var organization = await _context.Organizations.FindAsync(id);

        if (organization == null)
        {
            return NotFound(new ApiResponse<string>
            {
                Success = false,
                Message = "Organization not found"
            });
        }

        _context.Organizations.Remove(organization);
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<string>
        {
            Success = true,
            Message = "Organization deleted successfully"
        });
    }
}