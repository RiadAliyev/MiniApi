using Microsoft.AspNetCore.Identity;
using System.Net;
using System.Security.Claims;
using MiniApi.Application.Abstracts.Services;
using MiniApi.Application.DTOs.RoleDtos;
using MiniApi.Application.Shared;

namespace MiniApi.Persistence.Services;

public class RoleService:IRoleService
{
    private readonly RoleManager<IdentityRole> _rolemanager;

    public RoleService(RoleManager<IdentityRole> rolemanager)
    {
        _rolemanager = rolemanager;
    }

    public async Task<BaseResponse<string?>> CreateRole(RoleCreateDto dto)
    {

        var existingRole = await _rolemanager.FindByNameAsync(dto.Name);
        if (existingRole is not null)
        {
            return new BaseResponse<string?>("Role Already Exists", HttpStatusCode.BadRequest);
        }
        var identityRole = new IdentityRole(dto.Name);
        var result = await _rolemanager.CreateAsync(identityRole);
        if (!result.Succeeded)
        {
            var errorMessages = string.Join(", ", result.Errors.Select(e => e.Description));
            return new BaseResponse<string?>(errorMessages, HttpStatusCode.BadRequest);
        }

        foreach (var permission in dto.PermissionList.Distinct())
        {
            var claimResult = await _rolemanager.AddClaimAsync(identityRole, new Claim("Permission", permission));
            if (!claimResult.Succeeded)
            {
                var error = string.Join(", ", claimResult.Errors.Select(e => e.Description));
                return new BaseResponse<string?>($"Role created,but adding permission '{permission}' failed:{error}", HttpStatusCode.PartialContent);
            }
        }
        return new BaseResponse<string?>("Role created succesfuly", true, HttpStatusCode.Created);
    }

    public async Task<BaseResponse<string?>> UpdateRole(RoleUpdateDto dto)
    {
        var existingRole = await _rolemanager.FindByIdAsync(dto.Id);
        if (existingRole is null)
        {
            return new BaseResponse<string?>("Role not found", HttpStatusCode.NotFound);
        }

        existingRole.Name = dto.Name;
        var updateResult = await _rolemanager.UpdateAsync(existingRole);
        if (!updateResult.Succeeded)
        {
            var errorMessages = string.Join(", ", updateResult.Errors.Select(e => e.Description));
            return new BaseResponse<string?>(errorMessages, HttpStatusCode.BadRequest);
        }

        // Mövcud permission-ləri sil
        var currentClaims = await _rolemanager.GetClaimsAsync(existingRole);
        var permissionClaims = currentClaims.Where(c => c.Type == "Permission").ToList();

        foreach (var claim in permissionClaims)
        {
            await _rolemanager.RemoveClaimAsync(existingRole, claim);
        }

        // Yeni permission-ləri əlavə et
        foreach (var permission in dto.PermissionList.Distinct())
        {
            var claimResult = await _rolemanager.AddClaimAsync(existingRole, new Claim("Permission", permission));
            if (!claimResult.Succeeded)
            {
                var error = string.Join(", ", claimResult.Errors.Select(e => e.Description));
                return new BaseResponse<string?>($"Role updated, but adding permission '{permission}' failed: {error}", HttpStatusCode.PartialContent);
            }
        }

        return new BaseResponse<string?>("Role updated successfully", true, HttpStatusCode.OK);
    }

    public async Task<BaseResponse<string?>> DeleteRole(string roleName)
    {
        var role = await _rolemanager.FindByNameAsync(roleName);
        if (role is null)
            return new BaseResponse<string?>("Role not found", HttpStatusCode.NotFound);

        var result = await _rolemanager.DeleteAsync(role);
        if (!result.Succeeded)
            return new BaseResponse<string?>(string.Join(", ", result.Errors.Select(e => e.Description)), HttpStatusCode.BadRequest);

        return new BaseResponse<string?>("Role deleted successfully", true, HttpStatusCode.OK);
    }
    public List<string> GetAllRoles()
    {
        return _rolemanager.Roles.Select(r => r.Name).ToList();
    }
}
