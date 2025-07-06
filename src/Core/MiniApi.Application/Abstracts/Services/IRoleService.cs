using MiniApi.Application.DTOs.RoleDtos;
using MiniApi.Application.Shared;

namespace MiniApi.Application.Abstracts.Services;

public interface IRoleService
{
    Task<BaseResponse<string?>> CreateRole(RoleCreateDto dto);
    Task<BaseResponse<string?>> UpdateRole(RoleUpdateDto dto);
}
