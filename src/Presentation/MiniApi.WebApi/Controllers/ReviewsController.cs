using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniApi.Application.Abstracts.Services;
using MiniApi.Application.DTOs.ReviewDtos;
using MiniApi.Application.Shared;
using MiniApi.Application.Shared.Helpers;
using MiniApi.Infrastructure.Services;
using MiniApi.Persistence.Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MiniApi.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ReviewsController : ControllerBase
{

    private readonly IReviewService _reviewService;
    private readonly IUserContextService _userContextService;
    public ReviewsController(IReviewService reviewService, IUserContextService userContextService)
    {
        _reviewService = reviewService;
        _userContextService = userContextService;
    }

    [Authorize(Policy = Permissions.Review.Create)]
    [HttpPost("{productId}/AddReview")]
    public async Task<IActionResult> AddReview(Guid productId, [FromBody] ReviewCreateDto dto)
    {
        dto.ProductId = productId;
        var userId = _userContextService.GetCurrentUserId(User);
        var response = await _reviewService.CreateAsync(dto, userId);
        return StatusCode((int)response.StatusCode, response);
    }

    [Authorize(Policy = Permissions.Review.Delete)]
    [HttpDelete("reviews/{reviewId}/DeleteReview")]
    public async Task<IActionResult> DeleteReview(Guid reviewId)
    {
        var userId = _userContextService.GetCurrentUserId(User);
        var response = await _reviewService.DeleteAsync(reviewId, userId);
        return StatusCode((int)response.StatusCode, response);
    }
}
