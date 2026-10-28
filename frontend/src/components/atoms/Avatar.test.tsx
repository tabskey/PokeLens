import { fireEvent, render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'

import { Avatar } from './Avatar'

describe('Avatar', () => {
  it('shows the sprite with the Pokémon name as alt text', () => {
    render(<Avatar name="Geodude" src="https://example.test/74.png" />)

    expect(screen.getByRole('img', { name: 'Geodude' })).toHaveAttribute('src', 'https://example.test/74.png')
  })

  it('falls back to initials when the sprite fails to load', () => {
    render(<Avatar name="Geodude" src="https://example.test/74.png" />)

    fireEvent.error(screen.getByRole('img'))

    expect(screen.queryByRole('img')).not.toBeInTheDocument()
    expect(screen.getByText('GE')).toBeInTheDocument()
  })
})
