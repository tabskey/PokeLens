using PokeBattle.Application.Battles.PredictBattleOutcome;

namespace PokeBattle.Application.Favorites;

/// <summary>Saved predictions of a logged-in player: at most <see cref="Limit"/>, one per question, renameable.</summary>
public sealed class FavoritesUseCase(IFavoriteStore store, PredictBattleOutcomeUseCase predictions, TimeProvider clock)
{
    public const int Limit = 50;
    public const int MaxNameLength = 60;

    public async Task<IReadOnlyList<FavoriteItem>> ListAsync(string userId, CancellationToken cancellationToken)
        => [.. (await store.ListAsync(userId, cancellationToken).ConfigureAwait(false)).Select(ToItem)];

    /// <summary>
    /// Saves the question with the current candidate names. Saving the same question again returns the
    /// existing favorite (Created is false) instead of a duplicate.
    /// </summary>
    public async Task<(FavoriteItem? Item, bool Created, IReadOnlyDictionary<string, string[]> Errors)> SaveAsync(string userId, SaveFavoriteRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Prediction is null)
        {
            return (null, false, new Dictionary<string, string[]> { ["prediction"] = ["Informe a previsão a salvar."] });
        }

        var errors = new Dictionary<string, string[]>(predictions.Validate(request.Prediction));
        var (name, nameErrors) = Name(request.Name);
        foreach (var (field, messages) in nameErrors)
        {
            errors[field] = messages;
        }

        if (errors.Count > 0)
        {
            return (null, false, errors);
        }

        var saved = await store.ListAsync(userId, cancellationToken).ConfigureAwait(false);
        if (saved.FirstOrDefault(f => f.Prediction == request.Prediction) is { } existing)
        {
            return (ToItem(existing), false, errors);
        }

        // shortcut: two saves at the same instant can pass this check together and end at Limit + 1; harmless for a soft cap.
        if (saved.Count >= Limit)
        {
            return (null, false, new Dictionary<string, string[]> { ["favorites"] = [$"Você já tem {Limit} favoritos. Remova um para salvar outro."] });
        }

        var candidates = predictions.Execute(request.Prediction).Candidates.Select(c => c.Pokemon).ToList();
        var stored = await store.AddAsync(userId, name, request.Prediction, candidates, clock.GetUtcNow(), cancellationToken).ConfigureAwait(false);
        return (ToItem(stored), true, errors);
    }

    /// <summary>An empty name goes back to the automatic description.</summary>
    /// <returns>Item is null with no errors when the user has no favorite with that id.</returns>
    public async Task<(FavoriteItem? Item, IReadOnlyDictionary<string, string[]> Errors)> RenameAsync(string userId, long id, RenameFavoriteRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var (name, errors) = Name(request.Name);
        if (errors.Count > 0)
        {
            return (null, errors);
        }

        var renamed = await store.RenameAsync(userId, id, name, cancellationToken).ConfigureAwait(false);
        return (renamed is null ? null : ToItem(renamed), errors);
    }

    public Task<bool> DeleteAsync(string userId, long id, CancellationToken cancellationToken)
        => store.DeleteAsync(userId, id, cancellationToken);

    // Available means the prediction still validates against the current package and progression.
    private FavoriteItem ToItem(StoredFavorite favorite)
        => new(favorite.Id, favorite.Name, favorite.Prediction, favorite.Candidates, predictions.Validate(favorite.Prediction).Count == 0, favorite.CreatedAt);

    private static (string? Name, Dictionary<string, string[]> Errors) Name(string? name)
    {
        var trimmed = string.IsNullOrWhiteSpace(name) ? null : name.Trim();
        return trimmed is { Length: > MaxNameLength }
            ? (null, new Dictionary<string, string[]> { ["name"] = [$"O nome pode ter até {MaxNameLength} caracteres."] })
            : (trimmed, []);
    }
}