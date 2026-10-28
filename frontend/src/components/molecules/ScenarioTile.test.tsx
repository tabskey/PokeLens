import { render, screen, within } from '@testing-library/react'
import { describe, expect, it } from 'vitest'

import { toResultView } from '../../viewmodels/usePredictionResultViewModel'
import { response } from '../../test/fixtures'
import { ScenarioTile } from './ScenarioTile'

const firstScenario = (objective: 'Defeat' | 'Catch') => toResultView(response(objective)).candidates[0]!.scenarios[0]!

describe('ScenarioTile', () => {
  it('headlines the outcome and hits to KO, and shows percentages only as HP taken', () => {
    render(<ScenarioTile scenario={firstScenario('Defeat')} />)

    const tile = screen.getByRole('region', { name: 'Cenário Pessimista' })
    expect(within(tile).getByText('Equilibrado')).toBeInTheDocument()
    expect(within(tile).getByText('3–4 golpes')).toBeInTheDocument()
    expect(within(tile).getByText('Karate Chop · detalhes').closest('details')).not.toHaveAttribute('open')
    expect(within(tile).getByText(/Karate Chop 8–10 de 25 HP \(32%–40% do HP do alvo\)/)).toBeInTheDocument()
    expect(within(tile).getByText(/Tackle 3–4 de 20 HP \(15%–20% do seu HP\) · 5–7 golpes para KO/)).toBeInTheDocument()
    expect(tile.textContent?.replace(/\d+%(–\d+%)? (do HP do alvo|do seu HP)/g, '')).not.toContain('%')
  })

  it('shows chance per ball and balls for each confidence in separate columns', () => {
    render(<ScenarioTile scenario={firstScenario('Catch')} />)

    expect(screen.getByRole('columnheader', { name: 'Chance por bola' })).toBeInTheDocument()
    expect(screen.getByRole('columnheader', { name: 'Bolas para 50 / 90 / 95 / 99%' })).toBeInTheDocument()
    const row = screen.getByRole('row', { name: /Poké Ball/ })
    expect(within(row).getByText('62,4%')).toBeInTheDocument()
    expect(within(row).getByLabelText('5 bolas para 99%')).toBeInTheDocument()
    expect(screen.getByText(/risco de KO 6,3%/)).toBeInTheDocument()
    expect(screen.getByText('Nenhum golpe seguro')).toBeInTheDocument()
    expect(screen.getByText(/1× Karate Chop/)).toBeInTheDocument()
    expect(screen.getByText('Preparação e Poké Balls · 62,4% por bola').closest('details')).not.toHaveAttribute('open')
    expect(screen.queryByText('para KO')).not.toBeInTheDocument()
  })
})
