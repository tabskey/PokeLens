using Microsoft.EntityFrameworkCore;

using PokeBattle.Application.Battles.PredictBattleOutcome;
using PokeBattle.Application.Favorites;
using PokeBattle.Infrastructure.Persistence;

namespace PokeBattle.Infrastructure.Favorites;

/// <summary>Every query filters by user id, so one player never reaches another's favorites.</summary>
public sealed class EfFavoriteStore(IDbContextFactory<PokeBattleDbContext> contexts) : IFavoriteStore
{
    public async Task<IReadOnlyList<StoredFavorite>> ListAsync(string userId, CancellationToken cancellationToken)
    {
        var db = await contexts.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await using (db.ConfigureAwait(false))
        {
            var rows = await db.SavedPredictions
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.CreatedAt)
                .ThenByDescending(f => f.Id)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
            return [.. rows.Select(ToStored)];
        }
    }

    public async Task<StoredFavorite> AddAsync(string userId, string? name, PredictBattleOutcomeRequest prediction, IReadOnlyList<string> candidates, DateTimeOffset createdAt, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(prediction);

        var db = await contexts.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await using (db.ConfigureAwait(false))
        {
            var row = new SavedPrediction
            {
                UserId = userId,
                Name = name,
                Target = prediction.Target,
                Area = prediction.Area,
                Method = prediction.Method,
                Version = prediction.Version,
                Milestone = prediction.Milestone,
                Objective = prediction.Objective,
                Scenario = prediction.Scenario,
                Candidates = [.. candidates],
                CreatedAt = createdAt,
            };
            db.SavedPredictions.Add(row);
            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return ToStored(row);
        }
    }

    public async Task<StoredFavorite?> RenameAsync(string userId, long id, string? name, CancellationToken cancellationToken)
    {
        var db = await contexts.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await using (db.ConfigureAwait(false))
        {
            var row = await db.SavedPredictions.SingleOrDefaultAsync(f => f.Id == id && f.UserId == userId, cancellationToken).ConfigureAwait(false);
            if (row is null)
            {
                return null;
            }

            row.Name = name;
            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return ToStored(row);
        }
    }

    public async Task<bool> DeleteAsync(string userId, long id, CancellationToken cancellationToken)
    {
        var db = await contexts.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await using (db.ConfigureAwait(false))
        {
            return await db.SavedPredictions.Where(f => f.Id == id && f.UserId == userId).ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false) > 0;
        }
    }

    private static StoredFavorite ToStored(SavedPrediction row)
        => new(
            row.Id,
            row.Name,
            new PredictBattleOutcomeRequest(row.Target, row.Area, row.Method, row.Version, row.Milestone, row.Objective, row.Scenario),
            row.Candidates,
            row.CreatedAt);
}