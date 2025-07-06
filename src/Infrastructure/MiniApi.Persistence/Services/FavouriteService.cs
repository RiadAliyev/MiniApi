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
        // Məhsul mövcuddurmu?
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
            return new BaseResponse<string>("Məhsul tapılmadı", false, HttpStatusCode.NotFound);

        // Artıq favoritedirmi?
        var exists = await _favouriteRepository.GetByFiltered(x => x.ProductId == productId && x.UserId == userId).AnyAsync();
        if (exists)
            return new BaseResponse<string>("Bu məhsul artıq favoritdədir.", false, HttpStatusCode.BadRequest);

        var fav = new Favourite
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            UserId = userId
        };

        await _favouriteRepository.AddAsync(fav);
        await _favouriteRepository.SaveChangeAsync();

        return new BaseResponse<string>("Məhsul favoritedədir.", true, HttpStatusCode.Created);
    }

    public async Task<BaseResponse<string>> RemoveAsync(Guid productId, string userId)
    {
        var fav = await _favouriteRepository.GetByFiltered(x => x.ProductId == productId && x.UserId == userId).FirstOrDefaultAsync();
        if (fav == null)
            return new BaseResponse<string>("Favorit tapılmadı", false, HttpStatusCode.NotFound);

        _favouriteRepository.Delete(fav);
        await _favouriteRepository.SaveChangeAsync();

        return new BaseResponse<string>("Favoritdən silindi", true, HttpStatusCode.OK);
    }

    public async Task<BaseResponse<List<FavouriteGetDto>>> GetUserFavouritesAsync(string userId)
    {
        var list = await _favouriteRepository.GetByFiltered(x => x.UserId == userId, new Expression<Func<Favourite, object>>[] { x => x.Product })
            .Select(x => new FavouriteGetDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                ProductName = x.Product.Name
            }).ToListAsync();

        return new BaseResponse<List<FavouriteGetDto>>("Uğurlu", list, HttpStatusCode.OK);
    }
}
