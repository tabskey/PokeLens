using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PokeBattle.Application.Auditing;
using PokeBattle.Application.Favorites;
using PokeBattle.Infrastructure.Auditing;
using PokeBattle.Infrastructure.Favorites;

namespace PokeBattle.Infrastructure.Persistence;

public static class PersistenceServiceCollectionExtensions
{
    /// <summary>Postgres for users and audit. Migrations are applied by the bundle, never here.</summary>
    public static IServiceCollection AddPersistence(this IServiceCollection services, string connectionString)
    {
        // The factory also registers the context as scoped, which is what the Identity stores use.
        services.AddDbContextFactory<PokeBattleDbContext>(options => options.UseNpgsql(connectionString));
        services.AddSingleton<IAuditLog, EfAuditLog>();
        services.AddSingleton<IFavoriteStore, EfFavoriteStore>();
        return services;
    }
}