import { useTranslation } from '../../i18n/useTranslation'
import { useAuditViewModel } from '../../viewmodels/useAuditViewModel'
import { useSessionViewModel } from '../../viewmodels/useSessionViewModel'
import { Hint } from '../atoms/Hint'
import { Spinner } from '../atoms/Spinner'
import { AuditTable } from '../organisms/AuditTable'
import { PageLayout } from '../templates/PageLayout'
import { SiteHeader } from './SiteHeader'

export function AdminPage() {
  const audit = useAuditViewModel()
  const session = useSessionViewModel()
  const { t } = useTranslation()

  let content
  if (audit.loading) {
    content = <Spinner label={t('audit.loading')} />
  } else if (audit.error) {
    content = <Hint error>{audit.error}</Hint>
  } else {
    content = <AuditTable rows={audit.rows} />
  }

  return (
    <PageLayout header={<SiteHeader session={session} />} wide>
      <h2 className="mb-1 font-mono text-[11px] font-medium uppercase tracking-[0.2em] text-muted">{t('audit.heading')}</h2>
      <p className="mb-4 text-sm text-muted">{t('audit.subtitle')}</p>
      {content}
    </PageLayout>
  )
}
