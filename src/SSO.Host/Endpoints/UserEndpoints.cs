using SSO.Domain.Entities;
using SSO.Infrastructure.QueryServices.Interfaces;

namespace SSO.Host.Endpoints;

/// <summary>
/// User endpoints for the SSO API
/// </summary>
public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("Users")
            .WithOpenApi();

        group.MapGet("/", GetAllActiveUsers)
            .WithName("GetAllActiveUsers")
            .WithSummary("Get all active users");

        group.MapGet("/{username}", GetUserByUsername)
            .WithName("GetUserByUsername")
            .WithSummary("Get user by username");

        group.MapGet("/email/{email}", GetUserByEmail)
            .WithName("GetUserByEmail")
            .WithSummary("Get user by email");
    }

    private static async Task<IResult> GetAllActiveUsers(IUserQueryService userQueryService)
    {
        try
        {
            var users = await userQueryService.GetActiveUsersAsync();
            return Results.Ok(users);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    private static async Task<IResult> GetUserByUsername(string username, IUserQueryService userQueryService)
    {
        try
        {
            var user = await userQueryService.GetUserByUsernameAsync(username);
            return user != null ? Results.Ok(user) : Results.NotFound();
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    private static async Task<IResult> GetUserByEmail(string email, IUserQueryService userQueryService)
    {
        try
        {
            var user = await userQueryService.GetUserByEmailAsync(email);
            return user != null ? Results.Ok(user) : Results.NotFound();
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }
}
