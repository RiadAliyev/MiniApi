using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniApi.Application.Abstracts.Services;
using MiniApi.Application.DTOs.RoleDtos;
using MiniApi.Application.Shared;
using MiniApi.Application.Shared.Helpers;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MiniApi.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    // GET: api/<RolesController>
    [Authorize(Policy = Permissions.Role.GetAllPermission)]
    [HttpGet("permissions")]
    public IActionResult GetAllPermissions()
    {
        var permissions = PermissionHelper.GetAllPermissions();
        return Ok(permissions);
    }

    [Authorize(Policy = Permissions.Role.Create)]
    [HttpPost("Create Role")]
    [ProducesResponseType(typeof(BaseResponse<string>), (int)HttpStatusCode.PartialContent)]
    [ProducesResponseType(typeof(BaseResponse<string>), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<string>), (int)HttpStatusCode.Created)]
    public async Task<IActionResult> Create(RoleCreateDto dto)
    {
        var result = await _roleService.CreateRole(dto);
        return StatusCode((int)result.StatusCode, result);
    }

    [Authorize(Policy = Permissions.Role.Update)]
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(BaseResponse<TokenResponse>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResponse<string>), (int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(BaseResponse<string>), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> Update(string id, [FromBody] RoleUpdateDto dto)
    {
        if (id != dto.Id)
            return BadRequest("ID mismatch");

        var result = await _roleService.UpdateRole(dto);
        return StatusCode((int)result.StatusCode, result);
    }

    [Authorize(Policy = Permissions.Role.Delete)]
    [HttpDelete("{roleName}")]
    [ProducesResponseType(typeof(BaseResponse<TokenResponse>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResponse<string>), (int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> Delete(string roleName)
    {
        var result = await _roleService.DeleteRole(roleName);
        return StatusCode((int)result.StatusCode, result);
    }

    [Authorize]
    [HttpGet]
    [ProducesResponseType(typeof(BaseResponse<TokenResponse>), (int)HttpStatusCode.OK)]
    public IActionResult GetAllRoles()
    {
        var roles = _roleService.GetAllRoles();
        return Ok(roles);
    }
}

