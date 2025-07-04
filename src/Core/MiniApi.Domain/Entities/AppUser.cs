using Microsoft.AspNetCore.Identity;

namespace MiniApi.Domain.Entities;

public class AppUser:IdentityUser
{
    public string FullName { get; set; } = null!;
}
