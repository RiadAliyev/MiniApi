using MiniApi.Application.DTOs.PasswordDtos;
using MiniApi.Application.DTOs.UserDtos;
using MiniApi.Application.DTOs.Users;
using MiniApi.Application.Shared;

namespace MiniApi.Application.Abstracts.Services;

public interface IUserService
{
    Task<BaseResponse<string>> Register(UserRegisterDto dto);
    Task<BaseResponse<TokenResponse>> Login(UserLoginDto dto);
    Task<BaseResponse<TokenResponse>> RefreshTokenAsync(RefreshTokenRequest request);
    Task<BaseResponse<string>> AddRole(UserAddRoleDto dto);
    Task<BaseResponse<string>> ConfirmEmail(string userId, string token);
    Task<BaseResponse<string>> ForgotPassword(string email);
    Task<BaseResponse<string>> ResetPassword(ResetPasswordDto dto);
    Task<BaseResponse<UserProfileDto>> GetUserProfileAsync(string userId);
    Task<BaseResponse<List<UserProfileDto>>> GetAllUsersAsync();
    Task<BaseResponse<UserProfileDto>> GetUserByIdAsync(string userId);
}


