using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using MiniApi.Application.Abstracts.Services;
using MiniApi.Application.DTOs.UserDtos;
using MiniApi.Application.Shared;
using MiniApi.Domain.Entities;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MiniApi.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly UserManager<AppUser> _userManager; // Bütün istifadəçiləri gətirmək üçün

    public UsersController(IUserService userService, UserManager<AppUser> userManager)
    {
        _userService = userService;
        _userManager = userManager;
    }

    /// <summary>
    /// Yalnız adminlər üçün bütün istifadəçilərin siyahısı
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        // Bütün istifadəçiləri gətir
        var users = _userManager.Users
            .Select(u => new UserProfileDto
            {
                Id = u.Id,
                Email = u.Email,
                FullName = u.FullName
                // Lazım olsa əlavə property-lər
            })
            .ToList();

        return Ok(new BaseResponse<List<UserProfileDto>>("Success", users, HttpStatusCode.OK));
    }

    /// <summary>
    /// İstənilən istifadəçi üçün profil
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(string id)
    {
        var response = await _userService.GetUserProfileAsync(id);
        return StatusCode((int)response.StatusCode, response);
    }
}