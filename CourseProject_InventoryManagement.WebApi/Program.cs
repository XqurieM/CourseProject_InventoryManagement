using Ardalis.Result.AspNetCore;
using CourseProject_InventoryManagement.Application.Abstractions.Authentication;
using CourseProject_InventoryManagement.Application.Abstractions.Authorization;
using CourseProject_InventoryManagement.Application.Abstractions.Localization;
using CourseProject_InventoryManagement.Application.Abstractions.Notifications;
using CourseProject_InventoryManagement.Application.Abstractions.Persistence;
using CourseProject_InventoryManagement.Application.Abstractions.Storage;
using CourseProject_InventoryManagement.Application.Features.CQRS;
using CourseProject_InventoryManagement.Application.Features.CQRS.Handlers;
using CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.AuthHandlers;
using CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.CategoryHandlers;
using CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.CommentHandlers;
using CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.FilesHandlers;
using CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.GeneralHandlers;
using CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.InventoryHandlers;
using CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.ItemHandlers;
using CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.TagHandlers;
using CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.UserHandlers;
using CourseProject_InventoryManagement.Domain.Interfaces;
using CourseProject_InventoryManagement.Domain.Services;
using CourseProject_InventoryManagement.Infrastructure.Authentication;
using CourseProject_InventoryManagement.Infrastructure.Authorization;
using CourseProject_InventoryManagement.Infrastructure.Keys;
using CourseProject_InventoryManagement.Infrastructure.Localization;
using CourseProject_InventoryManagement.Infrastructure.Persistence.Context;
using CourseProject_InventoryManagement.Infrastructure.Storage;
using CourseProject_InventoryManagement.WebApi.Hubs;
using CourseProject_InventoryManagement.WebApi.Options;
using CourseProject_InventoryManagement.WebApi.Services;
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
builder.Services.AddSignalR();
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
builder.Services.AddMemoryCache();
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
builder.Services.Configure<MicrosoftExternalLoginOptions>(builder.Configuration.GetSection(MicrosoftExternalLoginOptions.SectionName));
builder.Services.Configure<GoogleDriveSettings>(builder.Configuration.GetSection("GoogleDriveSettings"));
builder.Services.Configure<TelegramStorageSettings>(builder.Configuration.GetSection("TelegramStorageSettings"));
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
builder.Services.AddScoped<IAdministrationAuthorizationService, AdministrationAuthorizationService>();
builder.Services.AddScoped<IReferenceDataAuthorizationService, ReferenceDataAuthorizationService>();
builder.Services.AddScoped<IUserAuthorizationService, UserAuthorizationService>();
builder.Services.AddScoped<ILocalizationCacheService, LocalizationCacheService>();
builder.Services.AddScoped<ICommentNotificationService, CommentNotificationService>();

