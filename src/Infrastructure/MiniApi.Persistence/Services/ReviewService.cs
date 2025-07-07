using System.Net.NetworkInformation;
using MiniApi.Application.Abstracts.Repositories;
using MiniApi.Application.Abstracts.Services;
using MiniApi.Application.DTOs.ReviewDtos;
using MiniApi.Application.Shared;
using MiniApi.Domain.Entities; 
using System.Net;


namespace MiniApi.Persistence.Services;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IProductRepository _productRepository;
    // Əgər review userə bağlıdırsa, UserId-ni də Review entitiyinə əlavə et (əks halda user yoxlamanı servisdə etmə)

    public ReviewService(IReviewRepository reviewRepository, IProductRepository productRepository)
    {
        _reviewRepository = reviewRepository;
        _productRepository = productRepository;
    }

    public async Task<BaseResponse<string>> CreateAsync(ReviewCreateDto dto, string userId)
    {
        var product = await _productRepository.GetByIdAsync(dto.ProductId);
        if (product == null)
            return new BaseResponse<string>("Məhsul tapılmadı", HttpStatusCode.NotFound);

        // Əgər bir istifadəçi bir məhsula bir dəfə review yazmalıdırsa, burada yoxlama əlavə et
        // var isExists = _reviewRepository.GetByFiltered(x => x.ProductId == dto.ProductId && x.UserId == userId).Any();
        // if (isExists) return new BaseResponse<string>("Bu məhsula artıq rəy yazmısınız", HttpStatusCode.BadRequest);

        var review = new Review
        {
            Id = Guid.NewGuid(),
            ProductId = dto.ProductId,
            Content = dto.Content,
            Rating = dto.Rating,
            UserId = userId
            // Əgər Reviewda UserId varsa əlavə et: UserId = userId
        };

        await _reviewRepository.AddAsync(review);
        await _reviewRepository.SaveChangeAsync();

        return new BaseResponse<string>("Rəy əlavə olundu", null, HttpStatusCode.OK);
    }

    public async Task<BaseResponse<string>> DeleteAsync(Guid reviewId, string userId)
    {
        var review = await _reviewRepository.GetByIdAsync(reviewId);
        if (review == null)
            return new BaseResponse<string>("Rəy tapılmadı", HttpStatusCode.NotFound);

        // Əgər Reviewda UserId varsa yoxla: if (review.UserId != userId) return new BaseResponse<string>("Yalnız öz rəyinizi silə bilərsiniz", HttpStatusCode.Forbidden);

        _reviewRepository.Delete(review);
        await _reviewRepository.SaveChangeAsync();

        return new BaseResponse<string>("Rəy silindi", null, HttpStatusCode.OK);
    }
}
