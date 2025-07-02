using MiniApi.Application.Abstracts.Repositories;
using MiniApi.Domain.Entities;
using MiniApi.Persistence.Contexts;

namespace MiniApi.Persistence.Repositories;

public class FavouriteRepository:Repository<Favourite>,IFavouriteRepository
{
    public FavouriteRepository(MiniApiDbContext context):base(context)
    {
        
    }
}
