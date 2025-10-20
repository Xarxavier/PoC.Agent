using Microsoft.EntityFrameworkCore;
using SSO.Domain.Entities;
using SSO.Infrastructure.EntityFramework;
using SSO.Infrastructure.QueryServices.Interfaces;

namespace SSO.Infrastructure.QueryServices.Services;

/// <summary>
/// Query service implementation for User operations
/// </summary>
public class UserQueryService : IUserQueryService
{
    private readonly SsoContext _context;

    public UserQueryService(SsoContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync()
    {
        return await _context.Users
            .Where(u => u.IsActive)
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .ToListAsync();
    }
}
