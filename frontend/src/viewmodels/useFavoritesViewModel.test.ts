import { act, renderHook, waitFor } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import type { PredictRequest } from '../models/battle'
import type { FavoriteItem } from '../models/favorites'
import { useFavoritesViewModel } from './useFavoritesViewModel'

const json = (body: unknown, status = 200) => Promise.resolve(new Response(body === null ? null : JSON.stringify(body), { status, headers: { 'Content-Type': 'application/json' } }))

const pidgey: PredictRequest = { target: 'pidgey', area: 'kanto-route-1-area', method: 'walk', version: 'firered', milestone: 'pokedex', objective: 'Defeat', scenario: 'Neutral' }

const favorite = (id: number, overrides: Partial<FavoriteItem> = {}): FavoriteItem => ({
  id, name: null, prediction: pidgey, candidates: ['mankey', 'nidoran-f'], available: true, createdAt: '2026-10-28T12:00:00Z', ...overrides,
})

describe('useFavoritesViewModel', () => {
  const fetchMock = vi.fn<typeof fetch>()

  beforeEach(() => {
    fetchMock.mockReset()
    vi.stubGlobal('fetch', fetchMock)
  })

  afterEach(() => vi.unstubAllGlobals())

  const loaded = async (items: FavoriteItem[]) => {
    fetchMock.mockImplementation(() => json(items))
    const hook = renderHook(() => useFavoritesViewModel(true))
    await waitFor(() => expect(hook.result.current.loading).toBe(false))
    return hook
  }

  it('anonymous players load nothing', () => {
    const { result } = renderHook(() => useFavoritesViewModel(false))

    expect(result.current).toMatchObject({ enabled: false, loading: false, rows: [] })
    expect(fetchMock).not.toHaveBeenCalled()
  })

  it('describes each favorite, with the name when there is one', async () => {
    const { result } = await loaded([favorite(1, { name: 'Começo' }), favorite(2, { available: false, prediction: { ...pidgey, objective: 'Catch' } })])

    expect(result.current.rows[0]).toMatchObject({ title: 'Começo', name: 'Começo', candidates: 'Mankey, Nidoran♀', available: true })
    expect(result.current.rows[0]!.href).toBe('#/?target=pidgey&area=kanto-route-1-area&method=walk&version=firered&milestone=pokedex&objective=Defeat&scenario=Neutral')
    expect(result.current.rows[1]).toMatchObject({ title: 'Pidgey · Route 1 · grama/caverna', name: '', available: false })
    expect(result.current.rows[1]!.details).toBe('FireRed · Recebeu a Pokédex · Capturar · Equilibrado')
    expect(result.current.count).toBe('2 de 50 favoritos')
  })

  it('the star saves the prediction, then removes it', async () => {
    const { result } = await loaded([])
    fetchMock.mockImplementation((_, init) => (init?.method === 'DELETE' ? json(null, 204) : json(favorite(9), 201)))

    await act(() => result.current.toggle(pidgey))
    expect(result.current.isSaved(pidgey)).toBe(true)
    expect(JSON.parse(String(fetchMock.mock.calls[1]![1]?.body))).toEqual({ prediction: pidgey })

    await act(() => result.current.toggle({ ...pidgey }))
    expect(result.current.isSaved(pidgey)).toBe(false)
    expect(fetchMock.mock.calls[2]).toEqual(['/api/v1/favorites/9', expect.objectContaining({ method: 'DELETE' })])
  })

  it('shows the backend message when the limit is reached', async () => {
    const { result } = await loaded([])
    fetchMock.mockImplementation(() => json({ errors: { favorites: ['Você já tem 50 favoritos. Remova um para salvar outro.'] } }, 400))

    await act(() => result.current.toggle(pidgey))

    expect(result.current.error).toBe('Você já tem 50 favoritos. Remova um para salvar outro.')
    expect(result.current.isSaved(pidgey)).toBe(false)
  })

  it('renames in place', async () => {
    const { result } = await loaded([favorite(1)])
    fetchMock.mockImplementation(() => json(favorite(1, { name: 'Rota 1' })))

    await act(() => result.current.rename(1, 'Rota 1'))

    expect(result.current.rows[0]!.title).toBe('Rota 1')
    expect(fetchMock.mock.calls[1]).toEqual(['/api/v1/favorites/1', expect.objectContaining({ method: 'PATCH', body: JSON.stringify({ name: 'Rota 1' }) })])
  })
})
