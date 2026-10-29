using PokeBattle.Application.Auditing;
using PokeBattle.Application.Battles.PredictBattleOutcome;
using PokeBattle.Application.Favorites;
using PokeBattle.Application.Tests.Battles;
using PokeBattle.Core.Battles;

namespace PokeBattle.Application.Tests.Favorites;

public class FavoritesUseCaseTests
{
    private const string User = "user-1";

    private static readonly PredictBattleOutcomeRequest Pidgey = new("pidgey", "route-a", "walk", "firered", "start", BattleObjective.Defeat, ScenarioKind.Neutral);

    private readonly InMemoryFavoriteStore store = new();
    private readonly FavoritesUseCase useCase;

    public FavoritesUseCaseTests()
    {
        var predictions = new PredictBattleOutcomeUseCase(PredictBattleOutcomeUseCaseTests.Package, new NoAuditLog(), TimeProvider.System);
        useCase = new FavoritesUseCase(store, predictions, TimeProvider.System);
    }

    [Fact]
    public async Task Save_KeepsTheQuestionAndTheCurrentCandidateNames()
    {
        var (item, created, errors) = await useCase.SaveAsync(User, new SaveFavoriteRequest(Pidgey, "  Rota A  "), CancellationToken.None);

        Assert.Empty(errors);
        Assert.True(created);
        Assert.Equal(("Rota A", Pidgey, true), (item!.Name, item.Prediction, item.Available));
        Assert.Contains("rattata", item.Candidates);
    }

    [Fact]
    public async Task Save_SameQuestionTwice_ReturnsTheExistingFavorite()
    {
        var (first, _, _) = await useCase.SaveAsync(User, new SaveFavoriteRequest(Pidgey, null), CancellationToken.None);

        var (again, created, _) = await useCase.SaveAsync(User, new SaveFavoriteRequest(Pidgey with { }, "outro nome"), CancellationToken.None);

        Assert.False(created);
        Assert.Equal(first!.Id, again!.Id);
        Assert.Single(await useCase.ListAsync(User, CancellationToken.None));
    }

    [Fact]
    public async Task Save_AtTheLimit_ReportsFavorites()
    {
        for (var i = 0; i < FavoritesUseCase.Limit; i++)
        {
            await store.AddAsync(User, null, Pidgey with { Milestone = $"m{i}" }, [], DateTimeOffset.UnixEpoch, CancellationToken.None);
        }

        var (item, _, errors) = await useCase.SaveAsync(User, new SaveFavoriteRequest(Pidgey, null), CancellationToken.None);

        Assert.Null(item);
        Assert.Equal(["favorites"], errors.Keys);
    }

    [Fact]
    public async Task Save_InvalidPredictionOrLongName_ReportsTheFields()
    {
        var request = new SaveFavoriteRequest(Pidgey with { Method = "old-rod" }, new string('x', FavoritesUseCase.MaxNameLength + 1));

        var (_, _, errors) = await useCase.SaveAsync(User, request, CancellationToken.None);

        Assert.Equal(["method", "name"], errors.Keys.Order());
        Assert.Empty(await store.ListAsync(User, CancellationToken.None));
    }

    [Fact]
    public async Task List_FavoriteNoLongerReachable_IsMarkedUnavailable()
    {
        // route-b opens only at badge-1; a favorite saved for "start" can no longer be predicted.
        await store.AddAsync(User, null, Pidgey with { Area = "route-b" }, [], DateTimeOffset.UnixEpoch, CancellationToken.None);

        Assert.False(Assert.Single(await useCase.ListAsync(User, CancellationToken.None)).Available);
    }

    [Fact]
    public async Task Rename_BlankName_GoesBackToTheAutomaticDescription()
    {
        var (item, _, _) = await useCase.SaveAsync(User, new SaveFavoriteRequest(Pidgey, "Rota A"), CancellationToken.None);

        var (renamed, errors) = await useCase.RenameAsync(User, item!.Id, new RenameFavoriteRequest("  "), CancellationToken.None);

        Assert.Empty(errors);
        Assert.Null(renamed!.Name);
    }

    [Fact]
    public async Task RenameAndDelete_AnotherUsersFavorite_AreNotFound()
    {
        var (item, _, _) = await useCase.SaveAsync(User, new SaveFavoriteRequest(Pidgey, null), CancellationToken.None);

        var (renamed, errors) = await useCase.RenameAsync("user-2", item!.Id, new RenameFavoriteRequest("meu"), CancellationToken.None);

        Assert.Null(renamed);
        Assert.Empty(errors);
        Assert.False(await useCase.DeleteAsync("user-2", item.Id, CancellationToken.None));
        Assert.True(await useCase.DeleteAsync(User, item.Id, CancellationToken.None));
    }

    private sealed class InMemoryFavoriteStore : IFavoriteStore
    {
        private readonly List<(string User, StoredFavorite Favorite)> rows = [];

        public Task<IReadOnlyList<StoredFavorite>> ListAsync(string userId, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<StoredFavorite>>([.. rows.Where(r => r.User == userId).Select(r => r.Favorite).Reverse()]);

        public Task<StoredFavorite> AddAsync(string userId, string? name, PredictBattleOutcomeRequest prediction, IReadOnlyList<string> candidates, DateTimeOffset createdAt, CancellationToken cancellationToken)
        {
            var favorite = new StoredFavorite(rows.Count + 1, name, prediction, candidates, createdAt);
            rows.Add((userId, favorite));
            return Task.FromResult(favorite);
        }

        public Task<StoredFavorite?> RenameAsync(string userId, long id, string? name, CancellationToken cancellationToken)
        {
            var index = rows.FindIndex(r => r.User == userId && r.Favorite.Id == id);
            if (index < 0)
            {
                return Task.FromResult<StoredFavorite?>(null);
            }

            rows[index] = (userId, rows[index].Favorite with { Name = name });
            return Task.FromResult<StoredFavorite?>(rows[index].Favorite);
        }

        public Task<bool> DeleteAsync(string userId, long id, CancellationToken cancellationToken)
            => Task.FromResult(rows.RemoveAll(r => r.User == userId && r.Favorite.Id == id) > 0);
    }

    private sealed class NoAuditLog : IAuditLog
    {
        public Task RecordAsync(AuditEntry entry, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<IReadOnlyList<AuditLogItem>> LatestAsync(int count, CancellationToken cancellationToken) => throw new NotSupportedException();
    }
}