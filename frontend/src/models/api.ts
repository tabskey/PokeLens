import type { GameOptions, PredictRequest, PredictResponse, TargetOption, Version } from './battle'

/**
 * A failed API call. `message` is a translation key; `fieldErrors` comes from ValidationProblemDetails
 * (field → messages, already written by the backend).
 */
export class ApiError extends Error {
  readonly fieldErrors: Record<string, string[]>

  constructor(message: string, fieldErrors: Record<string, string[]> = {}) {
    super(message)
    this.fieldErrors = fieldErrors
  }
}

async function request<T>(url: string, init?: RequestInit): Promise<T> {
  let response: Response
  try {
    response = await fetch(url, init)
  } catch {
    throw new ApiError('errors.network')
  }

  if (response.ok) {
    return (await response.json()) as T
  }

  const problem = (await response.json().catch(() => null)) as { errors?: Record<string, string[]> } | null
  throw new ApiError(response.status === 400 ? 'errors.validation' : 'errors.server', problem?.errors ?? {})
}

export function getOptions(): Promise<GameOptions> {
  return request<GameOptions>('/api/v1/game-data/options')
}

export function getTargets(version: Version, milestone: string): Promise<TargetOption[]> {
  const query = new URLSearchParams({ version, milestone })
  return request<TargetOption[]>(`/api/v1/targets?${query.toString()}`)
}

export function postPredict(body: PredictRequest): Promise<PredictResponse> {
  return request<PredictResponse>('/api/v1/battles/predict', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  })
}