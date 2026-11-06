import { useCallback, useEffect, useMemo, useState, type ReactNode } from 'react'
import type { ThemePreference } from '@/api/types'
import { useMediaQuery } from '@/hooks/misc'
import { THEME_KEY, ThemeContext, readStoredTheme } from './theme'

export function ThemeProvider({ children }: { children: ReactNode }) {
  const [theme, setThemeState] = useState<ThemePreference>(readStoredTheme)
  const prefersDark = useMediaQuery('(prefers-color-scheme: dark)')
  const resolved: 'light' | 'dark' = theme === 'system' ? (prefersDark ? 'dark' : 'light') : theme

  useEffect(() => {
    document.documentElement.classList.toggle('dark', resolved === 'dark')
  }, [resolved])

  const setTheme = useCallback((t: ThemePreference) => {
    setThemeState(t)
    try {
      localStorage.setItem(THEME_KEY, t)
    } catch {
      /* ignore */
    }
  }, [])

  const value = useMemo(() => ({ theme, resolved, setTheme }), [theme, resolved, setTheme])
  return <ThemeContext.Provider value={value}>{children}</ThemeContext.Provider>
}
