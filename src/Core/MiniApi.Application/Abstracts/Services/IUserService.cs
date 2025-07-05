using MiniApi.Application.DTOs.Users;

namespace MiniApi.Application.Abstracts.Services;

public interface IUserService
{
    Task Register(UserRegisterDto dto);
}
