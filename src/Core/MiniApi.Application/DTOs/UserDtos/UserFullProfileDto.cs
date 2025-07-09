using MiniApi.Application.DTOs.FavouriteDtos;
using MiniApi.Application.DTOs.OrderDtos;
using MiniApi.Application.DTOs.ProductDtos;
using MiniApi.Application.DTOs.ReviewDtos;

namespace MiniApi.Application.DTOs.UserDtos;

public class UserFullProfileDto
{
    public string Id { get; set; }
    public string Email { get; set; }
    public string FullName { get; set; }
    public List<string> Roles { get; set; }
    public List<OrderGetDto> Orders { get; set; }
    public decimal OrdersTotalPrice { get; set; }
    public List<FavouriteGetDto> Favourites { get; set; }
    public List<ReviewGetDto> Reviews { get; set; }
    public List<ProductGetDto> Products { get; set; } // Seller üçün
}
