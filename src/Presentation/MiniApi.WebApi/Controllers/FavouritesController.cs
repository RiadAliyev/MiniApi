using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MiniApi.Application.Abstracts.Services;
using MiniApi.Application.Shared;
using MiniApi.Application.Shared.Helpers;

namespace MiniApi.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FavouritesController : ControllerBase
{
    private readonly IFavouriteService _favouriteService;
    private readonly IUserContextService _userContextService;

    public FavouritesController(IFavouriteService favouriteService, IUserContextService userContextService)
    {
        _favouriteService = favouriteService;
        _userContextService= userContextService;
    }

    [Authorize(Policy = Permissions.Product.AddProductFavourite)]
    [HttpPost("{productId}/Add-favourite")]
    public async Task<IActionResult> AddFavourite(Guid productId)
    {
        var userId = _userContextService.GetCurrentUserId(User);
        var response = await _favouriteService.AddAsync(productId, userId);
        return StatusCode((int)response.StatusCode, response);
    }

    [Authorize]
    [HttpDelete("{productId}/Delete-favourite")]
    public async Task<IActionResult> RemoveFavourite(Guid productId)
    {
        var userId = _userContextService.GetCurrentUserId(User);
        var response = await _favouriteService.RemoveAsync(productId, userId);
        return StatusCode((int)response.StatusCode, response);
    }

    [Authorize]
    [HttpGet("GetMyFavourites")]
    public async Task<IActionResult> GetMyFavourites()
    {
        var userId = _userContextService.GetCurrentUserId(User);
        var response = await _favouriteService.GetUserFavouritesAsync(userId);
        return StatusCode((int)response.StatusCode, response);
    }

}
