import { AdminPage } from './components/pages/AdminPage'
import { AuthPage } from './components/pages/AuthPage'
import { FavoritesPage } from './components/pages/FavoritesPage'
import { PredictorPage } from './components/pages/PredictorPage'
import { LanguageProvider } from './i18n/LanguageProvider'
import { useRoute } from './routing'

export default function App() {
  const route = useRoute()

  return (
    <LanguageProvider>
      {route === 'login' && <AuthPage key="login" mode="login" />}
      {route === 'register' && <AuthPage key="register" mode="register" />}
      {route === 'admin' && <AdminPage />}
      {route === 'favorites' && <FavoritesPage />}
      {route === 'predictor' && <PredictorPage />}
    </LanguageProvider>
  )
}
