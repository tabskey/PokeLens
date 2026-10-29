import type { AuditItem, Me, TokenResponse } from './auth'
import type { FavoriteItem } from './favorites'
import type { GameOptions, PredictRequest, PredictResponse, TargetOption, Version } from './battle'

/**
 * A failed API call. `message` is a translation key, or a backend message that passes through as-is
 * (e.g. the 401 of a wrong password); `fieldErrors` comes from ValidationProblemDetails.
 */
export class ApiError extends Error {
  readonly fieldErrors: Record<string, string[]>
  /** HTTP status; null when the server could not be reached. */
  readonly status: number | null

  constructor(message: string, fieldErrors: Record<string, string[]> = {}, status: number | null = null) {
    super(message)
    this.fieldErrors = fieldErrors
    this.status = status
  }
}

const tokenKey = 'token'

/** The JWT of the logged-in player. Logging in is optional, so null is the normal case. */
export function getToken(): string | null {
  try {
    return localStorage.getItem(tokenKey)
  } catch {
    return null
  }
}

export function setToken(token: string | null) {
  try {
    if (token) {
      localStorage.setItem(tokenKey, token)
    } else {
      localStorage.removeItem(tokenKey)
    }
  } catch {
    // Storage blocked (private window): the session just ends with the page.
  }
}

const statusMessages: Record<number, string> = { 400: 'errors.validation', 401: 'errors.unauthorized', 403: 'errors.forbidden' }

async function request<T>(url: string, init?: RequestInit): Promise<T> {
  const headers = new Headers(init?.headers)
  const token = getToken()
  if (token) {
    headers.set('Authorization', `Bearer ${token}`)
  }

  let response: Response
  try {
    response = await fetch(url, { ...init, headers })
  } catch {
    throw new ApiError('errors.network')
  }

  if (response.ok) {
    // 201 Created has no body.
    const text = await response.text()
    return (text ? JSON.parse(text) : undefined) as T
  }

  const problem = (await response.json().catch(() => null)) as { title?: string; errors?: Record<string, string[]> } | null
  const message = response.status === 401 && problem?.title ? problem.title : (statusMessages[response.status] ?? 'errors.server')
  throw new ApiError(message, problem?.errors ?? {}, response.status)
}

const json = (body: unknown, method = 'POST'): RequestInit => ({
  method,
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify(body),
})

export function getOptions(): Promise<GameOptions> {
  return request<GameOptions>('/api/v1/game-data/options')
}

export function getTargets(version: Version, milestone: string): Promise<TargetOption[]> {
  const query = new URLSearchParams({ version, milestone })
  return request<TargetOption[]>(`/api/v1/targets?${query.toString()}`)
}

export function postPredict(body: PredictRequest): Promise<PredictResponse> {
  return request<PredictResponse>('/api/v1/battles/predict', json(body))
}

export function register(email: string, password: string): Promise<void> {
  return request<void>('/api/v1/auth/register', json({ email, password }))
}

export function login(email: string, password: string): Promise<TokenResponse> {
  return request<TokenResponse>('/api/v1/auth/login', json({ email, password }))
}

export function getMe(): Promise<Me> {
  return request<Me>('/api/v1/auth/me')
}

export function getAudit(limit: number): Promise<AuditItem[]> {
  return request<AuditItem[]>(`/api/v1/admin/audit?limit=${limit}`)
}

export function getFavorites(): Promise<FavoriteItem[]> {
  return request<FavoriteItem[]>('/api/v1/favorites')
}

/** Saving the same prediction again returns the existing favorite. */
export function saveFavorite(prediction: PredictRequest): Promise<FavoriteItem> {
  return request<FavoriteItem>('/api/v1/favorites', json({ prediction }))
}

/** An empty name goes back to the automatic description. */
export function renameFavorite(id: number, name: string): Promise<FavoriteItem> {
  return request<FavoriteItem>(`/api/v1/favorites/${id}`, json({ name }, 'PATCH'))
}

export function deleteFavorite(id: number): Promise<void> {
  return request<void>(`/api/v1/favorites/${id}`, { method: 'DELETE' })
}