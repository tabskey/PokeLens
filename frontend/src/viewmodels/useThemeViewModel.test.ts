import { act, renderHook } from '@testing-library/react'
import { afterEach, describe, expect, it } from 'vitest'

import { useThemeViewModel } from './useThemeViewModel'

describe('useThemeViewModel', () => {
  afterEach(() => localStorage.clear())

  it('starts from the stored choice and remembers a switch', () => {
    localStorage.setItem('theme', 'dark')
    const { result } = renderHook(() => useThemeViewModel())
    expect(result.current.dark).toBe(true)
    expect(document.documentElement.dataset.theme).toBe('dark')

    act(() => result.current.toggleTheme())

    expect(result.current.dark).toBe(false)
    expect(document.documentElement.dataset.theme).toBe('light')
    expect(localStorage.getItem('theme')).toBe('light')
  })
})
