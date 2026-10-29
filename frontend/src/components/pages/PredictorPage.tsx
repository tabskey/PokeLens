import { useTranslation } from '../../i18n/useTranslation'
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
  const { t } = useTranslation()

  let results
  if (form.loading) {
    results = <Spinner label={t('page.simulating')} />
  } else if (result) {
    results = <ResultsList result={result} />
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
