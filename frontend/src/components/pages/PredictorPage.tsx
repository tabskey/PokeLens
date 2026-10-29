import { useTranslation } from '../../i18n/useTranslation'
import { useFavoritesViewModel } from '../../viewmodels/useFavoritesViewModel'
import { usePredictionFormViewModel } from '../../viewmodels/usePredictionFormViewModel'
import { usePredictionResultViewModel } from '../../viewmodels/usePredictionResultViewModel'
import { useSessionViewModel } from '../../viewmodels/useSessionViewModel'
import { Spinner } from '../atoms/Spinner'
import { PredictionForm } from '../organisms/PredictionForm'
import { ResultsList } from '../organisms/ResultsList'
import { PredictorLayout } from '../templates/PredictorLayout'
import { SiteHeader } from './SiteHeader'

export function PredictorPage() {
  const form = usePredictionFormViewModel()
  const result = usePredictionResultViewModel(form.result)
  const session = useSessionViewModel()
  const favorites = useFavoritesViewModel(session.email !== null)
  const { t } = useTranslation()
  const prediction = form.prediction

  let results
  if (form.loading) {
    results = <Spinner label={t('page.simulating')} />
  } else if (result) {
    results = (
      <ResultsList
        result={result}
        favorite={
          favorites.enabled && prediction
            ? { saved: favorites.isSaved(prediction), busy: favorites.busy, error: favorites.error, onToggle: () => void favorites.toggle(prediction) }
            : undefined
        }
      />
    )
  } else {
    results = <p className="text-muted">{t('page.empty')}</p>
  }

  return (
    <PredictorLayout
      header={<SiteHeader session={session} versions={{ options: form.versions, value: form.version, onChange: form.setVersion }} />}
      form={<PredictionForm form={form} />}
      results={results}
    />
  )
}
