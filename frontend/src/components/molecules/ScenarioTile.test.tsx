import { render, screen, within } from '@testing-library/react'
import { describe, expect, it } from 'vitest'

import { toResultView } from '../../viewmodels/usePredictionResultViewModel'
import { response } from '../../test/fixtures'
import { ScenarioTile } from './ScenarioTile'

const firstScenario = (objective: 'Defeat' | 'Catch') => toResultView(response(objective)).candidates[0]!.scenarios[0]!

describe('ScenarioTile', () => {
  it('headlines the outcome and shows percentages only as HP taken', () => {
    render(<ScenarioTile scenario={firstScenario('Defeat')} />)

    const tile = screen.getByRole('region', { name: 'Cenário Pessimista' })
    expect(within(tile).getByText('Equilibrado')).toBeInTheDocument()
    expect(within(tile).getByText(/dano 8–10 de 25 HP/)).toBeInTheDocument()
    expect(within(tile).getAllByText(/% do HP$/)).toHaveLength(2)
    expect(tile.textContent?.replace(/\d+%(–\d+%)? do HP/g, '')).not.toContain('%')
  })

  it('shows chance per ball and balls for each confidence in separate columns', () => {
    render(<ScenarioTile scenario={firstScenario('Catch')} />)

    expect(screen.getByRole('columnheader', { name: 'Chance por bola' })).toBeInTheDocument()
    expect(screen.getByRole('columnheader', { name: 'Bolas para 50 / 90 / 95 / 99%' })).toBeInTheDocument()
    const row = screen.getByRole('row', { name: /Poké Ball/ })
    expect(within(row).getByText('62,4%')).toBeInTheDocument()
    expect(within(row).getByLabelText('5 bolas para 99%')).toBeInTheDocument()
    expect(screen.getByText(/risco de KO 6,3%/)).toBeInTheDocument()
  })
})
