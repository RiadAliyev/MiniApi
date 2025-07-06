using MiniApi.Application.DTOs.UserDtos;
using MiniApi.Application.DTOs.Users;
using MiniApi.Application.Shared;

namespace MiniApi.Application.Abstracts.Services;

public interface IUserService
{
    Task<BaseResponse<string>> Register(UserRegisterDto dto);
    Task<BaseResponse<TokenResponse>> Login(UserLoginDto dto);
    Task<BaseResponse<TokenResponse>> RefreshTokenAsync(RefreshTokenRequest request);
}
