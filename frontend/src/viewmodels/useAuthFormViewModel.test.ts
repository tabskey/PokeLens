import { act, renderHook } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import { useAuthFormViewModel } from './useAuthFormViewModel'

const json = (body: unknown, status = 200) => Promise.resolve(new Response(body === null ? null : JSON.stringify(body), { status, headers: { 'Content-Type': 'application/json' } }))

describe('useAuthFormViewModel', () => {
  const fetchMock = vi.fn<typeof fetch>()
  const onDone = vi.fn()

  beforeEach(() => {
    fetchMock.mockReset()
    onDone.mockReset()
    vi.stubGlobal('fetch', fetchMock)
  })

  afterEach(() => {
    vi.unstubAllGlobals()
    localStorage.clear()
  })

  const filled = (mode: 'login' | 'register') => {
    const hook = renderHook(() => useAuthFormViewModel(mode, onDone))
    act(() => hook.result.current.setEmail(' ash@example.com '))
    act(() => hook.result.current.setPassword('pikachu-123'))
    return hook
  }

  it('requires e-mail and password before calling the API', async () => {
    const { result } = renderHook(() => useAuthFormViewModel('login', onDone))

    await act(() => result.current.submit())

    expect(result.current.fieldErrors).toEqual({ email: ['Informe o e-mail.'], password: ['Informe a senha.'] })
    expect(fetchMock).not.toHaveBeenCalled()
  })

  it('registering logs in right away and stores the token', async () => {
    fetchMock.mockImplementation((input) => (String(input).endsWith('/register') ? json(null, 201) : json({ accessToken: 'jwt', expiresAt: '' })))
    const { result } = filled('register')

    await act(() => result.current.submit())

    expect(fetchMock.mock.calls.map(([url]) => url)).toEqual(['/api/v1/auth/register', '/api/v1/auth/login'])
    expect(JSON.parse(String(fetchMock.mock.calls[0]![1]?.body))).toEqual({ email: 'ash@example.com', password: 'pikachu-123' })
    expect(localStorage.getItem('token')).toBe('jwt')
    expect(onDone).toHaveBeenCalledOnce()
  })

  it('shows the backend message for a wrong password', async () => {
    fetchMock.mockImplementation(() => json({ title: 'E-mail ou senha inválidos.' }, 401))
    const { result } = filled('login')

    await act(() => result.current.submit())

    expect(result.current.error).toBe('E-mail ou senha inválidos.')
    expect(localStorage.getItem('token')).toBeNull()
    expect(onDone).not.toHaveBeenCalled()
  })

  it('maps register validation errors to the fields', async () => {
    fetchMock.mockImplementation(() => json({ errors: { email: ['Já existe uma conta com esse e-mail.'] } }, 400))
    const { result } = filled('register')

    await act(() => result.current.submit())

    expect(result.current.fieldErrors.email).toEqual(['Já existe uma conta com esse e-mail.'])
    expect(result.current.error).toBe('Confira os campos destacados.')
  })
})
