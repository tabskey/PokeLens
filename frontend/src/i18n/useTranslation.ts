import { useContext } from 'react'

import { LanguageContext } from './LanguageContext'

export function useTranslation() {
  const { translator, setLanguage } = useContext(LanguageContext)
  return { ...translator, setLanguage }
}
