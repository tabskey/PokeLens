import { Star } from 'lucide-react'

import { useTranslation } from '../../i18n/useTranslation'
import type { ResultView } from '../../viewmodels/usePredictionResultViewModel'
import { Avatar } from '../atoms/Avatar'
import { Hint } from '../atoms/Hint'
import { TypeBadge } from '../atoms/TypeBadge'

/** Star on the result, only for logged-in players. */
export interface FavoriteToggle {
  saved: boolean
  busy: boolean
  error: string | null
  onToggle: () => void
}

export function TargetSummary({ result, favorite }: { result: ResultView; favorite?: FavoriteToggle }) {
  const { t } = useTranslation()

  return (
    <div className="flex items-center gap-4 rounded-[10px] border border-line bg-panel p-[18px]">
      <Avatar name={result.target} src={result.targetSprite} size="lg" />
      <div>
        <p className="text-[13px] text-muted">{t('results.wildTarget', { objective: result.objective })}</p>
        <h3 className="text-lg font-semibold">
          {result.target} <span className="text-[13px] font-normal text-muted">{result.targetLevels}</span>
        </h3>
        <div className="mt-1.5 flex flex-wrap gap-1">
          {result.targetTypes.map((type) => (
            <TypeBadge key={type} type={type} />
          ))}
        </div>
        {favorite?.error && <Hint error>{favorite.error}</Hint>}
      </div>
      {favorite && (
        <button
          type="button"
          aria-pressed={favorite.saved}
          aria-label={t(favorite.saved ? 'favorites.remove' : 'favorites.save')}
          title={t(favorite.saved ? 'favorites.remove' : 'favorites.save')}
          disabled={favorite.busy}
          onClick={favorite.onToggle}
          className="ml-auto grid size-10 shrink-0 cursor-pointer place-items-center self-start rounded-lg border border-line text-muted aria-pressed:border-neutral aria-pressed:text-neutral disabled:cursor-wait disabled:opacity-60"
        >
          <Star size={18} aria-hidden="true" fill={favorite.saved ? 'currentColor' : 'none'} />
        </button>
      )}
    </div>
  )
}
