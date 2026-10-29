import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'

import type { FavoriteRow } from '../../viewmodels/useFavoritesViewModel'
import { FavoritesList } from './FavoritesList'

const row = (overrides: Partial<FavoriteRow> = {}): FavoriteRow => ({
  id: 1, title: 'Pidgey · Route 1 · grama/caverna', name: '', details: 'FireRed · Recebeu a Pokédex · Derrotar · Equilibrado',
  candidates: 'Mankey', available: true, href: '#/?target=pidgey', ...overrides,
})

describe('FavoritesList', () => {
  it('opens an available favorite and marks one that is not', () => {
    render(<FavoritesList rows={[row(), row({ id: 2, title: 'Velho', available: false })]} busy={false} onRename={vi.fn()} onRemove={vi.fn()} />)

    expect(screen.getAllByRole('link', { name: 'Abrir' })).toHaveLength(1)
    expect(screen.getByRole('link', { name: 'Abrir' })).toHaveAttribute('href', '#/?target=pidgey')
    expect(screen.getByText('Não disponível mais')).toBeInTheDocument()
  })

  it('renames with the typed name and removes', async () => {
    const onRename = vi.fn(() => Promise.resolve())
    const onRemove = vi.fn(() => Promise.resolve())
    render(<FavoritesList rows={[row()]} busy={false} onRename={onRename} onRemove={onRemove} />)

    await userEvent.click(screen.getByRole('button', { name: 'Renomear' }))
    await userEvent.type(screen.getByRole('textbox', { name: 'Nome do favorito' }), 'Rota 1{Enter}')
    expect(onRename).toHaveBeenCalledWith(1, 'Rota 1')

    await userEvent.click(screen.getByRole('button', { name: 'Remover' }))
    expect(onRemove).toHaveBeenCalledWith(1)
  })

  it('explains how to add the first one', () => {
    render(<FavoritesList rows={[]} busy={false} onRename={vi.fn()} onRemove={vi.fn()} />)

    expect(screen.getByText(/Nenhum favorito ainda/)).toBeInTheDocument()
  })
})
