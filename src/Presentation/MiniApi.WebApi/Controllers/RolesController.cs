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
    public async Task<IActionResult> Create(RoleCreateDto dto)
    {
        var result = await _roleService.CreateRole(dto);
        return StatusCode((int)result.StatusCode, result);
    }

    [Authorize(Policy = Permissions.Role.Update)]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] RoleUpdateDto dto)
    {
        if (id != dto.Id)
            return BadRequest("ID mismatch");

        var result = await _roleService.UpdateRole(dto);
        return StatusCode((int)result.StatusCode, result);
    }

    [Authorize(Policy = Permissions.Role.Delete)]
    [HttpDelete("{roleName}")]
    public async Task<IActionResult> Delete(string roleName)
    {
        var result = await _roleService.DeleteRole(roleName);
        return StatusCode((int)result.StatusCode, result);
    }

    [Authorize]
    [HttpGet]
    public IActionResult GetAllRoles()
    {
        var roles = _roleService.GetAllRoles();
        return Ok(roles);
    }
}

