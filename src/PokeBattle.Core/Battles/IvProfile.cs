namespace PokeBattle.Core.Battles;

/// <summary>Assumed IVs for Estimated IVs. Concrete values live in <see cref="ScenarioCatalog"/>.</summary>
public enum IvProfile
{
    Low,
    Average,
    High,
    Maximum,
}