builder.Services.AddScoped<ICQRS.IRegisterUser, RegisterUserCommandHandler>();
builder.Services.AddScoped<ICQRS.ILoginUser, LoginUserCommandHandler>();
builder.Services.AddScoped<ICQRS.IRefreshAccessToken, RefreshAccessTokenCommandHandler>();
builder.Services.AddScoped<ICQRS.IRevokeRefreshToken, RevokeRefreshTokenCommandHandler>();
builder.Services.AddScoped<ICQRS.IRevokeAllRefreshTokens, RevokeAllRefreshTokensCommandHandler>();
builder.Services.AddScoped<ICQRS.IGetActiveSessions, GetActiveSessionsQueryHandler>();
builder.Services.AddScoped<ICQRS.IGetCurrentUser, GetCurrentUserQueryHandler>();
builder.Services.AddScoped<ICQRS.IUpdateUserLanguage, UpdateUserLanguageCommandHandler>();
builder.Services.AddScoped<ICQRS.IUpdateUserTheme, UpdateUserThemeCommandHandler>();
builder.Services.AddScoped<ICQRS.ICreateInventory, CreateInventoryCommandHandler>();
builder.Services.AddScoped<ICQRS.IGetInventoryComments, GetInventoryCommentsQueryHandler>();
builder.Services.AddScoped<ICQRS.IGetInventoryById, GetInventoryByIdQueryHandler>();
builder.Services.AddScoped<ICQRS.IGetLast10Inventories, GetLast10InventoriesQueryHandler>();
builder.Services.AddScoped<ICQRS.IGetPopular5Inventories, GetPopular5InventoriesQueryHandler>();
builder.Services.AddScoped<ICQRS.IGetMyEditableInventories, GetMyEditableInventoriesQueryHandler>();
builder.Services.AddScoped<ICQRS.IGetOwnInventories, GetMyOwnInventoriesQueryHandler>();
builder.Services.AddScoped<ICQRS.IGetItemsByInventoryId, GetItemsByInventoryIdQueryHandler>();
builder.Services.AddScoped<ICQRS.IGetItemById, GetItemByIdQueryHandler>();
builder.Services.AddScoped<ICQRS.IGetItemFieldValuesByInventoryId, GetItemFieldValuesByInventoryIdQueryHandler>();
builder.Services.AddScoped<ICQRS.IGetItemFieldValuesByItemId, GetItemFieldValuesByItemIdQueryHandler>();
builder.Services.AddScoped<ICQRS.IToggleItemLike, ToggleItemLikeCommandHandler>();
builder.Services.AddScoped<ICQRS.IGetInventoryFieldsByInventoryId, GetInventoryFieldsByInventoryIdQueryHandler>();
builder.Services.AddScoped<ICQRS.IGetInventoryTagsByInventoryId, GetInventoryTagsByInventoryIdQueryHandler>();
builder.Services.AddScoped<ICQRS.IGetInventoryCustomIdRulesByInventoryId, GetInventoryCustomIdRulesByInventoryIdQueryHandler>();
builder.Services.AddScoped<ICQRS.IGetInventoryAccessList, GetInventoryAccessListQueryHandler>();
builder.Services.AddScoped<ICQRS.IGetInventoryStatistics, GetInventoryStatisticsQueryHandler>();
builder.Services.AddScoped<ICQRS.ISearchUsersForAccess, SearchUsersForAccessQueryHandler>();
builder.Services.AddScoped<ICQRS.IGetAllTags, GetAllTagsQueryHandler>();
builder.Services.AddScoped<ICQRS.IGetPopularTags, GetPopularTagsQueryHandler>();
builder.Services.AddScoped<ICQRS.IGetTagById, GetTagByIdQueryHandler>();
builder.Services.AddScoped<ICQRS.ISearchTags, SearchTagsQueryHandler>();
builder.Services.AddScoped<ICQRS.IGetInventoriesByTag, GetInventoriesByTagQueryHandler>();
builder.Services.AddScoped<ICQRS.ICreateTag, CreateTagCommandHandler>();
builder.Services.AddScoped<ICQRS.IUpdateTag, UpdateTagCommandHandler>();
builder.Services.AddScoped<ICQRS.IDeleteTag, DeleteTagCommandHandler>();
builder.Services.AddScoped<ICQRS.IAddInventoryField, AddInventoryFieldCommandHandler>();
builder.Services.AddScoped<ICQRS.IDeleteInventoryField, DeleteInventoryFieldCommandHandler>();
builder.Services.AddScoped<ICQRS.IReorderInventoryFields, ReorderInventoryFieldsCommandHandler>();
builder.Services.AddScoped<ICQRS.IUpdateInventoryFields, UpdateInventoryFieldsCommandHandler>();
builder.Services.AddScoped<ICQRS.IAddInventoryCustomIdRules, AddInventoryCustomIdRulesCommandHandler>();
builder.Services.AddScoped<ICQRS.IDeleteInventoryCustomIdRule, DeleteInventoryCustomIdRuleCommandHandler>();
builder.Services.AddScoped<ICQRS.IReorderInventoryCustomIdRules, ReorderInventoryCustomIdRulesCommandHandler>();
builder.Services.AddScoped<ICQRS.IUpdateInventoryCustomIdRules, UpdateInventoryCustomIdRulesCommandHandler>();
builder.Services.AddScoped<ICQRS.IUpdateInventoryAccess, UpdateInventoryAccessCommandHandler>();
builder.Services.AddScoped<ICQRS.IUpdateInventory, UpdateInventoryCommandHandler>();
builder.Services.AddScoped<ICQRS.IUpdateInventoryTags, UpdateInventoryTagsCommandHandler>();
builder.Services.AddScoped<ICQRS.IUpdateItem, UpdateItemCommandHandler>();
builder.Services.AddScoped<ICQRS.IUpdateItemFieldValues, UpdateItemFieldValuesCommandHandler>();
builder.Services.AddScoped<ICQRS.IDeleteItem, DeleteItemCommandHandler>();
builder.Services.AddScoped<ICQRS.IDeleteComment, DeleteCommentCommandHandler>();
builder.Services.AddScoped<ICQRS.IUpdateComment, UpdateCommentCommandHandler>();
builder.Services.AddScoped<ICQRS.IDeleteInventory, DeleteInventoryCommandHandler>();
builder.Services.AddScoped<ICQRS.IAddItem, AddItemCommandHandler>();
builder.Services.AddScoped<ICQRS.ICreateNewComment, CreateNewCommentCommandHandler>();
builder.Services.AddScoped<ICQRS.ICreateCategory, CreateCategoryCommandHandler>();
builder.Services.AddScoped<ICQRS.IGetAllCategories, GetAllCategoriesQueryHandler>();
builder.Services.AddScoped<ICQRS.IGetCategoryById, GetCategoryByIdQueryHandler>();
builder.Services.AddScoped<ICQRS.IDeleteCategory, DeleteCategoryCommandHandler>();
builder.Services.AddScoped<ICQRS.IUpdateCategory, UpdateCategoryCommandHandler>();
builder.Services.AddScoped<ICQRS.IAddItemFieldValues, AddItemFieldValuesCommandHandler>();
builder.Services.AddScoped<ICQRS.IGetUsers, GetUsersQueryHandler>();
builder.Services.AddScoped<ICQRS.IGetUsersById, GetUserByIdQueryHandler>();
builder.Services.AddScoped<ICQRS.IBlockUser, BlockUserCommandHandler>();
builder.Services.AddScoped<ICQRS.IUnblockUser, UnblockUserCommandHandler>();
builder.Services.AddScoped<ICQRS.IDeleteUser, DeleteUserCommandHandler>();
builder.Services.AddScoped<ICQRS.IGrantAdminRole, GrantAdminRoleCommandHandler>();
builder.Services.AddScoped<ICQRS.IRevokeAdminRole, RevokeAdminRoleCommandHandler>();
builder.Services.AddScoped<ICQRS.IGetDashboardStatistics, GetDashboardStatisticsQueryHandler>();
builder.Services.AddScoped<ICQRS.IGlobalSearch, GlobalSearchQueryHandler>();
builder.Services.AddScoped<ICQRS.IGetLocalizationResources, GetLocalizationResourcesQueryHandler>();
builder.Services.AddScoped<ICQRS.IGetLocalizationResourcesAdminList, GetLocalizationResourcesAdminListQueryHandler>();
builder.Services.AddScoped<ICQRS.IUpsertLocalizationResource, UpsertLocalizationResourceCommandHandler>();
builder.Services.AddScoped<ICQRS.IBulkUpsertLocalizationResources, BulkUpsertLocalizationResourcesCommandHandler>();
builder.Services.AddScoped<ICQRS.IDeleteLocalizationResource, DeleteLocalizationResourceCommandHandler>();
builder.Services.AddScoped<ICQRS.IUploadFile, UploadFileCommandHandler>();
builder.Services.AddScoped<ICustomIdGenerator, CustomIdGenerator>();

builder.Services.AddHttpClient<TelegramStorageService>();
builder.Services.AddScoped<ITelegramStorageProxy, TelegramStorageService>();


// builder.Services.AddScoped<IStorageService, GoogleDriveService>();
builder.Services.AddScoped<IStorageService, TelegramStorageService>();

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

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowUIPort",
        policy =>
        {
            policy.WithOrigins("https://localhost:7214", "http://localhost:5214") 
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials(); 
        });
});


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseRouting();
app.UseCors("AllowUIPort");
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<CommentHub>("/commentHub");
app.Run();
