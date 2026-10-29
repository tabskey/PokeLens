import type { ReactNode } from 'react'

/** Header and one centred column: login, register and admin screens. */
export function PageLayout({ header, children, wide = false }: { header: ReactNode; children: ReactNode; wide?: boolean }) {
  return (
    <>
      {header}
      <main className={`mx-auto my-6 px-4 ${wide ? 'max-w-[1180px]' : 'max-w-sm'}`}>{children}</main>
    </>
  )
}
