import { usePredictionFormViewModel } from '../../viewmodels/usePredictionFormViewModel'
import { usePredictionResultViewModel } from '../../viewmodels/usePredictionResultViewModel'
import { Spinner } from '../atoms/Spinner'
import { PredictionForm } from '../organisms/PredictionForm'
import { ResultsList } from '../organisms/ResultsList'
import { PredictorLayout } from '../templates/PredictorLayout'

export function PredictorPage() {
  const form = usePredictionFormViewModel()
  const result = usePredictionResultViewModel(form.result)

  let results
  if (form.loading) {
    results = <Spinner label="Simulando os três cenários…" />
  } else if (result) {
    results = <ResultsList result={result} />
  } else {
    results = <p className="text-muted">Escolha o alvo e até onde você chegou para ver as melhores opções.</p>
  }

  return <PredictorLayout versions={form.versions} version={form.version} onVersionChange={form.setVersion} form={<PredictionForm form={form} />} results={results} />
}
