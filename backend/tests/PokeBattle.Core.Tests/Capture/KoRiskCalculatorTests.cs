using PokeBattle.Core.Capture;
using PokeBattle.Core.Damage;
using PokeBattle.Core.Types;

namespace PokeBattle.Core.Tests.Capture;

// Damage from DamageCalculatorTests case 2 (Charmander L10 Ember vs Bulbasaur L10):
//   non-critical 17 x5, 18 x5, 19 x5, 20;  critical 35, 36 x3, 37 x2, 38 x2, 39 x3, 40 x2, 41 x2, 42.
public class KoRiskCalculatorTests
{
    private static readonly Combatant Charmander = new(10, Attack: 16, Defense: 16, SpecialAttack: 22, SpecialDefense: 18, PokemonType.Fire);
    private static readonly Combatant Bulbasaur = new(10, Attack: 16, Defense: 16, SpecialAttack: 19, SpecialDefense: 19, PokemonType.Grass, PokemonType.Poison);
    private static readonly Move Ember = new(PokemonType.Fire, 40);

    [Theory]
    [InlineData(21, 0.0)]       // above the max non-critical roll (20)
    [InlineData(17, 1.0)]       // at or below the min roll: every roll KOs
    [InlineData(19, 6 / 16.0)]  // 19 x5 and 20 -> 6 of 16 rolls
    [InlineData(20, 1 / 16.0)]
    public void ChanceToKo_WithoutCriticals_CountsRolls(int currentHp, double expected)
    {
        var damage = EmberDamage(CriticalHitPolicy.Never);

        Assert.Equal(expected, KoRiskCalculator.ChanceToKo(currentHp, damage));
    }

    // HP 21: no non-critical roll KOs, every critical roll does -> 1/16 * 1 = 0.0625.
    // HP 40: critical rolls >= 40 are 40, 40, 41, 41, 42 -> 1/16 * 5/16 = 5/256.
    [Theory]
    [InlineData(CriticalHitPolicy.EvaluateProbability, 21, 1 / 16.0)]
    [InlineData(CriticalHitPolicy.Normal, 21, 1 / 16.0)]
    [InlineData(CriticalHitPolicy.EvaluateProbability, 40, 5 / 256.0)]
    [InlineData(CriticalHitPolicy.Guaranteed, 40, 5 / 16.0)]
    public void ChanceToKo_WeightsCriticalHits(CriticalHitPolicy policy, int currentHp, double expected)
    {
        Assert.Equal(expected, KoRiskCalculator.ChanceToKo(currentHp, EmberDamage(policy)), precision: 12);
    }

    [Fact]
    public void ChanceToKo_WithoutRolls_Throws()
    {
        var damage = DamageCalculator.Calculate(Charmander, Bulbasaur, Ember, DamageRollPolicy.Minimum, CriticalHitPolicy.Never);

        Assert.Throws<ArgumentException>(() => KoRiskCalculator.ChanceToKo(19, damage));
    }

    [Fact]
    public void ChanceToKo_InvalidArguments_Throw()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => KoRiskCalculator.ChanceToKo(0, EmberDamage(CriticalHitPolicy.Never)));
        Assert.Throws<ArgumentNullException>(() => KoRiskCalculator.ChanceToKo(19, null!));
    }

    private static DamageResult EmberDamage(CriticalHitPolicy policy)
        => DamageCalculator.Calculate(Charmander, Bulbasaur, Ember, DamageRollPolicy.FullRange, policy);
}