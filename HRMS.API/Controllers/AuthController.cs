using HRMS.Application.DTOs.Auth;
using HRMS.Infrastructure.Identity;
using HRMS.Infrastructure.Data;
using HRMS.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace HRMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _db;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            ApplicationDbContext db)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _db = db;
        }

        // 🔹 REGISTER (UNCHANGED)
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequestDto dto)
        {
            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok("User registered successfully");
        }

        // 🔐 LOGIN (ENHANCED, NOT REPLACED)
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return Unauthorized("Invalid credentials");

            var valid = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!valid)
                return Unauthorized("Invalid credentials");

            // ✅ AUTO-ASSIGN ADMIN ROLE IF USER HAS NONE
            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Any())
            {
                if (!await _roleManager.RoleExistsAsync("Admin"))
                    await _roleManager.CreateAsync(new IdentityRole("Admin"));

                await _userManager.AddToRoleAsync(user, "Admin");
            }

            // 🔑 ACCESS TOKEN (JWT)
            var token = await GenerateJwtToken(user);

            // 🔁 REFRESH TOKEN (NEW)
            var refreshToken = GenerateRefreshToken();

            _db.RefreshTokens.Add(new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            });

            await _db.SaveChangesAsync();

            return Ok(new
            {
                token,
                refreshToken
            });
        }

        // 🔄 REFRESH TOKEN API (NEW)
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(RefreshTokenRequestDto dto)
        {
            var storedToken = await _db.RefreshTokens
                .FirstOrDefaultAsync(x =>
                    x.Token == dto.RefreshToken &&
                    !x.IsRevoked &&
                    x.ExpiresAt > DateTime.UtcNow);

            if (storedToken == null)
                return Unauthorized("Invalid refresh token");

            var user = await _userManager.FindByIdAsync(storedToken.UserId);
            if (user == null)
                return Unauthorized();


            var newAccessToken = await GenerateJwtToken(user);

            return Ok(new { token = newAccessToken });
        }

        // 🚪 LOGOUT (OPTIONAL BUT CLEAN)
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout(RefreshTokenRequestDto dto)
        {
            var token = await _db.RefreshTokens
                .FirstOrDefaultAsync(x => x.Token == dto.RefreshToken);

            if (token != null)
            {
                token.IsRevoked = true;
                await _db.SaveChangesAsync();
            }

            return Ok("Logged out successfully");
        }

        // 🔐 JWT GENERATION (ONLY SMALL SAFETY ADDITION)
        private async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["Key"])
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    Convert.ToDouble(jwtSettings["DurationInMinutes"])
                ),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // 🔁 REFRESH TOKEN GENERATOR (NEW)
        private string GenerateRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }
    }
}
  