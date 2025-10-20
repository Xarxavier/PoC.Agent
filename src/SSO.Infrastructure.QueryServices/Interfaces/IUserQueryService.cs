using SSO.Domain.Entities;

namespace SSO.Infrastructure.QueryServices.Interfaces;

/// <summary>
/// Query service interface for User operations
/// </summary>
public interface IUserQueryService
{
    Task<User?> GetUserByUsernameAsync(string username);
    Task<User?> GetUserByEmailAsync(string email);
    Task<IEnumerable<User>> GetActiveUsersAsync();
}
