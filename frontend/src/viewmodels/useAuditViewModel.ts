import { useEffect, useMemo, useState } from 'react'

import { ApiError, getAudit } from '../models/api'
import type { AuditItem } from '../models/auth'
import { useTranslation } from '../i18n/useTranslation'
import { areaName, methodLabel, milestoneLabel, pokemonName, versionLabels } from '../models/labels'

const limit = 100

export interface AuditRow {
  id: number
  when: string
  event: string
  user: string
  target: string
  where: string
  progress: string
  objective: string
  candidates: string
}

/** The latest predictions for admins. The backend decides who may see them (401/403 otherwise). */
export function useAuditViewModel() {
  const { t, locale } = useTranslation()
  const [items, setItems] = useState<AuditItem[] | null>(null)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    let cancelled = false
    getAudit(limit)
      .then((loaded) => {
        if (!cancelled) setItems(loaded)
      })
      .catch((reason: unknown) => {
        if (cancelled) return
        const denied = reason instanceof ApiError && (reason.status === 401 || reason.status === 403)
        setError(denied ? 'errors.forbidden' : reason instanceof ApiError ? reason.message : 'errors.unexpected')
      })
    return () => {
      cancelled = true
    }
  }, [])

  const rows = useMemo(() => {
    const date = new Intl.DateTimeFormat(locale, { dateStyle: 'short', timeStyle: 'medium' })
    return (items ?? []).map((item): AuditRow => ({
      id: item.id,
      when: date.format(new Date(item.occurredAt)),
      event: t(`audit.events.${item.event}`),
      user: item.userEmail ?? t('audit.anonymous'),
      target: pokemonName(item.target),
      where: `${areaName(item.area)} · ${methodLabel(item.method, t)}`,
      progress: `${versionLabels[item.version]} · ${milestoneLabel(item.milestone, t)}`,
      objective: t(`objective.${item.objective}`),
      candidates: item.candidates.map(pokemonName).join(', ') || '—',
    }))
  }, [items, locale, t])

  return { loading: items === null && error === null, error: error && t(error), rows }
}

export type AuditViewModel = ReturnType<typeof useAuditViewModel>
