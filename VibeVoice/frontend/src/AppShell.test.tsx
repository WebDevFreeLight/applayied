import { fireEvent, render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import type { DashboardDto, UserDto } from './api/types'

const user: UserDto = {
  id: 'u1',
  email: 'admin@vibevoice.local',
  displayName: 'Admin',
  role: 'Admin',
  preferredLanguage: 'en',
  preferredVoice: null,
  theme: 'light',
  isActive: true,
  createdAt: '2026-01-01T00:00:00Z',
}

const dashboard: DashboardDto = {
  stats: { notes: 3, memories: 2, audioClips: 4, images: 5, aiVoiceClips: 1, transcripts: 2, pinned: 1, trash: 0 },
  recentNotes: [
    {
      id: 'n1',
      title: 'Groceries',
      excerpt: 'Milk, eggs',
      color: null,
      isPinned: true,
      isArchived: false,
      isDeleted: false,
      tags: ['home'],
      imageCount: 0,
      audioCount: 1,
      thumbnailPath: null,
      createdAt: '2026-11-01T10:00:00Z',
      updatedAt: '2026-11-06T10:00:00Z',
    },
  ],
  recentMemories: [],
  onThisDay: [],
  activity: [{ kind: 'note', action: 'updated', id: 'n1', ownerId: null, title: 'Groceries', at: '2026-11-06T10:00:00Z' }],
}

function json(body: unknown) {
  return {
    ok: true,
    status: 200,
    statusText: 'OK',
    headers: { get: () => 'application/json' },
    text: async () => JSON.stringify(body),
  }
}

describe('authenticated shell', () => {
  it('renders the layout + dashboard and shows the admin-only Users link', async () => {
    localStorage.setItem('vv.token', 'jwt')
    localStorage.setItem('vv.lang', 'en')
    vi.stubGlobal(
      'fetch',
      vi.fn(async (url: string) => {
        if (url.endsWith('/api/auth/me')) return json(user)
        if (url.endsWith('/api/dashboard')) return json(dashboard)
        if (url.endsWith('/api/voice/status'))
          return json({ available: true, ttsEngine: 'vibevoice', asrEngine: 'whisper', device: 'cpu', message: null })
        return json([])
      }),
    )
    window.history.pushState({}, '', '/')
    const { App } = await import('./App')
    render(<App />)

    expect(await screen.findByRole('heading', { name: 'Hello, Admin' })).toBeInTheDocument()
    expect(await screen.findByText('Groceries', { selector: 'h3' })).toBeInTheDocument()
    expect(screen.getAllByRole('link', { name: 'Users' }).length).toBeGreaterThan(0)
    expect((await screen.findAllByText('Voice ready')).length).toBeGreaterThan(0)

    // ⌘/Ctrl+K opens quick search.
    fireEvent.keyDown(window, { key: 'k', ctrlKey: true })
    expect(await screen.findByRole('dialog', { name: 'Quick search' })).toBeInTheDocument()
    vi.unstubAllGlobals()
  })
})
