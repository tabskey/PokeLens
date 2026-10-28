import type { ReactNode } from 'react'

export type TagTone = 'good' | 'neutral' | 'bad' | 'plain'

const tones: Record<TagTone, string> = {
  good: 'bg-good text-on-status',
  // Yellow needs dark text in both themes.
  neutral: 'bg-neutral text-[#1f2328]',
  bad: 'bg-bad text-on-status',
  plain: 'border border-line text-muted',
}

export function Tag({ children, tone = 'plain' }: { children: ReactNode; tone?: TagTone }) {
  return <span className={`inline-block rounded px-2 py-0.5 text-xs font-semibold ${tones[tone]}`}>{children}</span>
}
