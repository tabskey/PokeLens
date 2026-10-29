import { useEffect, useMemo, useState } from 'react'

import { ApiError, deleteFavorite, getFavorites, renameFavorite, saveFavorite } from '../models/api'
import type { PredictRequest } from '../models/battle'
import { favoritesLimit, samePrediction, type FavoriteItem } from '../models/favorites'
import { useTranslation } from '../i18n/useTranslation'
import { areaName, methodLabel, milestoneLabel, pokemonName, versionLabels } from '../models/labels'
import { predictionHref } from '../routing'

export interface FavoriteRow {
  id: number
  /** The player's name, or the automatic description ("Pidgey · Route 1 · grama/caverna"). */
  title: string
  /** The name as typed, for the rename field; empty when it is the automatic description. */
  name: string
  details: string
  candidates: string
  available: boolean
  /** Opens the predictor with this prediction filled in and run. */
  href: string
}

/**
 * The logged-in player's saved predictions. `enabled` is false for anonymous players: nothing is
 * loaded and the star is not offered.
 */
export function useFavoritesViewModel(enabled: boolean) {
  const { t } = useTranslation()
  const [items, setItems] = useState<FavoriteItem[] | null>(null)
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    if (!enabled) {
      return
    }
    let cancelled = false
    getFavorites()
      .then((loaded) => {
        if (!cancelled) setItems(loaded)
      })
      .catch((reason: unknown) => {
        if (!cancelled) setError(reason instanceof ApiError ? reason.message : 'errors.unexpected')
      })
    return () => {
      cancelled = true
    }
  }, [enabled])

  // Each action keeps the list in step locally instead of loading it again.
  const act = async (action: () => Promise<(list: FavoriteItem[]) => FavoriteItem[]>) => {
    setBusy(true)
    setError(null)
    try {
      const update = await action()
      setItems((list) => update(list ?? []))
    } catch (reason) {
      // Field errors (limit, name) are already written by the backend.
      setError(reason instanceof ApiError ? (Object.values(reason.fieldErrors)[0]?.[0] ?? reason.message) : 'errors.unexpected')
    } finally {
      setBusy(false)
    }
  }

  const savedId = (prediction: PredictRequest | null) => (prediction && items?.find((item) => samePrediction(item.prediction, prediction))?.id) ?? null

  const remove = (id: number) => act(async () => {
    await deleteFavorite(id)
    return (list) => list.filter((item) => item.id !== id)
  })

  const rename = (id: number, name: string) => act(async () => {
    const renamed = await renameFavorite(id, name)
    return (list) => list.map((item) => (item.id === id ? renamed : item))
  })

  /** Star on the result: saves the prediction, or removes it when it is already a favorite. */
  const toggle = (prediction: PredictRequest) => {
    const id = savedId(prediction)
    return id === null
      ? act(async () => {
          const saved = await saveFavorite(prediction)
          return (list) => [saved, ...list.filter((item) => item.id !== saved.id)]
        })
      : remove(id)
  }

  const rows = useMemo(
    () => (items ?? []).map((item): FavoriteRow => {
      const { target, area, method, version, milestone, objective, scenario } = item.prediction
      return {
        id: item.id,
        title: item.name ?? `${pokemonName(target)} · ${areaName(area)} · ${methodLabel(method, t)}`,
        name: item.name ?? '',
        details: `${versionLabels[version]} · ${milestoneLabel(milestone, t)} · ${t(`objective.${objective}`)} · ${t(`ranking.${scenario}`)}`,
        candidates: item.candidates.map(pokemonName).join(', '),
        available: item.available,
        href: predictionHref(item.prediction),
      }
    }),
    [items, t],
  )

  return {
    enabled,
    loading: enabled && items === null && error === null,
    busy,
    error: error && t(error),
    rows,
    count: t('favorites.count', { count: rows.length, limit: favoritesLimit }),
    isSaved: (prediction: PredictRequest | null) => savedId(prediction) !== null,
    toggle,
    remove,
    rename,
  }
}

export type FavoritesViewModel = ReturnType<typeof useFavoritesViewModel>
