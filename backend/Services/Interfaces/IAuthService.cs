using MerxoSell.API.DTOs.Auth;

namespace MerxoSell.API.Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> RegisterAsync(RegisterRequestDto dto);
    Task<LoginResponseDto> LoginAsync(LoginRequestDto dto);
    Task<LoginResponseDto> AdminLoginAsync(LoginRequestDto dto);
    Task<LoginResponseDto> SellerLoginAsync(LoginRequestDto dto);
}
