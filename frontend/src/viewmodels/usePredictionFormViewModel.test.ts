import { act, renderHook, waitFor } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import { options, response } from '../test/fixtures'
import { usePredictionFormViewModel } from './usePredictionFormViewModel'

const json = (body: unknown, status = 200) => Promise.resolve(new Response(JSON.stringify(body), { status, headers: { 'Content-Type': 'application/json' } }))

describe('usePredictionFormViewModel', () => {
  const fetchMock = vi.fn<typeof fetch>()

  beforeEach(() => {
    fetchMock.mockReset()
    fetchMock.mockImplementation((input) => (String(input).includes('/options') ? json(options) : json(response())))
    vi.stubGlobal('fetch', fetchMock)
  })

  afterEach(() => vi.unstubAllGlobals())

  const urls = () => fetchMock.mock.calls.map(([input]) => String(input))

  const loaded = async () => {
    const hook = renderHook(() => usePredictionFormViewModel())
    await waitFor(() => expect(hook.result.current.optionsLoaded).toBe(true))
    return hook
  }

  it('starts at the first milestone and only offers areas unlocked so far', async () => {
    const { result } = await loaded()

    expect(urls()).toEqual(['/api/v1/game-data/options?version=firered'])
    expect(result.current.milestone).toBe('start')
    expect(result.current.areaOptions.map((o) => o.value)).toEqual(['pallet-town-area'])

    act(() => result.current.setMilestone('boulder-badge'))

    // Uncurated areas (milestone null) are never offered.
    expect(result.current.areaOptions.map((o) => o.label)).toEqual(['Pallet Town', 'Route 1', 'Mt Moon 1F'])
  })

  it('going back in progression clears an area that is no longer open', async () => {
    const { result } = await loaded()
    act(() => result.current.setMilestone('boulder-badge'))
    act(() => result.current.setArea('mt-moon-1f'))
    act(() => result.current.setTarget('geodude'))

    act(() => result.current.setMilestone('pokedex'))

    expect(result.current.area).toBe('')
    expect(result.current.target).toBe('')
  })

  it('requires area and target before calling the API', async () => {
    const { result } = await loaded()

    await act(() => result.current.submit())

    expect(result.current.fieldErrors).toEqual({ area: ['Escolha a área.'], target: ['Escolha o Pokémon-alvo.'] })
    expect(fetchMock).toHaveBeenCalledTimes(1)
  })

  it('posts the request and keeps the response', async () => {
    const { result } = await loaded()
    act(() => result.current.setMilestone('boulder-badge'))
    act(() => result.current.setArea('mt-moon-1f'))
    act(() => result.current.setTarget('geodude'))
    act(() => result.current.setObjective('Catch'))

    await act(() => result.current.submit())

    const [, init] = fetchMock.mock.calls[1]!
    expect(JSON.parse(String(init?.body))).toEqual({
      target: 'geodude', area: 'mt-moon-1f', version: 'firered', milestone: 'boulder-badge', objective: 'Catch', scenario: 'Neutral',
    })
    expect(result.current.result?.target).toBe('geodude')
  })

  it('maps API validation errors to fields', async () => {
    const { result } = await loaded()
    fetchMock.mockImplementation(() => json({ title: 'One or more validation errors occurred.', errors: { area: ['O Pokémon-alvo não aparece nessa área nessa versão.'] } }, 400))
    act(() => result.current.setArea('pallet-town-area'))
    act(() => result.current.setTarget('bulbasaur'))

    await act(() => result.current.submit())

    expect(result.current.error).toBe('Confira os campos destacados.')
    expect(result.current.fieldErrors.area).toEqual(['O Pokémon-alvo não aparece nessa área nessa versão.'])
    expect(result.current.result).toBeNull()
  })

  it('switching version reloads the options', async () => {
    const { result } = await loaded()

    act(() => result.current.setVersion('leafgreen'))

    await waitFor(() => expect(urls()).toContain('/api/v1/game-data/options?version=leafgreen'))
  })
})
