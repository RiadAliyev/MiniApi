using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using MiniApi.Application.Abstracts.Services;
using MiniApi.Application.DTOs.OrderDtos;
using MiniApi.Application.Shared.Helpers;
using MiniApi.Application.Shared;
using System.Net;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MiniApi.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IUserContextService _userContextService;

    public OrdersController(IOrderService orderService, IUserContextService userContextService)
    {
        _orderService = orderService;
        _userContextService = userContextService;
    }

    [Authorize(Policy = Permissions.Order.Create)]
    [HttpPost("Create")]
    [ProducesResponseType(typeof(BaseResponse<string>), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(BaseResponse<string>), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> Create([FromBody] OrderCreateDto dto)
    {
        var buyerId = _userContextService.GetCurrentUserId(User);
        var response = await _orderService.CreateAsync(dto, buyerId);
        return StatusCode((int)response.StatusCode, response);
    }

    [Authorize(Policy = Permissions.Order.GetMy)]
    [HttpGet("GetMyOrders")]
    [ProducesResponseType(typeof(BaseResponse<TokenResponse>), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetMyOrders()
    {
        var buyerId = _userContextService.GetCurrentUserId(User);
        var response = await _orderService.GetMyOrdersAsync(buyerId);
        return StatusCode((int)response.StatusCode, response);
    }

    [Authorize(Policy = Permissions.Order.GetMySales)]
    [HttpGet("my-sales")]
    [ProducesResponseType(typeof(BaseResponse<TokenResponse>), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetMySales()
    {
        var sellerId = _userContextService.GetCurrentUserId(User);
        var response = await _orderService.GetMySalesAsync(sellerId);
        return StatusCode((int)response.StatusCode, response);
    }

    [Authorize]
    [HttpGet("profile")]
    [ProducesResponseType(typeof(BaseResponse<TokenResponse>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResponse<string>), (int)HttpStatusCode.NotFound)]

    public IActionResult Profile()
    {
        var userId = _userContextService.GetCurrentUserId(User);
        var email = _userContextService.GetCurrentUserEmail(User);
        var roles = _userContextService.GetCurrentUserRoles(User);

        // istifadə et...
        return Ok(new { userId, email, roles });
    }


    [Authorize(Policy = Permissions.Order.Delete)] 
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(BaseResponse<TokenResponse>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResponse<string>), (int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = _userContextService.GetCurrentUserId(User);
        var response = await _orderService.DeleteAsync(id, userId);
        return StatusCode((int)response.StatusCode, response);
    }

}