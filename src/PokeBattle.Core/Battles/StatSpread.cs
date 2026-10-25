namespace PokeBattle.Core.Battles;

/// <summary>One value per stat. Used for base stats, IVs and EVs.</summary>
public sealed record StatSpread(int Hp, int Attack, int Defense, int SpecialAttack, int SpecialDefense, int Speed)
{
    public static StatSpread Uniform(int value) => new(value, value, value, value, value, value);
}