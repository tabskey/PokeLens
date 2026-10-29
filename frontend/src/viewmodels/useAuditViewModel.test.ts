import { renderHook, waitFor } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import type { AuditItem } from '../models/auth'
import { useAuditViewModel } from './useAuditViewModel'

const json = (body: unknown, status = 200) => Promise.resolve(new Response(JSON.stringify(body), { status, headers: { 'Content-Type': 'application/json' } }))

const item: AuditItem = {
  id: 7,
  event: 'BattlePredictionCompleted',
  userEmail: null,
  target: 'mr-mime',
  area: 'kanto-route-1-area',
  method: 'walk',
  version: 'leafgreen',
  milestone: 'pokedex',
  objective: 'Catch',
  candidates: ['pidgey', 'nidoran-f'],
  occurredAt: '2026-10-28T12:00:00Z',
}

describe('useAuditViewModel', () => {
  const fetchMock = vi.fn<typeof fetch>()

  beforeEach(() => {
    fetchMock.mockReset()
    vi.stubGlobal('fetch', fetchMock)
  })

  afterEach(() => vi.unstubAllGlobals())

  it('formats each entry for the table', async () => {
    fetchMock.mockImplementation(() => json([item]))

    const { result } = renderHook(() => useAuditViewModel())

    await waitFor(() => expect(result.current.loading).toBe(false))
    expect(fetchMock.mock.calls[0]![0]).toBe('/api/v1/admin/audit?limit=100')
    expect(result.current.rows[0]).toMatchObject({
      id: 7,
      event: 'Concluído',
      user: 'Anônimo',
      target: 'Mr. Mime',
      where: 'Route 1 · grama/caverna',
      progress: 'LeafGreen · Recebeu a Pokédex',
      objective: 'Capturar',
      candidates: 'Pidgey, Nidoran♀',
    })
  })

  it.each([401, 403])('without the admin role (%i) shows that the page is for admins', async (status) => {
    fetchMock.mockImplementation(() => json({}, status))

    const { result } = renderHook(() => useAuditViewModel())

    await waitFor(() => expect(result.current.error).toBe('Só administradores veem esta página.'))
    expect(result.current.rows).toEqual([])
  })
})
