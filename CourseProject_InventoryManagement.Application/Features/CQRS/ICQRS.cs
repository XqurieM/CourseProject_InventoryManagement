using Ardalis.Result;
using CourseProject_InventoryManagement.Application.DTOs;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.AuthCommands;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.CategoryCommands;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.CommentCommands;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.GeneralCommands;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.InventoryCommands;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.ItemCommands;
using CourseProject_InventoryManagement.Application.Features.CQRS.Commands.UserCommands;
using CourseProject_InventoryManagement.Application.Features.CQRS.Queries.AuthQueries;
using CourseProject_InventoryManagement.Application.Features.CQRS.Queries.GeneralQueries;
using CourseProject_InventoryManagement.Application.Features.CQRS.Queries.InventoryQueries;
using CourseProject_InventoryManagement.Application.Features.CQRS.Queries.UserQueries;
using CourseProject_InventoryManagement.Application.Features.CQRS.Results.GeneralResults;
using CourseProject_InventoryManagement.Application.Features.CQRS.Results.InventoryResults;
using CourseProject_InventoryManagement.Application.Features.CQRS.Results.ItemResults;

namespace CourseProject_InventoryManagement.Application.Features.CQRS
{
    public interface ICQRS
    {
        #region AuthInterfaces
        public interface IRegisterUser
        {
            Task<Result<AuthTokenDto>> RegisterUser(RegisterUserCommand command, CancellationToken cancellationToken = default);
        }

        public interface ILoginUser
        {
            Task<Result<AuthTokenDto>> LoginUser(LoginUserCommand command, CancellationToken cancellationToken = default, bool isExternalLogin = false);
        }

        public interface IRefreshAccessToken
        {
            Task<Result<AuthTokenDto>> RefreshAccessToken(RefreshAccessTokenCommand command, CancellationToken cancellationToken = default);
        }

        public interface IRevokeRefreshToken
        {
            Task<Result> RevokeRefreshToken(RevokeRefreshTokenCommand command, CancellationToken cancellationToken = default);
        }

        public interface IGetCurrentUser
        {
            Task<Result<UserDto>> GetCurrentUser(GetCurrentUserQuery query, CancellationToken cancellationToken = default);
        }

        public interface IUpdateUserLanguage
        {
            Task<Result<UserDto>> UpdateUserLanguage(UpdateUserLanguageCommand command, CancellationToken cancellationToken = default);
        }

        public interface IUpdateUserTheme
        {
            Task<Result<UserDto>> UpdateUserTheme(UpdateUserThemeCommand command, CancellationToken cancellationToken = default);
        }
        #endregion

        #region InventoryInterfaces
        public interface ICreateInventory
        {
            Task<Result<Guid>> CreateInventory(CreateInventoryCommand command, CancellationToken cancellationToken = default);
        }

        public interface IUpdateInventory
        {
            Task<Result<Guid>> UpdateInventory(UpdateInventoryCommand command, CancellationToken cancellationToken = default);
        }

        public interface IDeleteInventory
        {
            Task<Result<Guid>> DeleteInventory(DeleteInventoryCommand command, CancellationToken cancellationToken = default);
        }

        public interface IGetInventoryById
        {
            Task<Result<InventoryDto>> GetInventoryById(GetInventoryByIdQuery query, CancellationToken cancellationToken = default);
        }

        public interface IGetPopular5Inventories
        {
            Task<Result<List<GetInventoriesWithJoinInfosResult>>> GetPopular5Inventories(CancellationToken cancellationToken = default);
        }

        public interface IGetLast10Inventories
        {
            Task<Result<List<GetInventoriesWithJoinInfosResult>>> GetLast10Inventories(CancellationToken cancellationToken = default);
        }

        public interface IGetMyEditableInventories
        {
            Task<Result<List<GetProfileInventoriesResult>>> GetMyEditableInventories(Guid UserId, CancellationToken cancellationToken = default);
        }

        public interface IGetOwnInventories
        {
            Task<Result<List<GetProfileInventoriesResult>>> GetOwnInventories(Guid UserId, CancellationToken cancellationToken = default);
        }
        public interface IGetInventoryFieldsByInventoryId
        {
            Task<Result<List<GetInventoryFieldsByInventoryIdResult>>> GetInventoryFieldsByInventoryId(Guid inventoryId, CancellationToken cancellationToken = default);
        }
        public interface IGetInventoryAccessList
        {
            Task<Result<List<InventoryAccessListDto>>> GetInventoryAccessList(Guid inventoryId, CancellationToken cancellationToken = default);
        }
        public interface IAddInventoryField
        {
            Task<Result<Guid>> AddInventoryField(AddInventoryFieldCommand command, CancellationToken cancellationToken = default);
        }

        public interface IAddInventoryCustomIdRules
        {
            Task<Result<Guid>> AddInventoryCustomIdRules(AddInventoryCustomIdRulesCommand command, CancellationToken cancellationToken = default);
        }

        public interface IUpdateInventoryAccess
        {
            Task<Result<Guid>> UpdateInventoryAccess(UpdateInventoryAccessCommand command, CancellationToken cancellationToken = default);
        }
        #endregion

        #region ItemsInterfaces
        public interface IAddItem
        {
            Task<Result<List<Guid>>> AddItem(AddItemCommand command, CancellationToken cancellationToken = default);
        }

