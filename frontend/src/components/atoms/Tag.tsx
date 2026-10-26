import type { ReactNode } from 'react'

export type TagTone = 'good' | 'neutral' | 'bad' | 'plain'

const tones: Record<TagTone, string> = {
  good: 'bg-good text-white',
  neutral: 'bg-neutral text-ink',
  bad: 'bg-bad text-white',
  plain: 'border border-line text-muted',
}

export function Tag({ children, tone = 'plain' }: { children: ReactNode; tone?: TagTone }) {
  return <span className={`inline-block rounded px-2 py-0.5 text-xs font-semibold ${tones[tone]}`}>{children}</span>
}
