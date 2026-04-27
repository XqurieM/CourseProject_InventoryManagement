using Ardalis.Result.AspNetCore;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.Features.CQRS;
using CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.InventoryHandlers;
using CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.ItemHandlers;
using CourseProject_InventoryManagement.Domain.Interfaces;
using CourseProject_InventoryManagement.Domain.Services;
using CourseProject_InventoryManagement.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddScoped<IAppDbContext>(provider =>
    (IAppDbContext)provider.GetRequiredService<AppDbContext>());
builder.Services.AddScoped<ICQRS.ICreateInventory, CreateInventoryCommandHandler>();
builder.Services.AddScoped<ICQRS.IGetInventoryById, GetInventoryByIdQueryHandler>();
builder.Services.AddScoped<ICQRS.IAddInventoryField, AddInventoryFieldCommandHandler>();
builder.Services.AddScoped<ICQRS.IAddItem, AddItemCommandHandler>();
builder.Services.AddScoped<ICQRS.IAddInventoryCustomIdRules, AddInventoryCustomIdRulesCommandHandler>();
builder.Services.AddScoped<ICQRS.IAddItemFieldValues, AddItemFieldValuesCommandHandler>();
builder.Services.AddScoped<ICustomIdGenerator, CustomIdGenerator>();
// Add global result convention for Ardalis.Result
builder.Services.AddControllers(options =>
{
    options.AddResultConvention(resultStatusMap => resultStatusMap
        .AddDefaultMap()
    );
});

var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
