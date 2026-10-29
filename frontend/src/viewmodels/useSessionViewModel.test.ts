import { act, renderHook, waitFor } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import { useSessionViewModel } from './useSessionViewModel'

const json = (body: unknown, status = 200) => Promise.resolve(new Response(JSON.stringify(body), { status, headers: { 'Content-Type': 'application/json' } }))

describe('useSessionViewModel', () => {
  const fetchMock = vi.fn<typeof fetch>()

  beforeEach(() => {
    fetchMock.mockReset()
    vi.stubGlobal('fetch', fetchMock)
  })

  afterEach(() => {
    vi.unstubAllGlobals()
    localStorage.clear()
  })

  it('without a token the player is anonymous and nothing is called', () => {
    const { result } = renderHook(() => useSessionViewModel())

    expect(result.current).toMatchObject({ email: null, isAdmin: false, checked: true })
    expect(fetchMock).not.toHaveBeenCalled()
  })

  it('restores the user and the admin role from the stored token', async () => {
    localStorage.setItem('token', 'jwt')
    fetchMock.mockImplementation(() => json({ id: '1', email: 'ash@example.com', roles: ['Admin'] }))

    const { result } = renderHook(() => useSessionViewModel())

    await waitFor(() => expect(result.current.checked).toBe(true))
    expect(result.current).toMatchObject({ email: 'ash@example.com', isAdmin: true })
    const [url, init] = fetchMock.mock.calls[0]!
    expect(url).toBe('/api/v1/auth/me')
    expect(new Headers(init?.headers).get('Authorization')).toBe('Bearer jwt')
  })

  it('drops an expired token', async () => {
    localStorage.setItem('token', 'expired')
    fetchMock.mockImplementation(() => json({}, 401))

    const { result } = renderHook(() => useSessionViewModel())

    await waitFor(() => expect(result.current.checked).toBe(true))
    expect(result.current.email).toBeNull()
    expect(localStorage.getItem('token')).toBeNull()
  })

  it('logs out', async () => {
    localStorage.setItem('token', 'jwt')
    fetchMock.mockImplementation(() => json({ id: '1', email: 'ash@example.com', roles: [] }))
    const { result } = renderHook(() => useSessionViewModel())
    await waitFor(() => expect(result.current.email).toBe('ash@example.com'))

    act(() => result.current.logout())

    expect(result.current.email).toBeNull()
    expect(localStorage.getItem('token')).toBeNull()
  })
})
