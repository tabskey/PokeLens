namespace PokeBattle.Api.Auth;

/// <summary>
/// Accounts with the Admin role, from Auth:AdminEmails (e-mails separated by ';' or ','). The role is
/// synced at every login, so removing an e-mail from the list revokes it at that user's next login.
/// </summary>
public sealed record AdminEmails(IReadOnlySet<string> Emails)
{
    public static AdminEmails Load(IConfiguration configuration)
        => new(new HashSet<string>(
            (configuration["Auth:AdminEmails"] ?? string.Empty).Split([';', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
            StringComparer.OrdinalIgnoreCase));

    public bool Contains(string? email) => email is not null && Emails.Contains(email);
}