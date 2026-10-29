import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'

import { createTranslator } from '../../i18n/translator'
import { response } from '../../test/fixtures'
import { toResultView } from '../../viewmodels/usePredictionResultViewModel'
import { TargetSummary } from './TargetSummary'

const result = toResultView(response(), createTranslator('pt'))

describe('TargetSummary', () => {
  it('has no star for anonymous players', () => {
    render(<TargetSummary result={result} />)

    expect(screen.queryByRole('button')).not.toBeInTheDocument()
  })

  it('the star shows whether the prediction is saved and toggles it', async () => {
    const onToggle = vi.fn()
    render(<TargetSummary result={result} favorite={{ saved: true, busy: false, error: null, onToggle }} />)

    const star = screen.getByRole('button', { name: 'Remover dos favoritos' })
    expect(star).toHaveAttribute('aria-pressed', 'true')
    await userEvent.click(star)
    expect(onToggle).toHaveBeenCalledOnce()
  })
})
