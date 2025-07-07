using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MiniApi.Application.Abstracts.Repositories;
using MiniApi.Application.Abstracts.Services;
using MiniApi.Application.DTOs.OrderDtos;
using MiniApi.Application.DTOs.UserDtos;
using MiniApi.Application.Shared;
using MiniApi.Domain.Entities;
using System.Net;

namespace MiniApi.Persistence.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private UserManager<AppUser> _userManager { get; }

    public OrderService(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        UserManager<AppUser> userManager)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _userManager = userManager;
    }

    public async Task<BaseResponse<UserProfileDto>> GetUserProfileAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return new BaseResponse<UserProfileDto>("User not found", HttpStatusCode.NotFound);

        var userProfile = new UserProfileDto
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            // Lazım olsa əlavə sahələr əlavə et
        };

        return new BaseResponse<UserProfileDto>("Success", userProfile, HttpStatusCode.OK);
    }
    public async Task<BaseResponse<OrderGetDto>> CreateAsync(OrderCreateDto dto, string buyerId)
    {
        var order = new Order
        {
            OrderDate = DateTime.UtcNow,
            Status = "Pending",
            BuyerId = buyerId,
            OrderProducts = new List<OrderProduct>()
        };

        foreach (var item in dto.Products)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId);
            if (product == null)
                return new BaseResponse<OrderGetDto>($"Product {item.ProductId} not found", HttpStatusCode.BadRequest);

            var totalPrice = product.Price * item.ProductCount;
            order.OrderProducts.Add(new OrderProduct
            {
                ProductId = item.ProductId,
                ProductCount = item.ProductCount,
                TotalPrice = totalPrice,
                SellerId = product.OwnerId  // OwnerId mütləq olmalıdır Product-da!
            });
        }

        await _orderRepository.AddAsync(order);
        await _orderRepository.SaveChangeAsync();

        var orderDto = MapOrderToGetDto(order);
        return new BaseResponse<OrderGetDto>("Order created", orderDto, HttpStatusCode.Created);
    }

    public async Task<BaseResponse<List<OrderGetDto>>> GetMyOrdersAsync(string buyerId)
    {
        var orders = await _orderRepository
            .GetAll(true)
            .Where(x => x.BuyerId == buyerId)
            .Include(x => x.OrderProducts)
                .ThenInclude(op => op.Product)
            .ToListAsync();

        var dtos = orders.Select(MapOrderToGetDto).ToList();
        return new BaseResponse<List<OrderGetDto>>("Success", dtos, HttpStatusCode.OK);
    }

    public async Task<BaseResponse<List<OrderGetDto>>> GetMySalesAsync(string sellerId)
    {
        var orders = await _orderRepository
            .GetAll(true)
            .Where(x => x.OrderProducts.Any(op => op.SellerId == sellerId))
            .Include(x => x.OrderProducts)
                .ThenInclude(op => op.Product)
            .ToListAsync();

        var dtos = orders.Select(order =>
        {
            var dto = MapOrderToGetDto(order);
            dto.Products = dto.Products.Where(p => p.SellerId == sellerId).ToList();
            return dto;
        }).ToList();

        return new BaseResponse<List<OrderGetDto>>("Success", dtos, HttpStatusCode.OK);
    }

    public async Task<BaseResponse<OrderGetDto>> GetByIdAsync(Guid id, string userId, string role)
    {
        var order = await _orderRepository
            .GetAll(true)
            .Include(x => x.OrderProducts)
                .ThenInclude(op => op.Product)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (order == null)
            return new BaseResponse<OrderGetDto>("Order not found", HttpStatusCode.NotFound);

        // Buyer və Seller yoxlanışı
        var isOwner = order.BuyerId == userId;
        var isSeller = order.OrderProducts.Any(op => op.SellerId == userId);

        if (!(isOwner || isSeller || role == "Admin"))
            return new BaseResponse<OrderGetDto>("Forbidden", HttpStatusCode.Forbidden);

        var dto = MapOrderToGetDto(order);
        if (isSeller)
            dto.Products = dto.Products.Where(p => p.SellerId == userId).ToList();

        return new BaseResponse<OrderGetDto>("Success", dto, HttpStatusCode.OK);
    }

    private OrderGetDto MapOrderToGetDto(Order order)
    {
        return new OrderGetDto
        {
            Id = order.Id,
            OrderDate = order.OrderDate,
            Status = order.Status,
            Products = order.OrderProducts?.Select(op => new OrderProductDetailDto
            {
                ProductId = op.ProductId,
                ProductTitle = op.Product?.Title ?? "",
                Price = op.Product?.Price ?? 0,
                ProductCount = op.ProductCount,
                TotalPrice = op.TotalPrice,
                SellerId = op.SellerId,
                SellerName = "" // Seller adı varsa, ayrıca yüklə (və ya null)
            }).ToList() ?? new List<OrderProductDetailDto>()
        };
    }
}
