import { useEffect, useState } from 'react'

import { ApiError, getMe, getToken, setToken } from '../models/api'
import { adminRole, type Me } from '../models/auth'

/**
 * Who is logged in, restored from the stored token. Logging in is optional: with no token there is no
 * call and the player is anonymous. An expired or invalid token is dropped.
 */
export function useSessionViewModel() {
  const [user, setUser] = useState<Me | null>(null)
  const [checked, setChecked] = useState(() => getToken() === null)

  useEffect(() => {
    if (getToken() === null) {
      return
    }
    let cancelled = false
    getMe()
      .then((me) => {
        if (!cancelled) setUser(me)
      })
      .catch((reason: unknown) => {
        // Only a rejected token ends the session; a network error keeps it for the next visit.
        if (!cancelled && reason instanceof ApiError && reason.status === 401) setToken(null)
      })
      .finally(() => {
        if (!cancelled) setChecked(true)
      })
    return () => {
      cancelled = true
    }
  }, [])

  const logout = () => {
    setToken(null)
    setUser(null)
  }

  return {
    email: user?.email ?? null,
    isAdmin: user?.roles.includes(adminRole) ?? false,
    /** False while the stored token is being checked. */
    checked,
    logout,
  }
}

export type SessionViewModel = ReturnType<typeof useSessionViewModel>
