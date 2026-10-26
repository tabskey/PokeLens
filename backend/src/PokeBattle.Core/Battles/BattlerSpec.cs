using PokeBattle.Core.Stats;
using PokeBattle.Core.Types;

namespace PokeBattle.Core.Battles;

/// <summary>
/// What is known about one Pokémon before a scenario is applied. Each uncertain attribute carries
/// its <see cref="DataSource"/>: an Estimated attribute has no value (the scenario supplies it), any
/// other source must have one. A Known or UserProvided level is a single level.
/// </summary>
public sealed record BattlerSpec
{
    public BattlerSpec(
        StatSpread baseStats,
        PokemonType type1,
        PokemonType? type2,
        LevelRange level,
        DataSource levelSource,
        Nature? nature,
        DataSource natureSource,
        StatSpread? individualValues,
        DataSource ivSource,
        StatSpread? effortValues,
        DataSource evSource)
    {
        ArgumentNullException.ThrowIfNull(baseStats);
        ArgumentNullException.ThrowIfNull(level);

        if (levelSource != DataSource.Estimated && level.Min != level.Max)
        {
            throw new ArgumentException("A Known or UserProvided level must be a single level.", nameof(level));
        }

        EnsureMatchesSource(nature.HasValue, natureSource, nameof(nature));
        EnsureMatchesSource(individualValues is not null, ivSource, nameof(individualValues));
        EnsureMatchesSource(effortValues is not null, evSource, nameof(effortValues));

        BaseStats = baseStats;
        Type1 = type1;
        Type2 = type2;
        Level = level;
        LevelSource = levelSource;
        Nature = nature;
        NatureSource = natureSource;
        IndividualValues = individualValues;
        IvSource = ivSource;
        EffortValues = effortValues;
        EvSource = evSource;
    }

    public StatSpread BaseStats { get; }

    public PokemonType Type1 { get; }

    public PokemonType? Type2 { get; }

    public LevelRange Level { get; }

    public DataSource LevelSource { get; }

    public Nature? Nature { get; }

    public DataSource NatureSource { get; }

    public StatSpread? IndividualValues { get; }

    public DataSource IvSource { get; }

    public StatSpread? EffortValues { get; }

    public DataSource EvSource { get; }

    /// <summary>A wild Pokémon: level range from the route, everything else Estimated.</summary>
    public static BattlerSpec Wild(StatSpread baseStats, PokemonType type1, PokemonType? type2, LevelRange levels)
        => new(baseStats, type1, type2, levels, DataSource.Estimated, null, DataSource.Estimated, null, DataSource.Estimated, null, DataSource.Estimated);

    private static void EnsureMatchesSource(bool hasValue, DataSource source, string paramName)
    {
        if (hasValue == (source == DataSource.Estimated))
        {
            throw new ArgumentException(
                source == DataSource.Estimated
                    ? "An Estimated attribute must not have a value; the scenario supplies it."
                    : "A Known or UserProvided attribute must have a value.",
                paramName);
        }
    }
}