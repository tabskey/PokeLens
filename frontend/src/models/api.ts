import type { GameOptions, PredictRequest, PredictResponse, Version } from './battle'

/** A failed API call. `fieldErrors` comes from ValidationProblemDetails (field → messages). */
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
    throw new ApiError('Não foi possível falar com o servidor. Tente de novo.')
  }

  if (response.ok) {
    return (await response.json()) as T
  }

  const problem = (await response.json().catch(() => null)) as { title?: string; errors?: Record<string, string[]> } | null
  throw new ApiError(
    response.status === 400 ? 'Confira os campos destacados.' : (problem?.title ?? 'Erro inesperado no servidor.'),
    problem?.errors ?? {},
  )
}

export function getOptions(version: Version): Promise<GameOptions> {
  return request<GameOptions>(`/api/v1/game-data/options?version=${encodeURIComponent(version)}`)
}

export function postPredict(body: PredictRequest): Promise<PredictResponse> {
  return request<PredictResponse>('/api/v1/battles/predict', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  })
}