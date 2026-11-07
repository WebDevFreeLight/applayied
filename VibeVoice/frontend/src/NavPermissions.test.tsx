import { render, screen, within } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { makeUser } from './test/adminFixtures'

function json(body: unknown) {
  return {
    ok: true,
    status: 200,
    statusText: 'OK',
    headers: { get: () => 'application/json' },
    text: async () => JSON.stringify(body),
  }
}

describe('permission-aware navigation', () => {
  it('hides nav items the role cannot use and guards the route', { timeout: 20_000 }, async () => {
    const limited = makeUser({
      id: 'u9',
      email: 'reader@example.com',
      displayName: 'Reader',
      role: 'Readers',
      roleId: 'r-readers',
      permissions: ['memories', 'voice.tts'],
    })
    localStorage.setItem('vv.token', 'jwt')
    localStorage.setItem('vv.lang', 'en')
    vi.stubGlobal(
      'fetch',
      vi.fn(async (url: string) => {
        if (url.endsWith('/api/auth/me')) return json(limited)
        if (url.endsWith('/api/voice/status'))
          return json({ available: true, ttsEngine: 'edge', asrEngine: null, ocrEngine: null, device: 'cpu', message: null })
        return json([])
      }),
    )
    window.history.pushState({}, '', '/notes')
    const { App } = await import('./App')
    render(<App />)

    // Route guard: /notes needs the "notes" permission.
    expect(await screen.findByRole('heading', { name: "You don't have access" }, { timeout: 8000 })).toBeInTheDocument()

    const navs = screen.getAllByRole('navigation', { name: 'Main navigation' })
    const sidebar = navs[0]
    expect(within(sidebar).getByRole('link', { name: 'Memories' })).toBeInTheDocument()
    expect(within(sidebar).getByRole('link', { name: 'VibeVoice Studio' })).toBeInTheDocument()
    expect(within(sidebar).queryByRole('link', { name: 'Notes' })).not.toBeInTheDocument()
    expect(within(sidebar).queryByRole('link', { name: 'Users' })).not.toBeInTheDocument()
    expect(within(sidebar).queryByRole('link', { name: 'Roles' })).not.toBeInTheDocument()
    expect(within(sidebar).queryByText('Administration')).not.toBeInTheDocument()
    // Trash needs notes OR memories → visible.
    expect(within(sidebar).getByRole('link', { name: 'Trash' })).toBeInTheDocument()
    vi.unstubAllGlobals()
  })
})
