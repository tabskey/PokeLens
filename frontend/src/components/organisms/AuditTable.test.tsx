import { render, screen, within } from '@testing-library/react'
import { describe, expect, it } from 'vitest'

import { AuditTable } from './AuditTable'

describe('AuditTable', () => {
  it('lists one row per entry under the column headers', () => {
    render(
      <AuditTable
        rows={[{ id: 1, when: '28/10/2026 12:00:00', event: 'Concluído', user: 'Anônimo', target: 'Pidgey', where: 'Route 1 · grama/caverna', progress: 'FireRed · Recebeu a Pokédex', objective: 'Derrotar', candidates: 'Mankey' }]}
      />,
    )

    const table = screen.getByRole('table', { name: 'AUDITORIA' })
    expect(within(table).getAllByRole('columnheader').map((th) => th.textContent)).toEqual(['Quando', 'Evento', 'Usuário', 'Alvo', 'Onde', 'Progresso', 'Objetivo', 'Candidatos'])
    expect(within(table).getByRole('row', { name: /Pidgey/ })).toHaveTextContent('Mankey')
  })

  it('says when there is nothing recorded', () => {
    render(<AuditTable rows={[]} />)

    expect(screen.getByText('Nenhuma previsão registrada ainda.')).toBeInTheDocument()
  })
})
