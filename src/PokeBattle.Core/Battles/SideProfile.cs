namespace PokeBattle.Core.Battles;

/// <summary>The conditions a scenario assumes for one side of the battle.</summary>
public sealed record SideProfile(NatureProfile Nature, IvProfile Iv, EvProfile Ev, LevelProfile Level);