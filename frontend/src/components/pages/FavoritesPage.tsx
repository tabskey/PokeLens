import { useTranslation } from '../../i18n/useTranslation'
import { routeHref } from '../../routing'
import { useFavoritesViewModel } from '../../viewmodels/useFavoritesViewModel'
import { useSessionViewModel } from '../../viewmodels/useSessionViewModel'
import { Hint } from '../atoms/Hint'
import { Spinner } from '../atoms/Spinner'
import { FavoritesList } from '../organisms/FavoritesList'
import { PageLayout } from '../templates/PageLayout'
import { SiteHeader } from './SiteHeader'

export function FavoritesPage() {
  const session = useSessionViewModel()
  const favorites = useFavoritesViewModel(session.email !== null)
  const { t } = useTranslation()

  let content
  if (!session.checked || favorites.loading) {
    content = <Spinner label={t('favorites.loading')} />
  } else if (!favorites.enabled) {
    content = (
      <p className="text-muted">
        <a href={routeHref('login')} className="font-semibold text-accent hover:underline">
          {t('favorites.loginToSee')}
        </a>
      </p>
    )
  } else {
    content = <FavoritesList rows={favorites.rows} busy={favorites.busy} onRename={favorites.rename} onRemove={favorites.remove} />
  }

  return (
    <PageLayout header={<SiteHeader session={session} />} wide>
      <div className="mx-auto max-w-2xl">
        <h2 className="mb-1 font-mono text-[11px] font-medium uppercase tracking-[0.2em] text-muted">{t('favorites.heading')}</h2>
        {favorites.enabled && <p className="mb-4 text-sm text-muted">{favorites.count}</p>}
        {favorites.error && <Hint error>{favorites.error}</Hint>}
        {content}
      </div>
    </PageLayout>
  )
}
