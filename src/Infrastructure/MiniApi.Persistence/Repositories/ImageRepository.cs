using MiniApi.Application.Abstracts.Repositories;
using MiniApi.Domain.Entities;
using MiniApi.Persistence.Contexts;

namespace MiniApi.Persistence.Repositories;

public class ImageRepository:Repository<Image> , IImageRepository
{
    public ImageRepository(MiniApiDbContext context):base(context)
    {
        
    }
}
