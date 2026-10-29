using System.Security.Claims;
using System.Text;

using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace PokeBattle.Api.Auth;

/// <summary>"Jwt" configuration section. The signing key comes only from the environment (Jwt__SigningKey).</summary>
public sealed class JwtOptions
{
    public const string Section = "Jwt";

    /// <summary>HMAC-SHA256 needs a key of at least 256 bits.</summary>
    public const int MinimumKeyBytes = 32;

    public required string Issuer { get; init; }

    public required string Audience { get; init; }

    public required string SigningKey { get; init; }

    public TimeSpan Lifetime { get; init; } = TimeSpan.FromHours(1);

    private SymmetricSecurityKey Key => new(Encoding.UTF8.GetBytes(SigningKey));

    /// <exception cref="InvalidOperationException">The section is missing or the key is too short.</exception>
    public static JwtOptions Load(IConfiguration configuration)
    {
        var options = configuration.GetSection(Section).Get<JwtOptions>();
        if (options is null || string.IsNullOrEmpty(options.Issuer) || string.IsNullOrEmpty(options.Audience))
        {
            throw new InvalidOperationException("Configure Jwt:Issuer and Jwt:Audience.");
        }

        if (options.SigningKey is null || Encoding.UTF8.GetByteCount(options.SigningKey) < MinimumKeyBytes)
        {
            throw new InvalidOperationException($"Set Jwt__SigningKey to at least {MinimumKeyBytes} bytes.");
        }

        return options;
    }

    public TokenValidationParameters ValidationParameters() => new()
    {
        ValidIssuer = Issuer,
        ValidAudience = Audience,
        IssuerSigningKey = Key,
        ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
    };

    public TokenResponse CreateToken(IdentityUser user, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(user);

        var expiresAt = now + Lifetime;
        var token = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = Issuer,
            Audience = Audience,
            Subject = new ClaimsIdentity([
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            ]),
            IssuedAt = now.UtcDateTime,
            NotBefore = now.UtcDateTime,
            Expires = expiresAt.UtcDateTime,
            SigningCredentials = new SigningCredentials(Key, SecurityAlgorithms.HmacSha256),
        });
        return new TokenResponse(token, expiresAt);
    }
}

public sealed record TokenResponse(string AccessToken, DateTimeOffset ExpiresAt);