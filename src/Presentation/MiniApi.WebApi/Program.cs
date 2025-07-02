using Microsoft.EntityFrameworkCore;
using MiniApi.Persistence.Contexts;
using MiniApi.Persistence;
using FluentValidation;
using FluentValidation.AspNetCore;
using MiniApi.Application.Validations.ImageValidations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddValidatorsFromAssembly(typeof(ImageCreateDtoValidator).Assembly);
builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddEndpointsApiExplorer(); // Swagger üçün vacibdir
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<MiniApiDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
});


builder.Services.RegisterService();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "MiniApi v1");

});

// Configure the HTTP request pipeline.


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
