using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MiniApi.Application.Abstracts.Repositories;
using MiniApi.Application.Abstracts.Services;
using MiniApi.Application.DTOs.OrderDtos;
using MiniApi.Application.DTOs.UserDtos;
using MiniApi.Application.Shared;
using MiniApi.Domain.Entities;
using MiniApi.Infrastructure.Services;
using System.Net;

namespace MiniApi.Persistence.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IEmailService _emailService;
    private UserManager<AppUser> _userManager { get; }

    public OrderService(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        UserManager<AppUser> userManager,
        IEmailService emailService)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _userManager = userManager;
        _emailService = emailService;
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
                SellerId = product.OwnerId
            });
        }

        await _orderRepository.AddAsync(order);
        await _orderRepository.SaveChangeAsync();

        // --- BUYER-ə EMAIL GÖNDƏR ---
        var buyer = await _userManager.FindByIdAsync(buyerId);
        if (buyer != null && !string.IsNullOrWhiteSpace(buyer.Email))
        {
            var subject = "Sifarişiniz uğurla yaradıldı!";
            var body = $@"
            Hörmətli {buyer.FullName},<br>
            Sifarişiniz qəbul olundu.<br>
            Sifariş ID: {order.Id}<br>
            Tarix: {order.OrderDate}<br>
            Məhsullar: <br>
            <ul>
                {string.Join("", order.OrderProducts.Select(op => $"<li>{op.ProductCount} x {op.ProductId} (Cəmi: {op.TotalPrice} AZN)</li>"))}
            </ul>
        ";
            await _emailService.SendEmailAsync(new[] { buyer.Email }, subject, body);
        }

        // --- HƏR BİR SELLER-ə EMAIL GÖNDƏR ---
        var sellerIds = order.OrderProducts.Select(op => op.SellerId).Distinct().ToList();
        foreach (var sellerId in sellerIds)
        {
            var seller = await _userManager.FindByIdAsync(sellerId);
            if (seller != null && !string.IsNullOrWhiteSpace(seller.Email))
            {
                // Satıcının bu sifarişdə olan məhsullarını tap
                var sellerProducts = order.OrderProducts.Where(op => op.SellerId == sellerId).ToList();

                var subject = "Yeni sifarişiniz var!";
                var body = $@"
                Hörmətli {seller.FullName},<br>
                Sizə yeni sifariş gəlib!<br>
                Sifariş ID: {order.Id}<br>
                Tarix: {order.OrderDate}<br>
                Sifariş olunan məhsullar:<br>
                <ul>
                    {string.Join("", sellerProducts.Select(op => $"<li>{op.ProductCount} x {op.ProductId} (Cəmi: {op.TotalPrice} AZN)</li>"))}
                </ul>
            ";
                await _emailService.SendEmailAsync(new[] { seller.Email }, subject, body);
            }
        }

        var orderDto = MapOrderToGetDto(order);
        return new BaseResponse<OrderGetDto>("Order created", orderDto, HttpStatusCode.Created);
    }

    public async Task<BaseResponse<MyOrdersResponseDto>> GetMyOrdersAsync(string buyerId)
    {
        var orders = await _orderRepository
            .GetAll(true)
            .Where(x => x.BuyerId == buyerId && !x.IsDeleted)
            .Include(x => x.OrderProducts)
            .ThenInclude(op => op.Product)
            .ToListAsync();

        var dtos = orders.Select(MapOrderToGetDto).ToList();
        var totalPrice = dtos.Sum(o => o.TotalPrice);

        var result = new MyOrdersResponseDto
        {
            Orders = dtos,
            TotalPrice = totalPrice
        };

        return new BaseResponse<MyOrdersResponseDto>("Success", result, HttpStatusCode.OK);
    }

    public async Task<BaseResponse<List<OrderGetDto>>> GetMySalesAsync(string sellerId)
    {
        var orders = await _orderRepository
            .GetAll(true)
            .Where(x => x.OrderProducts.Any(op => op.SellerId == sellerId) && !x.IsDeleted) // <--- filter əlavə olundu
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
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted); // <--- filter əlavə olundu

        if (order == null)
            return new BaseResponse<OrderGetDto>("Order not found", HttpStatusCode.NotFound);

        // Buyer və Seller yoxlanışı...
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
        var products = order.OrderProducts?.Select(op => new OrderProductDetailDto
        {
            ProductId = op.ProductId,
            ProductTitle = op.Product?.Title ?? "",
            Price = op.Product?.Price ?? 0,
            ProductCount = op.ProductCount,
            TotalPrice = op.TotalPrice,
            SellerId = op.SellerId,
            SellerName = "" // Seller adı varsa, ayrıca yüklə (və ya null)
        }).ToList() ?? new List<OrderProductDetailDto>();

        return new OrderGetDto
        {
            Id = order.Id,
            OrderDate = order.OrderDate,
            Status = order.Status,
            Products = products,
            TotalPrice = products.Sum(p => p.TotalPrice) // << Bunu əlavə et!
        };
    }
    public async Task<BaseResponse<bool>> DeleteAsync(Guid id, string userId)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
            return new BaseResponse<bool>("Order not found", false, HttpStatusCode.NotFound);

        

        order.IsDeleted = true; // <--- soft delete
        _orderRepository.Update(order);
        await _orderRepository.SaveChangeAsync();

        return new BaseResponse<bool>("Order deleted (soft)", true, HttpStatusCode.OK);
    }

}
