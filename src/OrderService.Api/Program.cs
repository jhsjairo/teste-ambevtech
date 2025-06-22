using OrderService.Application.Interfaces;
using OrderService.Application.UseCases;
using OrderService.Infrastructure.Database;
using OrderService.Infrastructure.Repositories;
using Microsoft.OpenApi.Models;
using OrderService.Infrastructure.Cache;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Serviços do domínio
builder.Services.AddSingleton<SqlConnectionFactory>();
builder.Services.AddSingleton<IConnectionMultiplexer>(
    _ => ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")));

builder.Services.AddScoped<IOrderCache, RedisCacheService>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<GetOrdersByStatusUseCase>();
builder.Services.AddScoped<GetOrderByExternalIdUseCase>();

// Controllers e Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "OrderService API",
        Version = "v1",
        Description = "API para consulta de pedidos processados"
    });
});

var app = builder.Build();

// Ativa Swagger em todas as versões (ou só em dev, se preferir)
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "OrderService API v1");
    options.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
