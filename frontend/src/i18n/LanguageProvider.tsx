import { useEffect, useMemo, useState, type ReactNode } from 'react'

import { LanguageContext } from './LanguageContext'
import { createTranslator, languages, type Language } from './translator'

const storageKey = 'language'

function initialLanguage(): Language {
  try {
    const stored = localStorage.getItem(storageKey)
    if (languages.includes(stored as Language)) {
      return stored as Language
    }
  } catch {
    // Storage can be blocked (private window); fall back to the browser language.
  }
  return navigator.language.toLowerCase().startsWith('pt') ? 'pt' : 'en'
}

export function LanguageProvider({ children }: { children: ReactNode }) {
  const [language, setLanguageState] = useState(initialLanguage)
  const translator = useMemo(() => createTranslator(language), [language])

  useEffect(() => {
    document.documentElement.lang = translator.locale
  }, [translator])

  const setLanguage = (next: Language) => {
    setLanguageState(next)
    try {
      localStorage.setItem(storageKey, next)
    } catch {
      // Not remembered across visits, but the switch still works.
    }
  }

  return <LanguageContext.Provider value={{ translator, setLanguage }}>{children}</LanguageContext.Provider>
}
