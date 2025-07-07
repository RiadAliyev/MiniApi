using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using MiniApi.Application.Abstracts.Services;
using MiniApi.Application.DTOs.OrderDtos;
using MiniApi.Application.Shared.Helpers;

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

    [Authorize]
    [HttpPost("Create")]
    public async Task<IActionResult> Create([FromBody] OrderCreateDto dto)
    {
        var buyerId = _userContextService.GetCurrentUserId(User);
        var response = await _orderService.CreateAsync(dto, buyerId);
        return StatusCode((int)response.StatusCode, response);
    }

    [Authorize(Roles = "Buyer")]
    [HttpGet("GetMyOrders")]
    public async Task<IActionResult> GetMyOrders()
    {
        var buyerId = _userContextService.GetCurrentUserId(User);
        var response = await _orderService.GetMyOrdersAsync(buyerId);
        return StatusCode((int)response.StatusCode, response);
    }

    [Authorize(Roles = "Seller")]
    [HttpGet("my-sales")]
    public async Task<IActionResult> GetMySales()
    {
        var sellerId = _userContextService.GetCurrentUserId(User);
        var response = await _orderService.GetMySalesAsync(sellerId);
        return StatusCode((int)response.StatusCode, response);
    }

    [Authorize]
    [HttpGet("profile")]
    public IActionResult Profile()
    {
        var userId = _userContextService.GetCurrentUserId(User);
        var email = _userContextService.GetCurrentUserEmail(User);
        var roles = _userContextService.GetCurrentUserRoles(User);

        // istifadə et...
        return Ok(new { userId, email, roles });
    }
}