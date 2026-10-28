import { LocateFixed, Moon, Sun } from 'lucide-react'
import type { ReactNode } from 'react'

import { useTranslation } from '../../i18n/useTranslation'
import { SegmentedControl, type Segment } from '../atoms/SegmentedControl'

interface PredictorLayoutProps<T extends string, L extends string> {
  versions: Segment<T>[]
  version: T
  onVersionChange: (version: T) => void
  languages: Segment<L>[]
  language: L
  onLanguageChange: (language: L) => void
  dark: boolean
  onToggleTheme: () => void
  form: ReactNode
  results: ReactNode
}

export function PredictorLayout<T extends string, L extends string>({
  versions,
  version,
  onVersionChange,
  languages,
  language,
  onLanguageChange,
  dark,
  onToggleTheme,
  form,
  results,
}: PredictorLayoutProps<T, L>) {
  const { t } = useTranslation()

  return (
    <>
      <header className="flex flex-wrap items-center justify-between gap-3 border-b border-line bg-header px-6 py-3.5 text-white">
        <h1 className="flex items-center gap-2 font-display text-xl font-bold">
          <LocateFixed size={22} strokeWidth={2.5} className="text-accent" aria-hidden="true" />
          PokéLens
        </h1>
        <div className="flex gap-3">
          <div className="w-64">
            <SegmentedControl label={t('app.version')} variant="dark" options={versions} value={version} onChange={onVersionChange} />
          </div>
          <div className="w-24">
            <SegmentedControl label={t('app.language')} variant="dark" options={languages} value={language} onChange={onLanguageChange} />
          </div>
          <button
            type="button"
            aria-label={t('app.darkTheme')}
            title={t('app.darkTheme')}
            aria-pressed={dark}
            onClick={onToggleTheme}
            className="grid size-[38px] shrink-0 cursor-pointer place-items-center rounded-lg border border-white/30 text-white aria-pressed:bg-white/15"
          >
            {dark ? <Sun size={16} aria-hidden="true" /> : <Moon size={16} aria-hidden="true" />}
          </button>
        </div>
      </header>
      <main className="mx-auto my-6 grid max-w-[1180px] gap-5 px-4 md:grid-cols-[320px_1fr]">
        <aside>{form}</aside>
        <section aria-live="polite">{results}</section>
      </main>
    </>
  )
}
