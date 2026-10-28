import { useTranslation } from '../../i18n/useTranslation'
import type { ResultView } from '../../viewmodels/usePredictionResultViewModel'
import { CandidateCard } from './CandidateCard'
import { TargetSummary } from './TargetSummary'

export function ResultsList({ result }: { result: ResultView }) {
  const { t } = useTranslation()

  return (
    <div className="space-y-4">
      <TargetSummary result={result} />
      <div className="flex flex-wrap items-baseline justify-between gap-2">
        <h2 className="font-pixel text-[10px] text-accent">{t('results.heading')}</h2>
        <span className="text-[13px] text-muted">
          {t('results.count', { count: result.candidates.length })} · {t('results.rankedBy', { ranking: result.rankedBy })}
        </span>
      </div>
      {result.candidates.length === 0 ? (
        <p className="text-muted">{t('results.none')}</p>
      ) : (
        result.candidates.map((candidate, index) => <CandidateCard key={candidate.name} rank={index + 1} candidate={candidate} />)
      )}
      <p className="text-xs text-muted">
        {t('results.footer')}
        {result.isCatch && ` ${t('results.footerCatch')}`}
      </p>
    </div>
  )
}
