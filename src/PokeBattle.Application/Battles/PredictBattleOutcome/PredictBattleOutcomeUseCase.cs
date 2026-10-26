using PokeBattle.Application.GameData;
using PokeBattle.Core.Battles;
using PokeBattle.Core.Capture;
using PokeBattle.Core.Damage;

namespace PokeBattle.Application.Battles.PredictBattleOutcome;

/// <summary>
/// Recommends up to five candidates for a target. Progression is a validity filter: candidates and
/// moves the player cannot get yet are dropped before anything is simulated (docs/arquitetura.md §7).
/// </summary>
public sealed class PredictBattleOutcomeUseCase
{
    public static readonly IReadOnlyList<string> Versions = ["firered", "leafgreen"];

    private readonly GamePackage package;
    private readonly Dictionary<string, SpeciesData> species;
    private readonly Dictionary<string, (MoveData Data, Move Move)> moves;
    private readonly Dictionary<Move, string> moveNames = new(ReferenceEqualityComparer.Instance);

    public PredictBattleOutcomeUseCase(GamePackage package)
    {
        ArgumentNullException.ThrowIfNull(package);

        this.package = package;
        species = package.Species.ToDictionary(s => s.Name, StringComparer.Ordinal);
        moves = package.Moves.ToDictionary(m => m.Name, m => (m, new Move(m.Type, m.Power, m.HighCriticalRatio)), StringComparer.Ordinal);
        foreach (var (name, entry) in moves)
        {
            moveNames[entry.Move] = name;
        }
    }

    /// <summary>Returns the problems per request field; empty when the request can be executed.</summary>
    public IReadOnlyDictionary<string, string[]> Validate(PredictBattleOutcomeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var errors = new Dictionary<string, string[]>();
        if (!Versions.Contains(request.Version))
        {
            errors["version"] = ["Versão deve ser 'firered' ou 'leafgreen'."];
        }

        if (request.Milestone is null || !package.Progression.Milestones.Contains(request.Milestone))
        {
            errors["milestone"] = [$"Ponto de progressão desconhecido. Valores aceitos: {string.Join(", ", package.Progression.Milestones)}."];
        }

        if (request.Target is null || !species.ContainsKey(request.Target))
        {
            errors["target"] = ["Pokémon-alvo desconhecido."];
        }
        else if (!errors.ContainsKey("version") && !TargetEncounters(request).Any())
        {
            errors["area"] = ["O Pokémon-alvo não aparece nessa área nessa versão."];
        }

        if (!Enum.IsDefined(request.Objective))
        {
            errors["objective"] = ["Objetivo deve ser 'Defeat' ou 'Catch'."];
        }

        if (!Enum.IsDefined(request.Scenario))
        {
            errors["scenario"] = ["Cenário deve ser 'Pessimistic', 'Neutral' ou 'Optimistic'."];
        }

        return errors;
    }

    /// <exception cref="ArgumentException">The request is invalid; call <see cref="Validate"/> first.</exception>
    public PredictBattleOutcomeResponse Execute(PredictBattleOutcomeRequest request)
    {
        if (Validate(request).Count > 0)
        {
            throw new ArgumentException("Invalid request; call Validate first.", nameof(request));
        }

        var targetEncounters = TargetEncounters(request).ToList();
        var targetLevels = new LevelRange(targetEncounters.Min(e => e.MinLevel), targetEncounters.Max(e => e.MaxLevel));
        var targetData = species[request.Target];
        var target = Spec(targetData, targetLevels);
        // A wild target knows its level-up moves only; the highest level is the conservative assumption.
        var targetMoves = Moves(targetData, targetLevels.Max, machines: []);

        var milestone = MilestoneIndex(request.Milestone);
        var areas = Unlocked(package.Progression.Areas, milestone);
        var methods = Unlocked(package.Progression.EncounterMethods, milestone);
        var machines = Unlocked(package.Progression.Machines, milestone);

        // shortcut: encounter conditions (story flags, trades, starter choice) are ignored; the curated
        // progression decides availability. Upgrade when progression.json models them.
        var candidates = package.Encounters
            .Where(e => e.Version == request.Version && areas.Contains(e.Area) && methods.Contains(e.Method))
            .GroupBy(e => e.Pokemon, StringComparer.Ordinal)
            .Select(group => group
                .OrderByDescending(e => e.MinLevel)
                .ThenByDescending(e => e.MaxLevel)
                .ThenBy(e => e.Area, StringComparer.Ordinal)
                .First())
            .ToDictionary(e => e.Pokemon, StringComparer.Ordinal);

        var evaluations = candidates.Values.Select(encounter =>
        {
            var data = species[encounter.Pokemon];
            var levels = new LevelRange(encounter.MinLevel, encounter.MaxLevel);
            var spec = Spec(data, levels);
            // The lowest level is the one every scenario can count on.
            var candidateMoves = Moves(data, levels.Min, machines);

            var simulations = BattleSimulationService.Simulate(spec, candidateMoves, target, targetMoves, DamageRollPolicy.FullRange, CriticalHitPolicy.Never);
            IReadOnlyList<CatchPlan> plans = request.Objective == BattleObjective.Catch
                ? [.. ScenarioCatalog.All.Select(scenario => CatchPlanner.Plan(spec, candidateMoves, target, targetData.CaptureRate, scenario))]
                : [];
            return new CandidateEvaluation(encounter.Pokemon, simulations, plans);
        });

        var ranked = BattleRankingService.Rank(evaluations, request.Objective, request.Scenario);

        return new PredictBattleOutcomeResponse(
            request.Target,
            targetLevels.Min,
            targetLevels.Max,
            request.Objective,
            request.Scenario,
            [.. ranked.Select(evaluation => ToResult(evaluation, candidates[evaluation.Name]))]);
    }

