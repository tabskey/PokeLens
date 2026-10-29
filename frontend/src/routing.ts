import { useEffect, useState } from 'react'

// shortcut: hash routes cover four screens without a router dependency; adopt one if routes get parameters.
export type Route = 'predictor' | 'login' | 'register' | 'admin'

export const routeHref = (route: Route): string => (route === 'predictor' ? '#/' : `#/${route}`)

const current = (): Route => {
  const route = window.location.hash.replace(/^#\//, '')
  return route === 'login' || route === 'register' || route === 'admin' ? route : 'predictor'
}

export function navigate(route: Route) {
  window.location.hash = routeHref(route)
}

/** The screen in the URL hash (#/login, #/register, #/admin); anything else is the predictor. */
export function useRoute(): Route {
  const [route, setRoute] = useState(current)
  useEffect(() => {
    const onChange = () => setRoute(current())
    window.addEventListener('hashchange', onChange)
    return () => window.removeEventListener('hashchange', onChange)
  }, [])
  return route
}
