using System.Security.Claims;

using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.JsonWebTokens;

using PokeBattle.Api.Auth;

namespace PokeBattle.Api.Endpoints;

/// <param name="Email">Also the user name.</param>
public sealed record AuthRequest(string? Email, string? Password);

public sealed record MeResponse(string Id, string Email);

/// <summary>Optional accounts: predicting works without one; with one, the audit log records the user.</summary>
public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/auth/register", async (AuthRequest request, UserManager<IdentityUser> users) =>
        {
            var missing = Missing(request);
            if (missing.Count > 0)
            {
                return Results.ValidationProblem(missing);
            }

            var result = await users.CreateAsync(new IdentityUser { UserName = request.Email, Email = request.Email }, request.Password!);
            return result.Succeeded
                ? Results.Created("/api/v1/auth/me", null)
                : Results.ValidationProblem(result.Errors
                    .GroupBy(error => error.Code.StartsWith("Password", StringComparison.Ordinal) ? "password" : "email")
                    .ToDictionary(group => group.Key, group => group.Select(error => error.Description).Distinct().ToArray()));
        });

        app.MapPost("/api/v1/auth/login", async (AuthRequest request, UserManager<IdentityUser> users, SignInManager<IdentityUser> signIn, JwtOptions jwt, TimeProvider clock) =>
        {
            var user = request.Email is null ? null : await users.FindByEmailAsync(request.Email);
            if (user is null || request.Password is null)
            {
                return InvalidCredentials();
            }

            var result = await signIn.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
            if (result.IsLockedOut)
            {
                return Results.Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Muitas tentativas. Tente de novo em alguns minutos.");
            }

            return result.Succeeded ? Results.Ok(jwt.CreateToken(user, clock.GetUtcNow())) : InvalidCredentials();
        });

        app.MapGet("/api/v1/auth/me", (ClaimsPrincipal user) =>
            new MeResponse(user.FindFirstValue(JwtRegisteredClaimNames.Sub)!, user.FindFirstValue(JwtRegisteredClaimNames.Email)!))
            .RequireAuthorization();

        return app;
    }

    // Same answer for unknown e-mail and wrong password, so the endpoint does not reveal who has an account.
    private static IResult InvalidCredentials()
        => Results.Problem(statusCode: StatusCodes.Status401Unauthorized, title: "E-mail ou senha inválidos.");

    private static Dictionary<string, string[]> Missing(AuthRequest request)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            errors["email"] = ["Informe o e-mail."];
        }

        if (string.IsNullOrEmpty(request.Password))
        {
            errors["password"] = ["Informe a senha."];
        }

        return errors;
    }
}