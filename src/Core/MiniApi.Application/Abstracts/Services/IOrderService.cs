﻿using MiniApi.Application.DTOs.OrderDtos;
using MiniApi.Application.Shared;

namespace MiniApi.Application.Abstracts.Services;

public interface IOrderService
{
    Task<BaseResponse<OrderGetDto>> CreateAsync(OrderCreateDto dto, string buyerId);
    Task<BaseResponse<MyOrdersResponseDto>> GetMyOrdersAsync(string buyerId);
    Task<BaseResponse<List<OrderGetDto>>> GetMySalesAsync(string sellerId);
    Task<BaseResponse<OrderGetDto>> GetByIdAsync(Guid id, string userId, string role);
    Task<BaseResponse<bool>> DeleteAsync(Guid id, string userId);
    
}
