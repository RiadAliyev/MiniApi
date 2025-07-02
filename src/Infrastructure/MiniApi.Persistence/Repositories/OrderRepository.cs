using MiniApi.Application.Abstracts.Repositories;
using MiniApi.Domain.Entities;
using MiniApi.Persistence.Contexts;

namespace MiniApi.Persistence.Repositories;

public class OrderRepository:Repository<Order>,IOrderRepository
{
    public OrderRepository(MiniApiDbContext context):base(context)
    {
        
    }
}
