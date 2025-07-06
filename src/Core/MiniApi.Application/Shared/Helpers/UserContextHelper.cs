using System.Security.Claims;

namespace MiniApi.Application.Shared.Helpers;

public interface IUserContextService
{
    string GetCurrentUserId(ClaimsPrincipal user);
}

public class UserContextService : IUserContextService
{
    public string GetCurrentUserId(ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
    }
}
