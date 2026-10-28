import en from '../locales/en/translation.json'
import pt from '../locales/pt/translation.json'

export type Language = 'pt' | 'en'

export const languages: Language[] = ['pt', 'en']

const resources: Record<Language, object> = { pt, en }

const locales: Record<Language, string> = { pt: 'pt-BR', en: 'en-US' }

/** `count` picks the `_one`/`_other` key; `defaultValue` is used when the key is missing (i18next conventions). */
export type Params = Record<string, string | number | undefined> & { count?: number; defaultValue?: string }

export interface Translator {
  language: Language
  /** BCP 47 tag for `<html lang>` and number formats. */
  locale: string
  t: (key: string, params?: Params) => string
  percent: (value: number) => string
}

function lookup(tree: object, key: string): string | undefined {
  const value = key.split('.').reduce<unknown>((node, part) => (node as Record<string, unknown> | undefined)?.[part], tree)
  return typeof value === 'string' ? value : undefined
}

/** Hand-written texts from locales/<language>/translation.json; a missing key falls back to the key itself. */
export function createTranslator(language: Language): Translator {
  const tree = resources[language]
  const locale = locales[language]
  const percent = new Intl.NumberFormat(locale, { style: 'percent', maximumFractionDigits: 1 })

  return {
    language,
    locale,
    percent: (value) => percent.format(value),
    t: (key, params = {}) => {
      // Same rule in both languages, so "0 sugestões" stays plural as before.
      const plural = params.count === undefined ? undefined : lookup(tree, `${key}_${params.count === 1 ? 'one' : 'other'}`)
      const text = plural ?? lookup(tree, key) ?? params.defaultValue ?? key
      return text.replace(/\{\{(\w+)\}\}/g, (match, name: string) => String(params[name] ?? match))
    },
  }
}
