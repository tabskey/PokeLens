// Mirrors the backend auth and admin contracts (AuthEndpoints, AdminEndpoints, AuditLogItem).
import type { Objective, Version } from './battle'

export const adminRole = 'Admin'

export interface TokenResponse {
  accessToken: string
  expiresAt: string
}

export interface Me {
  id: string
  email: string
  roles: string[]
}

export type AuditEvent = 'BattlePredictionRequested' | 'BattlePredictionCompleted'

export interface AuditItem {
  id: number
  event: AuditEvent
  /** Null for anonymous players. */
  userEmail: string | null
  target: string
  area: string
  method: string
  version: Version
  milestone: string
  objective: Objective
  /** Recommended Pokémon in ranking order; empty for the request. */
  candidates: string[]
  occurredAt: string
}
