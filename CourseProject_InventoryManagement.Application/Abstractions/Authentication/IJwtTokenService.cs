using CourseProject_InventoryManagement.Application.DTOs;
using CourseProject_InventoryManagement.Domain.Entities;

namespace CourseProject_InventoryManagement.Application.Abstractions.Authentication
{
    public interface IJwtTokenService
    {
        AuthTokenDto CreateToken(AppUser user);
    }
}
