import { useTranslation } from '../../i18n/useTranslation'
import type { AttackView } from '../../viewmodels/usePredictionResultViewModel'

/** One attack as a detail line: absolute damage first, the HP share only in parentheses. */
export function DamageRange({ who, attack, withTurns = false }: { who: string; attack: AttackView | null; withTurns?: boolean }) {
  const { t } = useTranslation()

  if (attack === null) {
    return <p className="text-xs text-muted">{who}: {t('attack.noDamagingMove')}</p>
  }

  return (
    <p className="text-xs text-muted">
      <span className="font-semibold text-ink">{who}:</span> {attack.move} {attack.damage} ({attack.hpShare})
      {withTurns && ` · ${attack.turns}`}
    </p>
  )
}
