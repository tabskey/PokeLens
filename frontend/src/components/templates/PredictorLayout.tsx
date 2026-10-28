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
  form,
  results,
}: PredictorLayoutProps<T, L>) {
  const { t } = useTranslation()

  return (
    <>
      <header className="flex flex-wrap items-center justify-between gap-3 bg-ink px-6 py-3.5 text-white">
        <h1 className="font-pixel text-[13px]">
          POKEBATTLE <span className="text-gold">PREDICTOR</span>
        </h1>
        <div className="flex gap-3">
          <div className="w-64">
            <SegmentedControl label={t('app.version')} variant="dark" options={versions} value={version} onChange={onVersionChange} />
          </div>
          <div className="w-24">
            <SegmentedControl label={t('app.language')} variant="dark" options={languages} value={language} onChange={onLanguageChange} />
          </div>
        </div>
      </header>
      <main className="mx-auto my-6 grid max-w-[1180px] gap-5 px-4 md:grid-cols-[320px_1fr]">
        <aside>{form}</aside>
        <section aria-live="polite">{results}</section>
      </main>
    </>
  )
}
