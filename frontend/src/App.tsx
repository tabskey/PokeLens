import { PredictorPage } from './components/pages/PredictorPage'
import { LanguageProvider } from './i18n/LanguageProvider'

export default function App() {
  return (
    <LanguageProvider>
      <PredictorPage />
    </LanguageProvider>
  )
}
