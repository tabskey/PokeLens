import { LocateFixed, LogIn, LogOut, Moon, ShieldCheck, Star, Sun } from 'lucide-react'

import { useTranslation } from '../../i18n/useTranslation'
import { routeHref } from '../../routing'
import type { SessionViewModel } from '../../viewmodels/useSessionViewModel'
import { SegmentedControl, type Segment } from '../atoms/SegmentedControl'

interface AppHeaderProps<T extends string, L extends string> {
  /** Game version picker; only the predictor shows it. */
  versions?: { options: Segment<T>[]; value: T; onChange: (version: T) => void }
  languages: Segment<L>[]
  language: L
  onLanguageChange: (language: L) => void
  dark: boolean
  onToggleTheme: () => void
  session: SessionViewModel
}

const iconButton = 'grid size-[38px] shrink-0 cursor-pointer place-items-center rounded-lg border border-white/30 text-white aria-pressed:bg-white/15'

export function AppHeader<T extends string, L extends string>({ versions, languages, language, onLanguageChange, dark, onToggleTheme, session }: AppHeaderProps<T, L>) {
  const { t } = useTranslation()

  return (
    <header className="flex flex-wrap items-center justify-between gap-3 border-b border-line bg-header px-6 py-3.5 text-white">
      <a href={routeHref('predictor')} className="flex items-center gap-2 font-display text-xl font-bold">
        <LocateFixed size={22} strokeWidth={2.5} className="text-accent" aria-hidden="true" />
        <h1>PokéLens</h1>
      </a>
      <div className="flex flex-wrap items-center gap-3">
        {versions && (
          <div className="w-64">
            <SegmentedControl label={t('app.version')} variant="dark" options={versions.options} value={versions.value} onChange={versions.onChange} />
          </div>
        )}
        <div className="w-24">
          <SegmentedControl label={t('app.language')} variant="dark" options={languages} value={language} onChange={onLanguageChange} />
        </div>
        <button type="button" aria-label={t('app.darkTheme')} title={t('app.darkTheme')} aria-pressed={dark} onClick={onToggleTheme} className={iconButton}>
          {dark ? <Sun size={16} aria-hidden="true" /> : <Moon size={16} aria-hidden="true" />}
        </button>
        {session.email ? (
          <>
            <a href={routeHref('favorites')} aria-label={t('favorites.heading')} title={t('favorites.heading')} className={iconButton}>
              <Star size={16} aria-hidden="true" />
            </a>
            {session.isAdmin && (
              <a href={routeHref('admin')} className="flex items-center gap-1.5 rounded-lg border border-white/30 px-2.5 py-2 font-mono text-[11px] font-medium tracking-wider">
                <ShieldCheck size={14} aria-hidden="true" />
                {t('app.admin')}
              </a>
            )}
            <span className="hidden max-w-48 truncate text-sm text-white/80 sm:inline" title={session.email}>
              {session.email}
            </span>
            <button type="button" aria-label={t('app.logout')} title={t('app.logout')} onClick={session.logout} className={iconButton}>
              <LogOut size={16} aria-hidden="true" />
            </button>
          </>
        ) : (
          session.checked && (
            <a href={routeHref('login')} className="flex items-center gap-1.5 rounded-lg border border-white/30 px-2.5 py-2 text-sm font-semibold">
              <LogIn size={16} aria-hidden="true" />
              {t('app.login')}
            </a>
          )
        )}
      </div>
    </header>
  )
}
