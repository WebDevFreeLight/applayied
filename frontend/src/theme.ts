// Light/dark theme, persisted in localStorage and applied via <html data-theme>.
export type Theme = 'dark' | 'light'

export function getTheme(): Theme {
  return (localStorage.getItem('pmo.theme') as Theme) || 'dark'
}

export function applyTheme(theme: Theme) {
  document.documentElement.setAttribute('data-theme', theme)
}

export function setTheme(theme: Theme) {
  localStorage.setItem('pmo.theme', theme)
  applyTheme(theme)
}

export function toggleTheme(): Theme {
  const next: Theme = getTheme() === 'dark' ? 'light' : 'dark'
  setTheme(next)
  return next
}

// Apply immediately on module load so there is no flash of the wrong theme.
applyTheme(getTheme())
