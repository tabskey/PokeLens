import type { ReactNode } from 'react'

import { SegmentedControl, type Segment } from '../atoms/SegmentedControl'

interface PredictorLayoutProps<T extends string> {
  versions: Segment<T>[]
  version: T
  onVersionChange: (version: T) => void
  form: ReactNode
  results: ReactNode
}

export function PredictorLayout<T extends string>({ versions, version, onVersionChange, form, results }: PredictorLayoutProps<T>) {
  return (
    <>
      <header className="flex flex-wrap items-center justify-between gap-3 bg-ink px-6 py-3.5 text-white">
        <h1 className="font-pixel text-[13px]">
          POKEBATTLE <span className="text-gold">PREDICTOR</span>
        </h1>
        <div className="w-64">
          <SegmentedControl label="Versão do jogo" variant="dark" options={versions} value={version} onChange={onVersionChange} />
        </div>
      </header>
      <main className="mx-auto my-6 grid max-w-[1180px] gap-5 px-4 md:grid-cols-[320px_1fr]">
        <aside>{form}</aside>
        <section aria-live="polite">{results}</section>
      </main>
    </>
  )
}
