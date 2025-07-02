using MiniApi.Application.Abstracts.Repositories;
using MiniApi.Domain.Entities;
using MiniApi.Persistence.Contexts;
using MiniApi.Persistence.Repositories;

namespace MiniApi.Persistence.Repositoriesl;

public class CategoryRepository:Repository<Category>,ICategoryRepository
{
    public CategoryRepository(MiniApiDbContext context):base(context)
    {
        
    }
}
