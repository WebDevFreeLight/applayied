import { useCallback, useEffect, useMemo, useState, type ReactNode } from 'react'
import type { Palette, ThemePreference } from '@/api/types'
import { useMediaQuery } from '@/hooks/misc'
import { applyPalette, PALETTE_KEY, readStoredPalette, readStoredTheme, THEME_KEY, ThemeContext } from './theme'

export function ThemeProvider({ children }: { children: ReactNode }) {
  const [theme, setThemeState] = useState<ThemePreference>(readStoredTheme)
  const [palette, setPaletteState] = useState<Palette>(readStoredPalette)
  const prefersDark = useMediaQuery('(prefers-color-scheme: dark)')
  const resolved: 'light' | 'dark' = theme === 'system' ? (prefersDark ? 'dark' : 'light') : theme

  useEffect(() => {
    document.documentElement.classList.toggle('dark', resolved === 'dark')
    applyPalette(palette, resolved)
  }, [resolved, palette])

  const setTheme = useCallback((t: ThemePreference) => {
    setThemeState(t)
    try {
      localStorage.setItem(THEME_KEY, t)
    } catch {
      /* ignore */
    }
  }, [])

  const setPalette = useCallback((p: Palette) => {
    setPaletteState(p)
    try {
      localStorage.setItem(PALETTE_KEY, p)
    } catch {
      /* ignore */
    }
  }, [])

  const value = useMemo(
    () => ({ theme, resolved, setTheme, palette, setPalette }),
    [theme, resolved, setTheme, palette, setPalette],
  )
  return <ThemeContext.Provider value={value}>{children}</ThemeContext.Provider>
}
