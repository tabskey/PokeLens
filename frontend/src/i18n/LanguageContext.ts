import { createContext } from 'react'

import { createTranslator, type Language, type Translator } from './translator'

export interface LanguageState {
  translator: Translator
  setLanguage: (language: Language) => void
}

// Without a provider (unit tests) the app speaks Portuguese.
export const LanguageContext = createContext<LanguageState>({ translator: createTranslator('pt'), setLanguage: () => {} })
