using MiniApi.Domain.Enums;

namespace MiniApi.Application.DTOs.Users;

public class UserRegisterDto
{
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;

    public UserRole Role { get; set; }
}