    private IEnumerable<EncounterData> TargetEncounters(PredictBattleOutcomeRequest request)
        => package.Encounters.Where(e => e.Pokemon == request.Target && e.Area == request.Area && e.Version == request.Version);

    private int MilestoneIndex(string milestone) => package.Progression.Milestones.ToList().IndexOf(milestone);

    private HashSet<string> Unlocked(IEnumerable<Unlock> unlocks, int milestone)
        => unlocks.Where(u => MilestoneIndex(u.Milestone) <= milestone).Select(u => u.Name).ToHashSet(StringComparer.Ordinal);

    /// <summary>
    /// Level-up moves up to <paramref name="level"/> plus the machine moves already obtained. Egg and
    /// tutor moves are left out because their availability is not curated.
    /// </summary>
    // shortcut: a wild Pokémon really knows only its last four level-up moves; status moves are not in
    // the package, so that slot rule cannot be applied yet. Upgrade with the V2 movesets.
    private List<Move> Moves(SpeciesData data, int level, HashSet<string> machines)
        => [.. data.Learnset
            .Where(entry => entry.Method switch
            {
                LearnMethod.LevelUp => entry.Level <= level,
                LearnMethod.Machine => moves[entry.Move].Data.Machine is { } machine && machines.Contains(machine),
                _ => false,
            })
            .Select(entry => entry.Move)
            .Distinct(StringComparer.Ordinal)
            .Select(name => moves[name].Move)];

    private static BattlerSpec Spec(SpeciesData data, LevelRange levels)
        => BattlerSpec.Wild(data.BaseStats, data.Types[0], data.Types.Count > 1 ? data.Types[1] : null, levels);

    private CandidateResult ToResult(CandidateEvaluation evaluation, EncounterData encounter)
        => new(
            encounter.Pokemon,
            encounter.Area,
            encounter.Method,
            encounter.MinLevel,
            encounter.MaxLevel,
            [.. evaluation.Simulations.Select(simulation => new ScenarioResult(
                simulation.Scenario,
                ToResult(simulation.CandidateAttack),
                ToResult(simulation.OpponentAttack),
                simulation.CandidateKo,
                simulation.Outcome,
                evaluation.CatchPlans.SingleOrDefault(plan => plan.Scenario == simulation.Scenario) is { } plan ? ToResult(plan) : null))]);

    private CatchResult ToResult(CatchPlan plan)
        => new(
            ToResult(plan.Hit),
            plan.KoRisk,
            plan.TargetHp,
            [.. plan.Capture.Select(capture => new BallResult(
                capture.Key,
                capture.Value.ChancePerBall,
                capture.Value.BallsFor50Percent,
                capture.Value.BallsFor90Percent,
                capture.Value.BallsFor95Percent,
                capture.Value.BallsFor99Percent))]);

    private AttackResult? ToResult(AttackOutcome? attack)
        => attack is null
            ? null
            : new AttackResult(
                moveNames[attack.Move],
                attack.Damage.Min,
                attack.Damage.Max,
                attack.TargetMaxHp,
                attack.RemainingHp.Min,
                attack.RemainingHp.Max,
                attack.TurnsToKo?.Min,
                attack.TurnsToKo?.Max);
}