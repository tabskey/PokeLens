using System.Security.Claims;

using Microsoft.IdentityModel.JsonWebTokens;

using PokeBattle.Application.Favorites;

namespace PokeBattle.Api.Endpoints;

/// <summary>Saved predictions; only for logged-in players, each one seeing only their own.</summary>
public static class FavoritesEndpoints
{
    private const string Path = "/api/v1/favorites";

    public static IEndpointRouteBuilder MapFavoritesEndpoints(this IEndpointRouteBuilder app)
    {
        var favorites = app.MapGroup(Path).RequireAuthorization();

        favorites.MapGet("/", async (ClaimsPrincipal user, FavoritesUseCase useCase, CancellationToken cancellationToken)
            => Results.Ok(await useCase.ListAsync(UserId(user), cancellationToken)));

        // Saving the same prediction again returns the existing favorite with 200 instead of a duplicate.
        favorites.MapPost("/", async (SaveFavoriteRequest request, ClaimsPrincipal user, FavoritesUseCase useCase, CancellationToken cancellationToken) =>
        {
            var (item, created, errors) = await useCase.SaveAsync(UserId(user), request, cancellationToken);
            if (errors.Count > 0)
            {
                return Results.ValidationProblem(errors);
            }

            return created ? Results.Created($"{Path}/{item!.Id}", item) : Results.Ok(item);
        });

        favorites.MapPatch("/{id:long}", async (long id, RenameFavoriteRequest request, ClaimsPrincipal user, FavoritesUseCase useCase, CancellationToken cancellationToken) =>
        {
            var (item, errors) = await useCase.RenameAsync(UserId(user), id, request, cancellationToken);
            if (errors.Count > 0)
            {
                return Results.ValidationProblem(errors);
            }

            return item is null ? Results.NotFound() : Results.Ok(item);
        });

        favorites.MapDelete("/{id:long}", async (long id, ClaimsPrincipal user, FavoritesUseCase useCase, CancellationToken cancellationToken)
            => await useCase.DeleteAsync(UserId(user), id, cancellationToken) ? Results.NoContent() : Results.NotFound());

        return app;
    }

    // RequireAuthorization guarantees a validated token, which always carries the subject.
    private static string UserId(ClaimsPrincipal user) => user.FindFirstValue(JwtRegisteredClaimNames.Sub)!;
}