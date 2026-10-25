namespace PokeBattle.Core.Battles;

/// <summary>
/// Where an attribute's value comes from. Only <see cref="Estimated"/> attributes vary between
/// battle scenarios; Known and UserProvided values are used as they are.
/// </summary>
public enum DataSource
{
    /// <summary>Fixed by the game (e.g. a static encounter's level).</summary>
    Known,

    /// <summary>Informed by the user (V2: personal Pokédex).</summary>
    UserProvided,

    /// <summary>Unknown; each scenario assumes a value (e.g. a wild Pokémon's IVs).</summary>
    Estimated,
}