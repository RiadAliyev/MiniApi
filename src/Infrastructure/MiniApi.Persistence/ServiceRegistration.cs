using Microsoft.Extensions.DependencyInjection;
using MiniApi.Application.Abstracts.Repositories;
using MiniApi.Application.Abstracts.Services;
using MiniApi.Infrastructure.Services;
using MiniApi.Persistence.Repositories;
using MiniApi.Persistence.Repositoriesl;
using MiniApi.Persistence.Services;

namespace MiniApi.Persistence;

public static class ServiceRegistration
{
    public static void RegisterService(this IServiceCollection services)
    {
        services.AddScoped<IImageRepository,ImageRepository>();
        services.AddScoped<ICategoryRepository,CategoryRepository>();
        services.AddScoped<IFavouriteRepository, FavouriteRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        


        services.AddScoped<IImageService, ImageService>();
        services.AddScoped<IFileUpload, FileUpload>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IFavouriteService, FavouriteService>();


       
    }
}
