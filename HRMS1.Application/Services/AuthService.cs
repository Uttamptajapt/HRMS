using HRMS.Application.DTOs.Auth;
using HRMS.Application.Interfaces.Services;

namespace HRMS.Application.Services
{
    public class AuthService : IAuthService
    {
        public async Task<TokenResponseDto> RegisterAsync(RegisterRequestDto dto)
        {
            throw new NotImplementedException();
        }

        public async Task<TokenResponseDto> LoginAsync(LoginRequestDto dto)
        {
            throw new NotImplementedException();
        }

        public async Task<TokenResponseDto> RefreshTokenAsync(string refreshToken)
        {
            throw new NotImplementedException();
        }
    }
}
