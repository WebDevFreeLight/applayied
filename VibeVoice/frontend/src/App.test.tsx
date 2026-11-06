import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

describe('App smoke test', () => {
  it('redirects anonymous users to the login page', async () => {
    localStorage.clear()
    vi.stubGlobal('fetch', vi.fn().mockRejectedValue(new TypeError('offline')))
    window.history.pushState({}, '', '/notes')
    const { App } = await import('./App')
    render(<App />)
    expect(await screen.findByRole('heading', { name: 'Welcome back' }, { timeout: 8000 })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Sign in' })).toBeInTheDocument()
    expect(window.location.pathname).toBe('/login')
    vi.unstubAllGlobals()
  })
})
