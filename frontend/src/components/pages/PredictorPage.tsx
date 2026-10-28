import { languages } from '../../i18n/translator'
import { useTranslation } from '../../i18n/useTranslation'
import { usePredictionFormViewModel } from '../../viewmodels/usePredictionFormViewModel'
import { usePredictionResultViewModel } from '../../viewmodels/usePredictionResultViewModel'
import { Spinner } from '../atoms/Spinner'
import { PredictionForm } from '../organisms/PredictionForm'
import { ResultsList } from '../organisms/ResultsList'
import { PredictorLayout } from '../templates/PredictorLayout'

const languageOptions = languages.map((value) => ({ value, label: value.toUpperCase() }))

export function PredictorPage() {
  const form = usePredictionFormViewModel()
  const result = usePredictionResultViewModel(form.result)
  const { t, language, setLanguage } = useTranslation()

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
      versions={form.versions}
      version={form.version}
      onVersionChange={form.setVersion}
      languages={languageOptions}
      language={language}
      onLanguageChange={setLanguage}
      form={<PredictionForm form={form} />}
      results={results}
    />
  )
}
