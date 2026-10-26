namespace PokeBattle.Core.Battles;

/// <summary>
/// The three fixed V1 scenarios. In each one the two sides move in opposite directions: what is bad
/// for the candidate is good for the opponent. See docs/decisoes-cenarios.md.
/// </summary>
public static class ScenarioCatalog
{
    public static readonly BattleScenario Pessimistic = new(
        ScenarioKind.Pessimistic,
        Candidate: new SideProfile(NatureProfile.Hindering, IvProfile.Low, EvProfile.None, LevelProfile.Lowest),
        Opponent: new SideProfile(NatureProfile.Favoring, IvProfile.High, EvProfile.None, LevelProfile.Highest));

    public static readonly BattleScenario Neutral = new(
        ScenarioKind.Neutral,
        Candidate: new SideProfile(NatureProfile.Neutral, IvProfile.Average, EvProfile.None, LevelProfile.Midpoint),
        Opponent: new SideProfile(NatureProfile.Neutral, IvProfile.Average, EvProfile.None, LevelProfile.Midpoint));

    public static readonly BattleScenario Optimistic = new(
        ScenarioKind.Optimistic,
        Candidate: new SideProfile(NatureProfile.Favoring, IvProfile.High, EvProfile.None, LevelProfile.Highest),
        Opponent: new SideProfile(NatureProfile.Hindering, IvProfile.Low, EvProfile.None, LevelProfile.Lowest));

    public static IReadOnlyList<BattleScenario> All { get; } = [Pessimistic, Neutral, Optimistic];

    /// <summary>
    /// PROVISIONAL values (quartiles of the uniform 0..31 wild IV distribution), pending review in
    /// docs/decisoes-cenarios.md. The alternative under discussion is 0 / 15 / 31.
    /// </summary>
    public static int IndividualValueFor(IvProfile profile) => profile switch
    {
        IvProfile.Low => 8,
        IvProfile.Average => 15,
        IvProfile.High => 23,
        IvProfile.Maximum => 31,
        _ => throw new ArgumentOutOfRangeException(nameof(profile), profile, null),
    };

    public static int EffortValueFor(EvProfile profile) => profile switch
    {
        EvProfile.None => 0,
        _ => throw new ArgumentOutOfRangeException(nameof(profile), profile, null),
    };
}