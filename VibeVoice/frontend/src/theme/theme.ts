import { createContext, useContext } from 'react'
import type { ThemePreference } from '@/api/types'

export const THEME_KEY = 'vv.theme'

export interface ThemeContextValue {
  theme: ThemePreference
  resolved: 'light' | 'dark'
  setTheme: (t: ThemePreference) => void
}

export const ThemeContext = createContext<ThemeContextValue | null>(null)

export function useTheme(): ThemeContextValue {
  const ctx = useContext(ThemeContext)
  if (!ctx) throw new Error('useTheme must be used inside <ThemeProvider>')
  return ctx
}

export function readStoredTheme(): ThemePreference {
  try {
    const v = localStorage.getItem(THEME_KEY)
    if (v === 'light' || v === 'dark' || v === 'system') return v
  } catch {
    /* ignore */
  }
  return 'system'
}
