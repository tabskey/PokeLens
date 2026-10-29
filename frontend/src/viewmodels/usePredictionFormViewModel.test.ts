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
    // Lets the fetched targets land in state.
    await act(() => new Promise((resolve) => setTimeout(resolve, 0)))
  }

  const loaded = async () => {
    const hook = renderHook(() => usePredictionFormViewModel())
    await waitFor(() => expect(hook.result.current.optionsLoaded).toBe(true))
    return hook
  }

  const pokemonIn = (result: { current: ReturnType<typeof usePredictionFormViewModel> }) =>
    result.current.search.groups.find((g) => g.label === 'Pokémon')?.options.map((o) => o.label) ?? []

  it('starts at the first milestone and searches only what is unlocked', async () => {
    const { result } = await loaded()
    await waitFor(() => expect(urls()).toEqual(['/api/v1/game-data/options', '/api/v1/targets?version=firered&milestone=start']))
    expect(result.current.milestone).toBe('start')

    await reach(result, 'pokedex')
    act(() => result.current.search.setQuery('geo'))
    expect(result.current.search.noMatches).toBe(true)

    await reach(result, 'boulder-badge')
    expect(pokemonIn(result)).toEqual(['Geodude'])
  })

  it('groups suggestions in Pokémon and routes; a route lists its Pokémon', async () => {
    const { result } = await loaded()
    await reach(result, 'boulder-badge')

    act(() => result.current.search.setQuery('mt moon'))

    expect(result.current.search.groups.map((g) => g.label)).toEqual(['Pokémon', 'Rotas'])
    expect(pokemonIn(result)).toEqual(['Zubat', 'Geodude'])
    expect(result.current.search.groups[1]!.options).toEqual([{ id: 'area:mt-moon-1f', label: 'Mt. Moon 1F' }])
    expect(result.current.search.groups[0]!.options[1]!.detail).toBe('Mt. Moon 1F, Route 24')
  })

  it('a target with one option is ready without a choice', async () => {
    const { result } = await loaded()
    await reach(result, 'boulder-badge')

    act(() => result.current.search.setQuery('zub'))
    act(() => result.current.search.pick('pokemon:zubat'))

    expect(result.current.search.query).toBe('Zubat')
    expect(result.current.search.groups).toEqual([])
    expect(result.current.search.choices).toEqual([])
    expect(result.current.search.chosen).toBe('mt-moon-1f|walk')
  })

  it('a target with several options asks where, with method, levels and chance', async () => {
    const { result } = await loaded()
    await reach(result, 'boulder-badge')

    act(() => result.current.search.pick('pokemon:geodude'))

    expect(result.current.search.chosen).toBe('')
    expect(result.current.search.choices).toEqual([
      { id: 'mt-moon-1f|walk', area: 'Mt. Moon 1F', method: 'grama/caverna', requirement: null, levels: 'Nv. 7–9', chance: '20% dos encontros' },
      { id: 'kanto-route-24-area|super-rod', area: 'Route 24', method: 'Super Vara', requirement: 'Requer Super Vara', levels: 'Nv. 15–25', chance: '20% dos encontros' },
    ])
  })

  it('picking a Pokémon after searching its route skips the choice', async () => {
    const { result } = await loaded()
    await reach(result, 'boulder-badge')

    act(() => result.current.search.pick('area:kanto-route-24-area'))
    expect(result.current.search.query).toBe('Route 24')
    act(() => result.current.search.pick('pokemon:geodude'))

    expect(result.current.search.choices).toEqual([])
    expect(result.current.search.chosen).toBe('kanto-route-24-area|super-rod')
  })

  it('going back in progression drops a target that is no longer open', async () => {
    const { result } = await loaded()
    await reach(result, 'boulder-badge')
    act(() => result.current.search.pick('pokemon:zubat'))

    await reach(result, 'pokedex')
    await waitFor(() => expect(result.current.search.chosen).toBe(''))

    await act(() => result.current.submit())
    expect(result.current.search.errors).toEqual(['Escolha o Pokémon-alvo.'])
  })

  it('requires the target, and the encounter when there are several', async () => {
    const { result } = await loaded()
    await reach(result, 'boulder-badge')

    await act(() => result.current.submit())
    expect(result.current.search.errors).toEqual(['Escolha o Pokémon-alvo.'])

    act(() => result.current.search.pick('pokemon:geodude'))
    await act(() => result.current.submit())
    expect(result.current.search.errors).toEqual(['Escolha onde encontrar o Pokémon.'])
    expect(predictCalls()).toHaveLength(0)
  })

  it('posts the chosen area and method and keeps the response', async () => {
    const { result } = await loaded()
    await reach(result, 'boulder-badge')
    act(() => result.current.search.pick('pokemon:geodude'))
    act(() => result.current.search.choose('kanto-route-24-area|super-rod'))
    act(() => result.current.setObjective('Catch'))

    await act(() => result.current.submit())

    const [, init] = predictCalls()[0]!
    expect(JSON.parse(String(init?.body))).toEqual({
      target: 'geodude', area: 'kanto-route-24-area', method: 'super-rod', version: 'firered', milestone: 'boulder-badge', objective: 'Catch', scenario: 'Neutral',
    })
    expect(result.current.result?.target).toBe('geodude')
  })

  it('shows API errors on area or method under the search', async () => {
    const { result } = await loaded()
    await reach(result, 'pokedex')
    fetchMock.mockImplementation(() => json({ title: 'One or more validation errors occurred.', errors: { method: ['Método de encontro não liberado nesse ponto de progressão.'] } }, 400))
    act(() => result.current.search.pick('pokemon:pidgey'))

    await act(() => result.current.submit())

    expect(result.current.error).toBe('Confira os campos destacados.')
    expect(result.current.search.errors).toEqual(['Método de encontro não liberado nesse ponto de progressão.'])
    expect(result.current.result).toBeNull()
  })

  it('switching version reloads the targets', async () => {
    const { result } = await loaded()

    act(() => result.current.setVersion('leafgreen'))

    await waitFor(() => expect(urls()).toContain('/api/v1/targets?version=leafgreen&milestone=start'))
  })
})
