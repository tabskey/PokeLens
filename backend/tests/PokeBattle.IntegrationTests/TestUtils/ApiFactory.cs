using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PokeBattle.Infrastructure.Persistence;

using Testcontainers.PostgreSql;

namespace PokeBattle.IntegrationTests.TestUtils;

/// <summary>The API against a throwaway Postgres (Testcontainers), migrated the way the bundle does it.</summary>
public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    /// <summary>Configured as admin; tests that register it get the Admin role at login.</summary>
    public const string AdminEmail = "admin@pokebattle.test";

    private readonly PostgreSqlContainer database = new PostgreSqlBuilder("postgres:17.6-alpine").Build();

    public async Task InitializeAsync()
    {
        await database.StartAsync();
        using var scope = Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<PokeBattleDbContext>().Database.MigrateAsync();
    }

    /// <summary>Runs <paramref name="query"/> against the test database.</summary>
    public async Task<T> QueryAsync<T>(Func<PokeBattleDbContext, Task<T>> query)
    {
        using var scope = Services.CreateScope();
        return await query(scope.ServiceProvider.GetRequiredService<PokeBattleDbContext>());
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Default", database.GetConnectionString());
        builder.UseSetting("Jwt:SigningKey", "integration-tests-signing-key-32-bytes-or-more");
        builder.UseSetting("Auth:AdminEmails", AdminEmail);
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
        await database.DisposeAsync();
    }
}

/// <summary>One Postgres container shared by every test class that talks to the API.</summary>
[CollectionDefinition(Name)]
public sealed class ApiTests : ICollectionFixture<ApiFactory>
{
    public const string Name = "api";
}