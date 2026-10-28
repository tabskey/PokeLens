import { fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it } from 'vitest'

import { LanguageProvider } from './LanguageProvider'
import { useTranslation } from './useTranslation'

function Probe() {
  const { t, setLanguage } = useTranslation()
  return (
    <button type="button" onClick={() => setLanguage('en')}>
      {t('form.heading')}
    </button>
  )
}

describe('LanguageProvider', () => {
  afterEach(() => localStorage.clear())

  it('switches the texts, the page language and remembers the choice', () => {
    localStorage.setItem('language', 'pt')
    render(
      <LanguageProvider>
        <Probe />
      </LanguageProvider>,
    )
    expect(screen.getByRole('button')).toHaveTextContent('SUA BATALHA')

    fireEvent.click(screen.getByRole('button'))

    expect(screen.getByRole('button')).toHaveTextContent('YOUR BATTLE')
    expect(document.documentElement.lang).toBe('en-US')
    expect(localStorage.getItem('language')).toBe('en')
  })
})
