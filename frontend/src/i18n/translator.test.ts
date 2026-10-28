import { describe, expect, it } from 'vitest'

import en from '../locales/en/translation.json'
import pt from '../locales/pt/translation.json'
import { createTranslator } from './translator'

const keys = (tree: object, prefix = ''): string[] =>
  Object.entries(tree).flatMap(([key, value]) => (typeof value === 'object' ? keys(value as object, `${prefix}${key}.`) : [`${prefix}${key}`]))

describe('createTranslator', () => {
  it('has every key in both languages', () => {
    expect(keys(en).sort()).toEqual(keys(pt).sort())
  })

  it('fills placeholders and picks singular or plural by count', () => {
    const { t } = createTranslator('en')

    expect(t('results.rankedBy', { ranking: 'Safest' })).toBe('ranked by: Safest')
    expect(t('results.count', { count: 1 })).toBe('1 suggestion')
    expect(t('results.count', { count: 0 })).toBe('0 suggestions')
  })

  it('falls back to the default value, then to the key', () => {
    const { t } = createTranslator('pt')

    expect(t('method.surf', { defaultValue: 'Surf' })).toBe('Surf')
    expect(t('O Pokémon-alvo não aparece nessa área.')).toBe('O Pokémon-alvo não aparece nessa área.')
  })

  it('formats percentages for the language', () => {
    expect(createTranslator('pt').percent(0.0625)).toBe('6,3%')
    expect(createTranslator('en').percent(0.0625)).toBe('6.3%')
  })
})
