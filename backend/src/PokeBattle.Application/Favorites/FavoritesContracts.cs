using PokeBattle.Application.Battles.PredictBattleOutcome;

namespace PokeBattle.Application.Favorites;

/// <summary>
/// A saved prediction: the question, not the answer. Opening it runs the prediction again, so fixes to
/// the data or formulas show up. The candidate names are kept only to describe it in the list.
/// </summary>
/// <param name="Name">Set by the player; null shows the automatic description.</param>
public sealed record StoredFavorite(long Id, string? Name, PredictBattleOutcomeRequest Prediction, IReadOnlyList<string> Candidates, DateTimeOffset CreatedAt);

/// <param name="Available">False when the target can no longer be met that way at that milestone (e.g. the progression changed).</param>
public sealed record FavoriteItem(long Id, string? Name, PredictBattleOutcomeRequest Prediction, IReadOnlyList<string> Candidates, bool Available, DateTimeOffset CreatedAt);

public sealed record SaveFavoriteRequest(PredictBattleOutcomeRequest? Prediction, string? Name);

public sealed record RenameFavoriteRequest(string? Name);

/// <summary>Where favorites are kept. Every call is scoped to one user: nobody reads or changes another user's favorites.</summary>
public interface IFavoriteStore
{
    /// <summary>The user's favorites, newest first.</summary>
    Task<IReadOnlyList<StoredFavorite>> ListAsync(string userId, CancellationToken cancellationToken);

    Task<StoredFavorite> AddAsync(string userId, string? name, PredictBattleOutcomeRequest prediction, IReadOnlyList<string> candidates, DateTimeOffset createdAt, CancellationToken cancellationToken);

    /// <returns>Null when the user has no favorite with that id.</returns>
    Task<StoredFavorite?> RenameAsync(string userId, long id, string? name, CancellationToken cancellationToken);

    /// <returns>False when the user has no favorite with that id.</returns>
    Task<bool> DeleteAsync(string userId, long id, CancellationToken cancellationToken);
}