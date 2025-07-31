using Microsoft.EntityFrameworkCore;
using MiniApi.Persistence.Contexts;
using MiniApi.Persistence;
using FluentValidation;
using FluentValidation.AspNetCore;
using MiniApi.Application.Validations.ImageValidations;
using MiniApi.Application.Abstracts.Repositories;
using MiniApi.Persistence.Repositories;
using MiniApi.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Writers;
using MiniApi.Application.Shared.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Diagnostics;
using MiniApi.Application.Shared.Helpers;
using MiniApi.WebApi.Middlewares;
using MiniApi.Application.Abstracts.Services;

var builder = WebApplication.CreateBuilder(args);


builder.WebHost.UseWebRoot("wwwroot");
// Add services to the container.
builder.Services.AddValidatorsFromAssembly(typeof(ImageCreateDtoValidator).Assembly);
builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();


builder.Services.AddEndpointsApiExplorer(); // Swagger üçün vacibdir
builder.Services.AddSwaggerGen(options => {
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "AzBina API",
        Version = "v1"
    });

    // JWT üçün təhlükəsizlik sxemini əlavə et
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT token daxil edin. Format: Bearer {token}"
    });

    // Təhlükəsizlik tələbi əlavə olunur
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


builder.Services.AddDbContext<MiniApiDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
});


builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 8;
    options.User.RequireUniqueEmail = true;
    //options.Lockout.AllowedForNewUsers = true;
    options.Lockout.MaxFailedAccessAttempts = 3;

}).AddEntityFrameworkStores<MiniApiDbContext>()
  .AddDefaultTokenProviders();

builder.Services.Configure<DataProtectionTokenProviderOptions>(opt =>
{
    opt.TokenLifespan = TimeSpan.FromMinutes(15); // və ya istədiyin vaxt
});//burada yazilan 15 deq dir yeni 15 deq sonra token yararsiz oalcaq

builder.Services.Configure<JWTSettings>(builder.Configuration.GetSection("JWTSettings"));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JWTSettings>();


builder.Services.AddAuthorization(options =>
{
    foreach (var permission in PermissionHelper.GetAllPermissionList())
    {
        options.AddPolicy(permission, policy =>
        {
            policy.RequireClaim("Permission", permission);
        });
    }
});


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
    };

    // 🔥 BURADA BLACKLIST CHECK EDİLİR
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var redisService = context.HttpContext.RequestServices.GetRequiredService<IRedisService>();
            var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            var isBlacklisted = await redisService.GetAsync<bool>($"blacklist:{token}");
            if (isBlacklisted)
            {
                context.Fail("Token is blacklisted");
            }
        }
    };
});


builder.Services.AddStackExchangeRedisCache(options =>
{
    options.ConfigurationOptions = new StackExchange.Redis.ConfigurationOptions
    {
        EndPoints = { "main-lark-29389.upstash.io:6379" },
        Password = "AXLNAAIjcDE2MDdmNzk4YmFhMWE0ZjI0ODY5ODgyNzI1ZDNjOGMyYnAxMA",
        Ssl = true,
        AbortOnConnectFail = false // bele yazanda isleyir amma bunu appsetting.json da yazanda islemir
    };
    options.InstanceName = "ECommerceApp_";
}); //redis 2 variant
//builder.Services.AddStackExchangeRedisCache(options =>
//{
//    options.Configuration = builder.Configuration.GetConnectionString("Redis");
//    options.InstanceName = "ECommerceApp_";
//});  // Redis ucun

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.RegisterService();


var app = builder.Build();


app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "MiniApi v1");

});

//Configure the HTTP request pipeline.


app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
