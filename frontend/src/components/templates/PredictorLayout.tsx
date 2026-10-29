import type { ReactNode } from 'react'

export function PredictorLayout({ header, form, results }: { header: ReactNode; form: ReactNode; results: ReactNode }) {
  return (
    <>
      {header}
      <main className="mx-auto my-6 grid max-w-[1180px] gap-5 px-4 md:grid-cols-[320px_1fr]">
        <aside>{form}</aside>
        <section aria-live="polite">{results}</section>
      </main>
    </>
  )
}
