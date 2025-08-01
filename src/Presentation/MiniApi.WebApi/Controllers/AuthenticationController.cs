﻿using System.Net;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using MiniApi.Application.Abstracts.Services;
using MiniApi.Application.DTOs.PasswordDtos;
using MiniApi.Application.DTOs.UserDtos;
using MiniApi.Application.DTOs.Users;
using MiniApi.Application.Shared;

namespace MiniApi.WebApi.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthenticationController : ControllerBase
{
    private readonly IUserService _userService;

    public AuthenticationController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("Register")]
    [ProducesResponseType(typeof(BaseResponse<TokenResponse>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResponse<string>), (int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(BaseResponse<string>), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<string>), (int)HttpStatusCode.Created)]
    public async Task<IActionResult> Register([FromBody] UserRegisterDto dto)
    {
        var result = await _userService.Register(dto);
        return StatusCode((int)result.StatusCode, result);
    }

    [HttpPost("Login")]
    [ProducesResponseType(typeof(BaseResponse<TokenResponse>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResponse<string>), (int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(BaseResponse<string>), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
    {
        var result = await _userService.Login(dto);
        return StatusCode((int)result.StatusCode, result);
    }

    [Authorize]
    [HttpGet("User-About")]
    [ProducesResponseType(typeof(BaseResponse<TokenResponse>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResponse<string>), (int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized(new BaseResponse<string>("Unauthorized", HttpStatusCode.Unauthorized));

        var result = await _userService.GetUserProfileAsync(userId);
        return StatusCode((int)result.StatusCode, result);
    }


    [Authorize]
    [HttpGet("MyFullProfile")]
    [ProducesResponseType(typeof(BaseResponse<TokenResponse>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResponse<string>), (int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(BaseResponse<string>), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> GetFullProfileByToken([FromQuery] string token)
    {
        var result = await _userService.GetFullProfileAsync(token);
        return StatusCode((int)result.StatusCode, result);
    }

    [Authorize]
    [HttpPost("Refresh-token")]
    [ProducesResponseType(typeof(BaseResponse<TokenResponse>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResponse<string>), (int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(BaseResponse<string>), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest dto)
    {
        var result = await _userService.RefreshTokenAsync(dto);
        return StatusCode((int)result.StatusCode, result);
    }



    [HttpGet("Confirm-Email")]
    [ProducesResponseType(typeof(BaseResponse<string>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResponse<string>), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
    {
        // Tokeni URL-decode etmək MÜTLƏQ vacibdir!
        token = WebUtility.UrlDecode(token);

        var result = await _userService.ConfirmEmail(userId, token);
        return StatusCode((int)result.StatusCode, result);
    }


    
    [HttpPost("ForgotPassword")]
    [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPassword([FromQuery] string email)
    {
        var result = await _userService.ForgotPassword(email);
        return StatusCode((int)result.StatusCode, result);
    }


    
    [HttpPost("ResetPassword")]
    [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        var result = await _userService.ResetPassword(dto);
        return StatusCode((int)result.StatusCode, result);
    }
}

