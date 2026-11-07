import { createContext, useContext } from 'react'
import type { Palette, ThemePreference } from '@/api/types'

export const THEME_KEY = 'vv.theme'
export const PALETTE_KEY = 'vv.palette'

export const PALETTES: readonly Palette[] = ['sunset', 'oasis', 'ocean', 'neon']
export const DEFAULT_PALETTE: Palette = 'sunset'

/** Browser UI colour (<meta name="theme-color">) per identity — mirrors --theme-color in palettes.css. */
export const THEME_COLORS: Record<Palette, { light: string; dark: string }> = {
  sunset: { light: '#fff8f3', dark: '#1a1220' },
  oasis: { light: '#fbf7ef', dark: '#0e1a1a' },
  ocean: { light: '#f5f9ff', dark: '#0b1224' },
  neon: { light: '#f7f7f5', dark: '#0a0a0b' },
}

export interface ThemeContextValue {
  theme: ThemePreference
  resolved: 'light' | 'dark'
  setTheme: (t: ThemePreference) => void
  palette: Palette
  setPalette: (p: Palette) => void
}

export const ThemeContext = createContext<ThemeContextValue | null>(null)

export function useTheme(): ThemeContextValue {
  const ctx = useContext(ThemeContext)
  if (!ctx) throw new Error('useTheme must be used inside <ThemeProvider>')
  return ctx
}

export function isPalette(v: unknown): v is Palette {
  return typeof v === 'string' && (PALETTES as readonly string[]).includes(v)
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

export function readStoredPalette(): Palette {
  try {
    const v = localStorage.getItem(PALETTE_KEY)
    if (isPalette(v)) return v
  } catch {
    /* ignore */
  }
  return DEFAULT_PALETTE
}

/** Applies the identity to <html data-palette> and updates <meta name="theme-color">. */
export function applyPalette(palette: Palette, mode: 'light' | 'dark'): void {
  const root = document.documentElement
  root.dataset.palette = palette
  let meta = document.querySelector<HTMLMetaElement>('meta[name="theme-color"]')
  if (!meta) {
    meta = document.createElement('meta')
    meta.name = 'theme-color'
    document.head.appendChild(meta)
  }
  meta.content = THEME_COLORS[palette][mode]
}
