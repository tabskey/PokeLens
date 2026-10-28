import { useEffect, useState } from 'react'

const storageKey = 'theme'

function initialDark(): boolean {
  try {
    const stored = localStorage.getItem(storageKey)
    if (stored === 'dark' || stored === 'light') {
      return stored === 'dark'
    }
  } catch {
    // Storage can be blocked (private window); follow the system.
  }
  return window.matchMedia?.('(prefers-color-scheme: dark)').matches ?? false
}

/** Light or dark theme: the system's until the player picks one, then remembered. */
export function useThemeViewModel() {
  const [dark, setDark] = useState(initialDark)

  useEffect(() => {
    document.documentElement.dataset.theme = dark ? 'dark' : 'light'
  }, [dark])

  const toggleTheme = () => {
    const next = !dark
    setDark(next)
    try {
      localStorage.setItem(storageKey, next ? 'dark' : 'light')
    } catch {
      // Not remembered across visits, but the switch still works.
    }
  }

  return { dark, toggleTheme }
}
