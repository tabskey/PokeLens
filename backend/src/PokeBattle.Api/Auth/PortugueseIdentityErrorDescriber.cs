using Microsoft.AspNetCore.Identity;

namespace PokeBattle.Api.Auth;

/// <summary>
/// Backend validation messages are in Portuguese (AGENTS.md). Only the errors the register endpoint can
/// produce with the configured policy (unique e-mail, minimum length) are translated.
/// </summary>
public sealed class PortugueseIdentityErrorDescriber : IdentityErrorDescriber
{
    public override IdentityError DuplicateEmail(string email) => new() { Code = nameof(DuplicateEmail), Description = "Já existe uma conta com esse e-mail." };

    // The user name is the e-mail, so both duplicates mean the same thing to the player.
    public override IdentityError DuplicateUserName(string userName) => new() { Code = nameof(DuplicateUserName), Description = "Já existe uma conta com esse e-mail." };

    public override IdentityError InvalidEmail(string? email) => new() { Code = nameof(InvalidEmail), Description = "E-mail inválido." };

    public override IdentityError InvalidUserName(string? userName) => new() { Code = nameof(InvalidUserName), Description = "E-mail inválido." };

    public override IdentityError PasswordTooShort(int length) => new() { Code = nameof(PasswordTooShort), Description = $"A senha precisa ter pelo menos {length} caracteres." };
}