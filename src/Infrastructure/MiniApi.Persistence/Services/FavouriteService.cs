using Microsoft.EntityFrameworkCore;
using MiniApi.Application.Abstracts.Repositories;
using MiniApi.Application.Abstracts.Services;
using MiniApi.Application.DTOs.FavouriteDtos;
using MiniApi.Application.Shared;
using MiniApi.Domain.Entities;
using System.Linq.Expressions;
using System.Net;

namespace MiniApi.Persistence.Services;

public class FavouriteService : IFavouriteService
{
    private readonly IFavouriteRepository _favouriteRepository;
    private readonly IRepository<Product> _productRepository;

    public FavouriteService(IFavouriteRepository favouriteRepository, IRepository<Product> productRepository)
    {
        _favouriteRepository = favouriteRepository;
        _productRepository = productRepository;
    }

    public async Task<BaseResponse<string>> AddAsync(Guid productId, string userId)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
            return new BaseResponse<string>("Məhsul tapılmadı", false, HttpStatusCode.NotFound);

        var fav = await _favouriteRepository
            .GetByFiltered(x => x.ProductId == productId && x.UserId == userId)
            .FirstOrDefaultAsync();

        if (fav != null)
        {
            if (!fav.IsDeleted)
                return new BaseResponse<string>("Bu məhsul artıq favoritdədir.", false, HttpStatusCode.BadRequest);

            fav.IsDeleted = false; // <--- bərpa et
            _favouriteRepository.Update(fav);
            await _favouriteRepository.SaveChangeAsync();
            return new BaseResponse<string>("Məhsul favoritə əlavə olundu (bərpa olundu).", true, HttpStatusCode.OK);
        }

        fav = new Favourite
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            UserId = userId,
            IsDeleted = false
        };

        await _favouriteRepository.AddAsync(fav);
        await _favouriteRepository.SaveChangeAsync();

        return new BaseResponse<string>("Məhsul favoritedədir.", true, HttpStatusCode.Created);
    }

    public async Task<BaseResponse<string>> RemoveAsync(Guid productId, string userId)
    {
        var fav = await _favouriteRepository.GetByFiltered(
            x => x.ProductId == productId && x.UserId == userId && !x.IsDeleted)
            .FirstOrDefaultAsync();
        if (fav == null)
            return new BaseResponse<string>("Favorit tapılmadı", false, HttpStatusCode.NotFound);

        fav.IsDeleted = true; // <--- soft delete
        _favouriteRepository.Update(fav); // <--- update et!
        await _favouriteRepository.SaveChangeAsync();

        return new BaseResponse<string>("Favoritdən silindi", true, HttpStatusCode.OK);
    }

    public async Task<BaseResponse<List<FavouriteGetDto>>> GetUserFavouritesAsync(string userId)
    {
        var list = await _favouriteRepository
            .GetByFiltered(
                x => x.UserId == userId && !x.IsDeleted, // <--- filter əlavə olundu
                new Expression<Func<Favourite, object>>[] { x => x.Product }
            )
            .Select(x => new FavouriteGetDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
            })
            .ToListAsync();
        return new BaseResponse<List<FavouriteGetDto>>("Uğurlu", list, HttpStatusCode.OK);
    }
}