        public interface IAddItemFieldValues
        {
            Task<Result<Guid>> AddItemFieldValues(AddItemFieldValuesCommand command, CancellationToken cancellationToken = default);
        }
        public interface IUpdateItem
        {
            Task<Result<Guid>> UpdateItem(UpdateItemCommand command, CancellationToken cancellationToken = default);
        }
        public interface IDeleteItem
        {
            Task<Result<Guid>> DeleteItem(DeleteItemCommand command, CancellationToken cancellationToken = default);
        }
        public interface IGetItemsByInventoryId
        {
            Task<Result<List<ItemDto>>> GetItemsByInventoryId(Guid inventoryId, CancellationToken cancellationToken = default);
        }

        public interface IGetItemById
        {
            Task<Result<ItemDto>> GetItemById(Guid itemId, CancellationToken cancellationToken = default);
        }
        public interface IGetItemFieldValuesByItemId
        {
            Task<Result<List<ItemFieldValuesResult>>> GetItemFieldValuesByItemId(Guid itemId, CancellationToken cancellationToken = default);
        }
        public interface IGetItemFieldValuesByInventoryId
        {
            Task<Result<List<ItemFieldValuesResult>>> GetItemFieldValuesByInventoryId(Guid inventoryId, CancellationToken cancellationToken = default);
        }
        #endregion

        #region UserManagementInterfaces
        public interface IGetUsers
        {
            Task<Result<List<UserDto>>> GetUsers(GetUsersQuery query, CancellationToken cancellationToken = default);
        }

        public interface IGetUsersById
        {
            Task<Result<UserDto>> GetUsersById(GetUserByIdQuery query, CancellationToken cancellationToken = default);
        }

        public interface IBlockUser
        {
            Task<Result<Guid>> BlockUser(BlockUserCommand command, CancellationToken cancellationToken = default);
        }

        public interface IUnblockUser
        {
            Task<Result<Guid>> UnblockUser(UnblockUserCommand command, CancellationToken cancellationToken = default);
        }

        public interface IDeleteUser
        {
            Task<Result<Guid>> DeleteUser(DeleteUserCommand command, CancellationToken cancellationToken = default);
        }

        public interface IGrantAdminRole
        {
            Task<Result<Guid>> GrantAdminRole(GrantAdminRoleCommand command, CancellationToken cancellationToken = default);
        }

        public interface IRevokeAdminRole
        {
            Task<Result<Guid>> RevokeAdminRole(RevokeAdminRoleCommand command, CancellationToken cancellationToken = default);
        }
        #endregion

        #region GeneralInterfaces
        public interface IGetDashboardStatistics
        {
            Task<Result<GetDashboardStatisticsResult>> GetDashboardStatistics(GetDashboardStatisticsQuery query, CancellationToken cancellationToken = default);
        }

        public interface IGetLocalizationResources
        {
            Task<Result<GetLocalizationResourcesResult>> GetLocalizationResources(GetLocalizationResourcesQuery query, CancellationToken cancellationToken = default);
        }

        public interface IGetLocalizationResourcesAdminList
        {
            Task<Result<List<LocalizationResourceAdminResult>>> GetLocalizationResourcesAdminList(GetLocalizationResourcesAdminListQuery query, CancellationToken cancellationToken = default);
        }

        public interface IUpsertLocalizationResource
        {
            Task<Result<Guid>> UpsertLocalizationResource(UpsertLocalizationResourceCommand command, CancellationToken cancellationToken = default);
        }

        public interface IBulkUpsertLocalizationResources
        {
            Task<Result<List<Guid>>> BulkUpsertLocalizationResources(BulkUpsertLocalizationResourcesCommand command, CancellationToken cancellationToken = default);
        }

        public interface IDeleteLocalizationResource
        {
            Task<Result<Guid>> DeleteLocalizationResource(DeleteLocalizationResourceCommand command, CancellationToken cancellationToken = default);
        }

        #endregion

        #region Tags
        public interface IGetAllTags
        {
            Task<Result<List<TagDto>>> GetAllTags(CancellationToken cancellationToken = default);
        }
        #endregion

        #region Category
        public interface ICreateCategory
        {
            Task<Result<Guid>> CreateCategory(CreateCategoryCommand command, CancellationToken cancellationToken = default);
        }

        public interface IGetAllCategories
        {
            Task<Result<List<CategoryDto>>> GetAllCategories(CancellationToken cancellationToken = default);
        }

        public interface IGetCategoryById
        {
            Task<Result<CategoryDto>> GetCategoryById(Guid categoryId,CancellationToken cancellationToken = default);
        }

        public interface IUpdateCategory
        {
            Task<Result<Guid>> UpdateCategory(UpdateCategoryCommand command, CancellationToken cancellationToken = default);
        }

        public interface IDeleteCategory
        {
            Task<Result<Guid>> DeleteCategory(DeleteCategoryCommand command, CancellationToken cancellationToken = default);
        }
        #endregion

        #region Comments
        public interface ICreateNewComment
        {
            Task<Result<Guid>> CreateNewComment(CreateNewCommentCommand command, CancellationToken cancellationToken = default);
        }

        #endregion
    }
}
