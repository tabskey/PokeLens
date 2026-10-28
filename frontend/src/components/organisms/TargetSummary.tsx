import { useTranslation } from '../../i18n/useTranslation'
import type { ResultView } from '../../viewmodels/usePredictionResultViewModel'
import { Avatar } from '../atoms/Avatar'
import { TypeBadge } from '../atoms/TypeBadge'

export function TargetSummary({ result }: { result: ResultView }) {
  const { t } = useTranslation()

  return (
    <div className="flex items-center gap-4 rounded-[10px] border border-line bg-panel p-[18px]">
      <Avatar name={result.target} src={result.targetSprite} size="lg" />
      <div>
        <p className="text-[13px] text-muted">{t('results.wildTarget', { objective: result.objective })}</p>
        <h3 className="text-lg font-bold">
          {result.target} <span className="text-[13px] font-normal text-muted">{result.targetLevels}</span>
        </h3>
        <div className="mt-1.5 flex flex-wrap gap-1">
          {result.targetTypes.map((type) => (
            <TypeBadge key={type} type={type} />
          ))}
        </div>
      </div>
    </div>
  )
}
