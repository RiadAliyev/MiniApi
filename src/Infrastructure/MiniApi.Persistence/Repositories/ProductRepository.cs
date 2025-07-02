using MiniApi.Application.Abstracts.Repositories;
using MiniApi.Domain.Entities;
using MiniApi.Persistence.Contexts;

namespace MiniApi.Persistence.Repositories;

public class ProductRepository:Repository<Product>, IProductRepository
{
    public ProductRepository(MiniApiDbContext context):base(context)
    {
        
    }
}
