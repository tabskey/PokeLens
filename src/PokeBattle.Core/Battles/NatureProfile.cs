namespace PokeBattle.Core.Battles;

/// <summary>
/// Assumed Nature for an Estimated Nature, expressed as its effect on the offensive stat the side
/// uses (Attack for physical moves, Sp. Atk for special moves). Other stats stay neutral.
/// </summary>
public enum NatureProfile
{
    Hindering,
    Neutral,
    Favoring,
}