import { act, renderHook, waitFor } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import { options, response, targetsByMilestone } from '../test/fixtures'
import { usePredictionFormViewModel } from './usePredictionFormViewModel'

const json = (body: unknown, status = 200) => Promise.resolve(new Response(JSON.stringify(body), { status, headers: { 'Content-Type': 'application/json' } }))

describe('usePredictionFormViewModel', () => {
  const fetchMock = vi.fn<typeof fetch>()

  beforeEach(() => {
    fetchMock.mockReset()
    fetchMock.mockImplementation((input) => {
      const url = new URL(String(input), 'http://localhost')
      if (url.pathname.endsWith('/options')) return json(options)
      if (url.pathname.endsWith('/targets')) return json(targetsByMilestone[url.searchParams.get('milestone')!])
      return json(response())
    })
    vi.stubGlobal('fetch', fetchMock)
  })

  afterEach(() => vi.unstubAllGlobals())

  const urls = () => fetchMock.mock.calls.map(([input]) => String(input))
  const predictCalls = () => fetchMock.mock.calls.filter(([input]) => String(input).includes('/predict'))

  const reach = async (result: { current: ReturnType<typeof usePredictionFormViewModel> }, milestone: string) => {
    act(() => result.current.setMilestone(milestone))
    await waitFor(() => expect(urls()).toContain(`/api/v1/targets?version=${result.current.version}&milestone=${milestone}`))
    await waitFor(() => expect(result.current.areaOptions.length).toBeGreaterThan(0))
  }

  const loaded = async () => {
    const hook = renderHook(() => usePredictionFormViewModel())
    await waitFor(() => expect(hook.result.current.optionsLoaded).toBe(true))
    return hook
  }

  it('starts at the first milestone and offers the areas of the unlocked targets', async () => {
    const { result } = await loaded()

    await waitFor(() => expect(urls()).toEqual(['/api/v1/game-data/options', '/api/v1/targets?version=firered&milestone=start']))
    expect(result.current.milestone).toBe('start')
    expect(result.current.areaOptions).toEqual([])

    await reach(result, 'boulder-badge')

    expect(result.current.areaOptions.map((o) => o.label)).toEqual(['Mt. Moon 1F', 'Route 1', 'Route 24'])
    act(() => result.current.setArea('mt-moon-1f'))
    expect(result.current.targetOptions.map((o) => o.value)).toEqual(['zubat', 'geodude'])
  })

  it('going back in progression clears an area that is no longer open', async () => {
    const { result } = await loaded()
    await reach(result, 'boulder-badge')
    act(() => result.current.setArea('mt-moon-1f'))
    act(() => result.current.setTarget('geodude'))

    await reach(result, 'pokedex')
    await waitFor(() => expect(result.current.areaOptions).toHaveLength(1))

    expect(result.current.area).toBe('')
    expect(result.current.target).toBe('')
  })

  it('requires area and target before calling the API', async () => {
    const { result } = await loaded()

    await act(() => result.current.submit())

    expect(result.current.fieldErrors).toEqual({ area: ['Escolha a área.'], target: ['Escolha o Pokémon-alvo.'] })
    expect(predictCalls()).toHaveLength(0)
  })

  it('posts the request and keeps the response', async () => {
    const { result } = await loaded()
    await reach(result, 'boulder-badge')
    act(() => result.current.setArea('kanto-route-24-area'))
    act(() => result.current.setTarget('geodude'))
    act(() => result.current.setObjective('Catch'))

    await act(() => result.current.submit())

    const [, init] = predictCalls()[0]!
    expect(JSON.parse(String(init?.body))).toEqual({
      target: 'geodude', area: 'kanto-route-24-area', method: 'super-rod', version: 'firered', milestone: 'boulder-badge', objective: 'Catch', scenario: 'Neutral',
    })
    expect(result.current.result?.target).toBe('geodude')
  })

  it('maps API validation errors to fields', async () => {
    const { result } = await loaded()
    await reach(result, 'pokedex')
    fetchMock.mockImplementation(() => json({ title: 'One or more validation errors occurred.', errors: { area: ['O Pokémon-alvo não aparece nessa área nessa versão.'] } }, 400))
    act(() => result.current.setArea('kanto-route-1-area'))
    act(() => result.current.setTarget('pidgey'))

    await act(() => result.current.submit())

    expect(result.current.error).toBe('Confira os campos destacados.')
    expect(result.current.fieldErrors.area).toEqual(['O Pokémon-alvo não aparece nessa área nessa versão.'])
    expect(result.current.result).toBeNull()
  })

  it('switching version reloads the targets', async () => {
    const { result } = await loaded()

    act(() => result.current.setVersion('leafgreen'))

    await waitFor(() => expect(urls()).toContain('/api/v1/targets?version=leafgreen&milestone=start'))
  })
})
