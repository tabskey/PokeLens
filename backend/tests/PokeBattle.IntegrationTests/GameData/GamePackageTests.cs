using PokeBattle.Application.GameData;
using PokeBattle.Core.Types;
using PokeBattle.Infrastructure.GameData;

namespace PokeBattle.IntegrationTests.GameData;

/// <summary>Checks the committed data/gen3-frlg package, as copied next to the API.</summary>
public class GamePackageTests
{
    private static readonly GamePackage Package =
        GamePackageLoader.Load(Path.Combine(AppContext.BaseDirectory, "data", "gen3-frlg"));

    [Fact]
    public void Route1_FireRed_HasLevels2To5()
    {
        var route1 = Package.Encounters
            .Where(e => e.Area == "kanto-route-1-area" && e.Version == "firered")
            .ToList();

        Assert.Equal(2, route1.Min(e => e.MinLevel));
        Assert.Equal(5, route1.Max(e => e.MaxLevel));
        Assert.Equal(["pidgey", "rattata"], route1.Select(e => e.Pokemon).Distinct().Order());
    }

    [Fact]
    public void Bite_IsDarkAndSpecial()
    {
        var bite = Package.Moves.Single(m => m.Name == "bite");

        Assert.Equal(PokemonType.Dark, bite.Type);
        Assert.Equal(MoveCategory.Special, bite.Category);
    }

    [Fact]
    public void Butterfree_HasNoHiddenAbility()
    {
        Assert.Equal(["compound-eyes"], Package.Species.Single(s => s.Name == "butterfree").Abilities);
    }

    [Fact]
    public void EveryAreaWithWildEncounters_HasAMilestone()
    {
        // Altering Cave B–I only change through Mystery Gift events; the default layout is cave A.
        var curated = Package.Progression.Areas.Select(a => a.Name).ToHashSet(StringComparer.Ordinal);
        var missing = Package.Encounters
            .Where(e => GamePackage.WildMethods.Contains(e.Method) && !e.Area.StartsWith("kanto-altering-cave-", StringComparison.Ordinal))
            .Select(e => e.Area)
            .Where(area => !curated.Contains(area))
            .Distinct();

        Assert.Empty(missing);
    }

    [Fact]
    public void Validate_ProgressionReferencingMissingData_ReportsEachProblem()
    {
        var broken = Package with
        {
            Progression = Package.Progression with
            {
                Areas = [new AreaUnlock("kanto-route-99-area", "start", " ")],
                Machines = [new Unlock("tm39", "zephyr-badge")],
            },
        };

        var errors = broken.Validate();

        Assert.Contains(errors, e => e.Contains("kanto-route-99-area", StringComparison.Ordinal));
        Assert.Contains(errors, e => e.Contains("zephyr-badge", StringComparison.Ordinal));
        Assert.Contains(errors, e => e.Contains("displayName", StringComparison.Ordinal));
    }
}