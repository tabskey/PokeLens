import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'

import type { SessionViewModel } from '../../viewmodels/useSessionViewModel'
import { AppHeader } from './AppHeader'

const header = (session: Partial<SessionViewModel>) => (
  <AppHeader
    languages={[{ value: 'pt', label: 'PT' }]}
    language="pt"
    onLanguageChange={() => {}}
    dark={false}
    onToggleTheme={() => {}}
    session={{ email: null, isAdmin: false, checked: true, logout: vi.fn(), ...session }}
  />
)

describe('AppHeader', () => {
  it('offers login to anonymous players', () => {
    render(header({}))

    expect(screen.getByRole('link', { name: 'Entrar' })).toHaveAttribute('href', '#/login')
  })

  it('shows the user, the admin link only to admins, and logs out', async () => {
    const logout = vi.fn()
    const { rerender } = render(header({ email: 'ash@example.com', logout }))
    expect(screen.queryByRole('link', { name: 'ADMIN' })).not.toBeInTheDocument()

    rerender(header({ email: 'ash@example.com', isAdmin: true, logout }))

    expect(screen.getByText('ash@example.com')).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'ADMIN' })).toHaveAttribute('href', '#/admin')
    await userEvent.click(screen.getByRole('button', { name: 'Sair' }))
    expect(logout).toHaveBeenCalledOnce()
  })
})
