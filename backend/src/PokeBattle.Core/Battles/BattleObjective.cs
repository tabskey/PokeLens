namespace PokeBattle.Core.Battles;

/// <summary>What the player wants to do with the target. See docs/arquitetura.md §9.</summary>
public enum BattleObjective
{
    /// <summary>Knock the target out.</summary>
    Defeat,

    /// <summary>Weaken the target without knocking it out, then throw balls.</summary>
    Catch,
}