using Microsoft.EntityFrameworkCore;
using Pedidos.Api.ExceptionHandling;
using Pedidos.Application.Interfaces;
using Pedidos.Application.Interfaces.Persistence;
using Pedidos.Application.Services;
using Pedidos.Infrastructure.Persistence;
using Pedidos.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddDbContext<PedidosDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("PedidosDatabase")));
builder.Services.AddScoped<IPedidoRepository, PedidoRepository>();
builder.Services.AddScoped<IProdutoRepository, ProdutoRepository>();
builder.Services.AddScoped<IPedidoService, PedidoService>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
