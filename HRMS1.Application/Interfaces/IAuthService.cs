using HRMS.Application.DTOs.Auth;

namespace HRMS.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<TokenResponseDto> RegisterAsync(RegisterRequestDto dto);
        Task<TokenResponseDto> LoginAsync(LoginRequestDto dto);
        Task<TokenResponseDto> RefreshTokenAsync(string refreshToken);
    }
}
