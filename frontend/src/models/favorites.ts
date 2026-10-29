// Mirrors the backend favorites contracts (FavoritesContracts, FavoritesEndpoints).
import type { PredictRequest } from './battle'

export const favoritesLimit = 50

/** A saved prediction: the question, run again when opened. */
export interface FavoriteItem {
  id: number
  /** Set by the player; null shows the automatic description. */
  name: string | null
  prediction: PredictRequest
  /** Candidate names when saved, only to describe it. */
  candidates: string[]
  /** False when the target can no longer be met that way at that milestone. */
  available: boolean
  createdAt: string
}

export const samePrediction = (a: PredictRequest, b: PredictRequest): boolean =>
  a.target === b.target && a.area === b.area && a.method === b.method && a.version === b.version &&
  a.milestone === b.milestone && a.objective === b.objective && a.scenario === b.scenario
