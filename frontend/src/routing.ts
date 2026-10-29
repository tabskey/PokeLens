import { useEffect, useState } from 'react'

import type { Objective, PredictRequest, ScenarioKind, Version } from './models/battle'

// shortcut: hash routes cover a handful of screens without a router dependency; adopt one if routes multiply.
export type Route = 'predictor' | 'login' | 'register' | 'admin' | 'favorites'

const pages: Route[] = ['login', 'register', 'admin', 'favorites']

export const routeHref = (route: Route): string => (route === 'predictor' ? '#/' : `#/${route}`)

const current = (): Route => {
  const route = window.location.hash.replace(/^#\//, '').split('?')[0] as Route
  return pages.includes(route) ? route : 'predictor'
}

export function navigate(route: Route) {
  window.location.hash = routeHref(route)
}

/** The screen in the URL hash (#/login, #/favorites...); anything else is the predictor. */
export function useRoute(): Route {
  const [route, setRoute] = useState(current)
  useEffect(() => {
    const onChange = () => setRoute(current())
    window.addEventListener('hashchange', onChange)
    return () => window.removeEventListener('hashchange', onChange)
  }, [])
  return route
}

const predictionKeys = ['target', 'area', 'method', 'version', 'milestone', 'objective', 'scenario'] as const

/** Opens the predictor with this prediction filled in and run (e.g. from a favorite). */
export const predictionHref = (prediction: PredictRequest): string =>
  `#/?${new URLSearchParams(predictionKeys.map((key) => [key, prediction[key]])).toString()}`

/** The prediction in the URL, if a complete one is there; the backend still validates it. */
export function predictionFromHash(): PredictRequest | null {
  const params = new URLSearchParams(window.location.hash.split('?')[1] ?? '')
  const values = predictionKeys.map((key) => params.get(key))
  if (values.some((value) => !value)) {
    return null
  }
  const [target, area, method, version, milestone, objective, scenario] = values as string[]
  // The form's pickers only know these values; anything else is ignored rather than half-filled.
  if (!['firered', 'leafgreen'].includes(version!) || !['Defeat', 'Catch'].includes(objective!) || !['Pessimistic', 'Neutral', 'Optimistic'].includes(scenario!)) {
    return null
  }
  return {
    target: target!, area: area!, method: method!, milestone: milestone!,
    version: version as Version, objective: objective as Objective, scenario: scenario as ScenarioKind,
  }
}
