using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PokeBattle.Infrastructure.Persistence;

/// <summary>
/// Lets `dotnet ef` build the model without starting the API. Adding a migration or building the bundle
/// never opens this connection; the bundle receives the real one with --connection.
/// </summary>
internal sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<PokeBattleDbContext>
{
    public PokeBattleDbContext CreateDbContext(string[] args)
        => new(new DbContextOptionsBuilder<PokeBattleDbContext>().UseNpgsql("Host=localhost;Database=pokebattle").Options);
}