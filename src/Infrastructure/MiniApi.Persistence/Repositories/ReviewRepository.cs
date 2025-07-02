using MiniApi.Application.Abstracts.Repositories;
using MiniApi.Domain.Entities;
using MiniApi.Persistence.Contexts;

namespace MiniApi.Persistence.Repositories;

public class ReviewRepository:Repository<Review>,IReviewRepository
{
    public ReviewRepository(MiniApiDbContext context):base(context)
    {
        
    }
}
