using PokeBattle.Application.Auditing;
using PokeBattle.Infrastructure.Persistence;

namespace PokeBattle.Api.Endpoints;

/// <summary>Admin-only screens; the policy requires the Admin role in the token.</summary>
public static class AdminEndpoints
{
    public const string Policy = PokeBattleDbContext.AdminRole;

    private const int MaxEntries = 200;

    public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/admin/audit", async (int? limit, IAuditLog auditLog, CancellationToken cancellationToken) =>
            limit is < 1 or > MaxEntries
                ? Results.ValidationProblem(new Dictionary<string, string[]> { ["limit"] = [$"Limite deve ficar entre 1 e {MaxEntries}."] })
                : Results.Ok(await auditLog.LatestAsync(limit ?? 50, cancellationToken)))
            .RequireAuthorization(Policy);

        return app;
    }
}