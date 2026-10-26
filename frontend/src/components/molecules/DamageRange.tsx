import type { AttackView } from '../../viewmodels/usePredictionResultViewModel'

/** One attack as numbers: damage range and remaining HP are details, not a win chance. */
export function DamageRange({ who, attack, showBar = false }: { who: string; attack: AttackView | null; showBar?: boolean }) {
  if (attack === null) {
    return <p className="text-xs text-muted">{who}: nenhum golpe de dano</p>
  }

  return (
    <div className="text-xs text-muted">
      <p>
        <span className="font-semibold text-ink">{who}:</span> {attack.move} tira <span className="font-semibold text-ink">{attack.hpShare}</span> · {attack.turns}
        <br />
        dano {attack.damage} · sobra {attack.remainingHp}
      </p>
      {showBar && (
        // Solid up to the weakest roll, lighter up to the strongest: the bar is HP taken, not a chance.
        <div className="relative mt-1 h-1.5 overflow-hidden rounded bg-line" aria-hidden="true">
          <div className="absolute inset-y-0 left-0 bg-ink/30" style={{ width: `${attack.bar.max}%` }} />
          <div className="absolute inset-y-0 left-0 bg-ink" style={{ width: `${attack.bar.min}%` }} />
        </div>
      )}
    </div>
  )
}
