import type { ReactNode } from 'react'

export function Hint({ children, error = false, id }: { children: ReactNode; error?: boolean; id?: string }) {
  return (
    <p id={id} className={`mt-1.5 text-xs ${error ? 'text-bad' : 'text-muted'}`} role={error ? 'alert' : undefined}>
      {children}
    </p>
  )
}
