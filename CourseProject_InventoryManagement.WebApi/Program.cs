using Ardalis.Result.AspNetCore;
using CourseProject_InventoryManagement.Application.Abstractions.Authentication;
using CourseProject_InventoryManagement.Application.Abstractions.Authorization;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.Features.CQRS;
using CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.AuthHandlers;
using CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.GeneralHandlers;
using CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.InventoryHandlers;
using CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.ItemHandlers;
using CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.UserHandlers;
using CourseProject_InventoryManagement.Application.Features.CQRS.Queries.GeneralQueries;
using CourseProject_InventoryManagement.Domain.Interfaces;
using CourseProject_InventoryManagement.Domain.Services;
using CourseProject_InventoryManagement.Infrastructure.Authentication;
using CourseProject_InventoryManagement.Infrastructure.Authorization;
using CourseProject_InventoryManagement.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
{
    options.AddResultConvention(resultStatusMap => resultStatusMap.AddDefaultMap());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Course Project Inventory Management API", Version = "v1" });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Bearer token",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = JwtBearerDefaults.AuthenticationScheme
        }
    };

    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, securityScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            securityScheme,
            Array.Empty<string>()
        }
    });
});

builder.Services.AddHttpContextAccessor();
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddScoped<IAppDbContext>(provider =>
    (IAppDbContext)provider.GetRequiredService<AppDbContext>());

builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IAuthenticatedUserService, AuthenticatedUserService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IInventoryAuthorizationService, InventoryAuthorizationService>();

builder.Services.AddScoped<ICQRS.IRegisterUser, RegisterUserCommandHandler>();
builder.Services.AddScoped<ICQRS.ILoginUser, LoginUserCommandHandler>();
builder.Services.AddScoped<ICQRS.IRefreshAccessToken, RefreshAccessTokenCommandHandler>();
builder.Services.AddScoped<ICQRS.IRevokeRefreshToken, RevokeRefreshTokenCommandHandler>();
builder.Services.AddScoped<ICQRS.IGetCurrentUser, GetCurrentUserQueryHandler>();
builder.Services.AddScoped<ICQRS.IUpdateUserLanguage, UpdateUserLanguageCommandHandler>();
builder.Services.AddScoped<ICQRS.IUpdateUserTheme, UpdateUserThemeCommandHandler>();
builder.Services.AddScoped<ICQRS.ICreateInventory, CreateInventoryCommandHandler>();
builder.Services.AddScoped<ICQRS.IGetInventoryById, GetInventoryByIdQueryHandler>();
builder.Services.AddScoped<ICQRS.IGetLast10Inventories, GetLast10InventoriesQueryHandler>();
builder.Services.AddScoped<ICQRS.IGetPopular5Inventories, GetPopular5InventoriesQueryHandler>();
builder.Services.AddScoped<ICQRS.IAddInventoryField, AddInventoryFieldCommandHandler>();
builder.Services.AddScoped<ICQRS.IAddInventoryCustomIdRules, AddInventoryCustomIdRulesCommandHandler>();
builder.Services.AddScoped<ICQRS.IUpdateInventoryAccess, UpdateInventoryAccessCommandHandler>();
builder.Services.AddScoped<ICQRS.IAddItem, AddItemCommandHandler>();
builder.Services.AddScoped<ICQRS.IAddItemFieldValues, AddItemFieldValuesCommandHandler>();
builder.Services.AddScoped<ICQRS.IGetUsers, GetUsersQueryHandler>();
builder.Services.AddScoped<ICQRS.IGetUsersById, GetUserByIdQueryHandler>();
builder.Services.AddScoped<ICQRS.IBlockUser, BlockUserCommandHandler>();
builder.Services.AddScoped<ICQRS.IUnblockUser, UnblockUserCommandHandler>();
builder.Services.AddScoped<ICQRS.IDeleteUser, DeleteUserCommandHandler>();
builder.Services.AddScoped<ICQRS.IGrantAdminRole, GrantAdminRoleCommandHandler>();
builder.Services.AddScoped<ICQRS.IRevokeAdminRole, RevokeAdminRoleCommandHandler>();
builder.Services.AddScoped<ICQRS.IGetDashboardStatistics, GetDashboardStatisticsQueryHandler>();
builder.Services.AddScoped<ICustomIdGenerator, CustomIdGenerator>();

var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? throw new InvalidOperationException("Jwt settings are missing.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
