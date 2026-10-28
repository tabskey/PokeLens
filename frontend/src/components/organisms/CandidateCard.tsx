import { useTranslation } from '../../i18n/useTranslation'
import type { CandidateView } from '../../viewmodels/usePredictionResultViewModel'
import { Avatar } from '../atoms/Avatar'
import { Tag } from '../atoms/Tag'
import { TypeBadge } from '../atoms/TypeBadge'
import { ScenarioTile } from '../molecules/ScenarioTile'

export function CandidateCard({ rank, candidate }: { rank: number; candidate: CandidateView }) {
  // The list comes ranked from the API, so the first card is the best pick.
  const best = rank === 1
  const { t } = useTranslation()

  return (
    <article
      className={`rounded-[10px] border bg-panel p-[18px] ${best ? 'border-2 border-good' : 'border-line'}`}
      aria-label={t('results.rank', { rank, name: candidate.name })}
    >
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div className="flex items-center gap-3">
          <Avatar name={candidate.name} src={candidate.sprite} />
          <div>
            <h4 className="text-base font-semibold">
              <span className="text-muted">{rank}.</span> {candidate.name} {best && <Tag tone="good">{t('results.best')}</Tag>}
            </h4>
            <p className="text-xs text-muted">{candidate.where}</p>
          </div>
        </div>
        <div className="flex flex-wrap gap-1">
          {candidate.types.map((type) => (
            <TypeBadge key={type} type={type} />
          ))}
        </div>
      </div>
      <div className="mt-3 grid gap-2 md:grid-cols-3">
        {candidate.scenarios.map((scenario) => (
          <ScenarioTile key={scenario.kind} scenario={scenario} />
        ))}
      </div>
    </article>
  )
}
