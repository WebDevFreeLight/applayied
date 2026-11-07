import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { fireEvent, render, screen, waitFor } from '@testing-library/react'
import type { ReactNode } from 'react'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { authApi } from '@/api/auth'
import type { UserDto } from '@/api/types'
import { AuthContext, type AuthContextValue } from '@/auth/context'
import { ThemeProvider } from '@/theme/ThemeProvider'
import { IdentityPicker, IdentityQuickSwitch } from './IdentityPicker'

const user: UserDto = {
  id: 'u1',
  email: 'admin@vibevoice.local',
  displayName: 'Admin',
  role: 'Admin',
  preferredLanguage: 'en',
  preferredVoice: 'en-Carter',
  theme: 'light',
  palette: 'sunset',
  isActive: true,
  createdAt: '2026-01-01T00:00:00Z',
}

function wrap(ui: ReactNode, setUser = vi.fn()) {
  const qc = new QueryClient({ defaultOptions: { mutations: { retry: false } } })
  const auth = { user, token: 't', isAdmin: true, setUser } as unknown as AuthContextValue
  return render(
    <QueryClientProvider client={qc}>
      <ThemeProvider>
        <AuthContext.Provider value={auth}>{ui}</AuthContext.Provider>
      </ThemeProvider>
    </QueryClientProvider>,
  )
}

beforeEach(() => {
  localStorage.removeItem('vv.palette')
  document.documentElement.removeAttribute('data-palette')
  document.head.querySelector('meta[name="theme-color"]')?.remove()
})

describe('visual identity switch', () => {
  it('defaults to Sunset Voice and marks it active', () => {
    vi.spyOn(authApi, 'updateProfile').mockResolvedValue(user)
    wrap(<IdentityPicker />)
    expect(document.documentElement.dataset.palette).toBe('sunset')
    expect(screen.getByRole('radio', { name: /Sunset Voice/ })).toHaveAttribute('aria-checked', 'true')
    expect(screen.getAllByRole('radio')).toHaveLength(4)
  })

  it('applies instantly (data-palette + theme-color), persists locally and saves to the profile', async () => {
    const saved = { ...user, palette: 'neon' as const }
    const update = vi.spyOn(authApi, 'updateProfile').mockResolvedValue(saved)
    const setUser = vi.fn()
    wrap(<IdentityPicker />, setUser)

    fireEvent.click(screen.getByRole('radio', { name: /Neon Pulse/ }))

    expect(document.documentElement.dataset.palette).toBe('neon')
    expect(localStorage.getItem('vv.palette')).toBe('neon')
    expect(document.querySelector('meta[name="theme-color"]')).toHaveAttribute('content', '#f7f7f5')
    expect(screen.getByRole('radio', { name: /Neon Pulse/ })).toHaveAttribute('aria-checked', 'true')
    await waitFor(() => expect(update).toHaveBeenCalledTimes(1))
    expect(update).toHaveBeenCalledWith({
      displayName: 'Admin',
      preferredLanguage: 'en',
      preferredVoice: 'en-Carter',
      theme: 'light',
      palette: 'neon',
    })
    await waitFor(() => expect(setUser).toHaveBeenCalledWith(saved))
  })

  it('the header quick switch changes the identity too', async () => {
    const update = vi.spyOn(authApi, 'updateProfile').mockResolvedValue({ ...user, palette: 'ocean' })
    wrap(<IdentityQuickSwitch />)
    fireEvent.click(screen.getByRole('radio', { name: 'Deep Ocean' }))
    expect(document.documentElement.dataset.palette).toBe('ocean')
    await waitFor(() => expect(update).toHaveBeenCalledWith(expect.objectContaining({ palette: 'ocean' })))
  })

  it('restores the stored identity on load', () => {
    localStorage.setItem('vv.palette', 'oasis')
    wrap(<IdentityQuickSwitch />)
    expect(document.documentElement.dataset.palette).toBe('oasis')
    expect(screen.getByRole('radio', { name: 'Desert Oasis' })).toHaveAttribute('aria-checked', 'true')
  })

  it('previews render each identity in light and dark with scoped data attributes', () => {
    const { container } = wrap(<IdentityPicker />)
    for (const p of ['sunset', 'oasis', 'ocean', 'neon']) {
      expect(container.querySelector(`[data-palette="${p}"][data-mode="light"]`)).not.toBeNull()
      expect(container.querySelector(`[data-palette="${p}"][data-mode="dark"]`)).not.toBeNull()
    }
  })
})
