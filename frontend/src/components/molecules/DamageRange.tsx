import type { AttackView } from '../../viewmodels/usePredictionResultViewModel'

/** One attack as numbers: damage range and remaining HP are details, not a win chance. */
export function DamageRange({ who, attack }: { who: string; attack: AttackView | null }) {
  if (attack === null) {
    return <p className="text-xs text-muted">{who}: nenhum golpe de dano</p>
  }

  return (
    <p className="text-xs text-muted">
      <span className="font-semibold text-ink">{who}:</span> {attack.move} · {attack.turns}
      <br />
      dano {attack.damage} · sobra {attack.remainingHp}
    </p>
  )
}
