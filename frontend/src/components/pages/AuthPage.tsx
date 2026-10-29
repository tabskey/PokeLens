import { navigate } from '../../routing'
import { useAuthFormViewModel, type AuthMode } from '../../viewmodels/useAuthFormViewModel'
import { useSessionViewModel } from '../../viewmodels/useSessionViewModel'
import { AuthForm } from '../organisms/AuthForm'
import { PageLayout } from '../templates/PageLayout'
import { SiteHeader } from './SiteHeader'

export function AuthPage({ mode }: { mode: AuthMode }) {
  const form = useAuthFormViewModel(mode, () => navigate('predictor'))
  const session = useSessionViewModel()

  return (
    <PageLayout header={<SiteHeader session={session} />}>
      <AuthForm form={form} />
    </PageLayout>
  )
}
