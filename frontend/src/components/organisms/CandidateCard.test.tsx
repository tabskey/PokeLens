import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'

import { toResultView } from '../../viewmodels/usePredictionResultViewModel'
import { createTranslator } from '../../i18n/translator'
import { response } from '../../test/fixtures'
import { CandidateCard } from './CandidateCard'

const pt = createTranslator('pt')

describe('CandidateCard', () => {
  it('renders the candidate with its three scenarios', () => {
    render(<CandidateCard rank={1} candidate={toResultView(response(), pt).candidates[0]!} />)

    expect(screen.getByRole('article', { name: '1º Mankey' })).toBeInTheDocument()
    expect(screen.getByText('Fighting')).toBeInTheDocument()
    expect(screen.getAllByRole('region').map((region) => region.getAttribute('aria-label'))).toEqual([
      'Cenário Pessimista',
      'Cenário Neutro',
      'Cenário Otimista',
    ])
    expect(screen.getByText('★ Melhor escolha')).toBeInTheDocument()
  })

  it('marks only the first candidate as the best pick', () => {
    render(<CandidateCard rank={2} candidate={toResultView(response(), pt).candidates[0]!} />)

    expect(screen.queryByText('★ Melhor escolha')).not.toBeInTheDocument()
  })
})
