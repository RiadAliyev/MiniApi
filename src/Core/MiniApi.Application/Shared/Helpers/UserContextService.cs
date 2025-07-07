using System.Security.Claims;

namespace MiniApi.Application.Shared.Helpers;

public interface IUserContextService
{
    /// <summary>
    /// Verilmiş ClaimsPrincipal obyektindən cari istifadəçinin ID-sini qaytarır.
    /// </summary>
    string? GetCurrentUserId(ClaimsPrincipal user);

    /// <summary>
    /// Verilmiş ClaimsPrincipal obyektindən istifadəçinin e-mail ünvanını qaytarır.
    /// </summary>
    string? GetCurrentUserEmail(ClaimsPrincipal user);

    /// <summary>
    /// Verilmiş ClaimsPrincipal obyektindən istifadəçinin rol(lar)ını qaytarır.
    /// </summary>
    List<string> GetCurrentUserRoles(ClaimsPrincipal user);
}

public class UserContextService : IUserContextService
{
    public string? GetCurrentUserId(ClaimsPrincipal user)
    {
        return user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    public string? GetCurrentUserEmail(ClaimsPrincipal user)
    {
        return user?.FindFirst(ClaimTypes.Email)?.Value;
    }

    public List<string> GetCurrentUserRoles(ClaimsPrincipal user)
    {
        return user?.FindAll(ClaimTypes.Role).Select(x => x.Value).ToList() ?? new List<string>();
    }
}
