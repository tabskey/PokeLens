import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'

import type { TargetSearchViewModel } from '../../viewmodels/usePredictionFormViewModel'
import { TargetSearch } from './TargetSearch'

const search = (overrides: Partial<TargetSearchViewModel> = {}): TargetSearchViewModel => ({
  query: 'mt',
  groups: [
    { label: 'Pokémon', options: [{ id: 'pokemon:zubat', label: 'Zubat', detail: 'Mt. Moon 1F' }] },
    { label: 'Rotas', options: [{ id: 'area:mt-moon-1f', label: 'Mt. Moon 1F' }] },
  ],
  noMatches: false,
  choices: [],
  chosen: '',
  errors: [],
  setQuery: vi.fn(),
  pick: vi.fn(),
  choose: vi.fn(),
  ...overrides,
})

describe('TargetSearch', () => {
  it('is a combobox with suggestions grouped in Pokémon and routes', async () => {
    render(<TargetSearch search={search()} />)

    await userEvent.click(screen.getByRole('combobox', { name: 'Pokémon-alvo' }))

    expect(screen.getByRole('group', { name: 'Pokémon' })).toHaveTextContent('Zubat')
    expect(screen.getByRole('group', { name: 'Rotas' })).toHaveTextContent('Mt. Moon 1F')
  })

  it('moves with the arrows and picks with Enter', async () => {
    const vm = search()
    render(<TargetSearch search={vm} />)
    const input = screen.getByRole('combobox')

    await userEvent.click(input)
    await userEvent.keyboard('{ArrowDown}')
    expect(screen.getByRole('option', { name: /Mt\. Moon 1F/, selected: true })).toHaveAttribute('id', input.getAttribute('aria-activedescendant'))

    await userEvent.keyboard('{Enter}')
    expect(vm.pick).toHaveBeenCalledWith('area:mt-moon-1f')
  })

  it('closes with Escape', async () => {
    render(<TargetSearch search={search()} />)
    const input = screen.getByRole('combobox')

    await userEvent.click(input)
    expect(input).toHaveAttribute('aria-expanded', 'true')
    await userEvent.keyboard('{Escape}')

    expect(input).toHaveAttribute('aria-expanded', 'false')
  })

  it('asks where to find the target with method tag, levels and chance', async () => {
    const vm = search({
      groups: [],
      choices: [
        { id: 'mt-moon-1f|walk', area: 'Mt. Moon 1F', method: 'grama/caverna', requirement: null, levels: 'Nv. 7–9', chance: '20% dos encontros' },
        { id: 'kanto-route-24-area|super-rod', area: 'Route 24', method: 'Super Vara', requirement: 'Requer Super Vara', levels: 'Nv. 15–25', chance: '20% dos encontros' },
      ],
    })
    render(<TargetSearch search={vm} />)

    const choices = screen.getByRole('group', { name: 'Onde você vai encontrá-lo?' })
    expect(choices).toHaveTextContent('Requer Super Vara')
    expect(choices).toHaveTextContent('Nv. 15–25 · 20% dos encontros')

    await userEvent.click(screen.getByRole('radio', { name: /Route 24/ }))
    expect(vm.choose).toHaveBeenCalledWith('kanto-route-24-area|super-rod')
  })

  it('shows errors on the field', () => {
    render(<TargetSearch search={search({ errors: ['Escolha o Pokémon-alvo.'] })} />)

    expect(screen.getByRole('combobox')).toHaveAttribute('aria-invalid', 'true')
    expect(screen.getByRole('alert')).toHaveTextContent('Escolha o Pokémon-alvo.')
  })
})
