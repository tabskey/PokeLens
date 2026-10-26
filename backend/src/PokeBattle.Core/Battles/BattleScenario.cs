namespace PokeBattle.Core.Battles;

/// <summary>
/// The conditions assumed for both sides (Nature, IV, EV, level). It deliberately carries no RNG:
/// damage roll and critical hits are evaluated by <see cref="Damage.DamageRollPolicy"/> and
/// <see cref="Damage.CriticalHitPolicy"/>.
/// </summary>
public sealed record BattleScenario(ScenarioKind Kind, SideProfile Candidate, SideProfile Opponent)
{
    public SideProfile For(BattleSide side) => side == BattleSide.Candidate ? Candidate : Opponent;
}