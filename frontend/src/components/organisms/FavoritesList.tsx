import { Pencil, Trash2 } from 'lucide-react'
import { useState } from 'react'

import { useTranslation } from '../../i18n/useTranslation'
import type { FavoriteRow } from '../../viewmodels/useFavoritesViewModel'
import { Tag } from '../atoms/Tag'

interface FavoritesListProps {
  rows: FavoriteRow[]
  busy: boolean
  onRename: (id: number, name: string) => Promise<void>
  onRemove: (id: number) => Promise<void>
}

const smallButton = 'cursor-pointer rounded-lg border border-line px-2.5 py-1.5 text-sm font-semibold disabled:cursor-wait disabled:opacity-60'

export function FavoritesList({ rows, busy, onRename, onRemove }: FavoritesListProps) {
  const { t } = useTranslation()
  // Only one favorite is renamed at a time; the draft is view state until it is saved.
  const [editing, setEditing] = useState<number | null>(null)
  const [draft, setDraft] = useState('')

  if (rows.length === 0) {
    return <p className="text-muted">{t('favorites.empty')}</p>
  }

  return (
    <ul className="space-y-3">
      {rows.map((row) => (
        <li key={row.id} className="rounded-[10px] border border-line bg-panel p-[18px]">
          {editing === row.id ? (
            <form
              className="flex flex-wrap gap-2"
              onSubmit={(event) => {
                event.preventDefault()
                void onRename(row.id, draft).then(() => setEditing(null))
              }}
            >
              <input
                aria-label={t('favorites.name')}
                placeholder={t('favorites.namePlaceholder')}
                maxLength={60}
                value={draft}
                onChange={(event) => setDraft(event.target.value)}
                className="min-w-0 flex-1 rounded-lg border border-line bg-surface px-2.5 py-1.5"
                autoFocus
              />
              <button type="submit" disabled={busy} className={smallButton}>
                {t('favorites.saveName')}
              </button>
              <button type="button" onClick={() => setEditing(null)} className={smallButton}>
                {t('favorites.cancel')}
              </button>
            </form>
          ) : (
            <div className="flex flex-wrap items-start justify-between gap-2">
              <h3 className="font-semibold">{row.title}</h3>
              {!row.available && <Tag tone="bad">{t('favorites.unavailable')}</Tag>}
            </div>
          )}
          <p className="mt-1 text-[13px] text-muted">{row.details}</p>
          {row.candidates && <p className="mt-1 text-sm">{t('favorites.candidates', { candidates: row.candidates })}</p>}
          <div className="mt-3 flex flex-wrap gap-2">
            {row.available && (
              <a href={row.href} className="rounded-lg bg-accent px-3 py-1.5 text-sm font-semibold text-on-status hover:brightness-110">
                {t('favorites.open')}
              </a>
            )}
            <button
              type="button"
              className={`${smallButton} flex items-center gap-1.5`}
              onClick={() => {
                setDraft(row.name)
                setEditing(row.id)
              }}
            >
              <Pencil size={14} aria-hidden="true" />
              {t('favorites.rename')}
            </button>
            <button type="button" disabled={busy} className={`${smallButton} flex items-center gap-1.5`} onClick={() => void onRemove(row.id)}>
              <Trash2 size={14} aria-hidden="true" />
              {t('favorites.delete')}
            </button>
          </div>
        </li>
      ))}
    </ul>
  )
}
