import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { EmptyState } from '../ui/Display'
import { ArtTile, type ArtName } from './ArtTile'
import { EmptyArt, type EmptyArtName } from './EmptyArt'
import { Logo, LogoMark } from './Logo'

const ART: ArtName[] = [
  'dashboard', 'memories', 'notes', 'audio', 'studio', 'tts', 'stt', 'ocr', 'images', 'search',
  'trash', 'settings', 'users', 'mic', 'video', 'ai', 'pin', 'transcript', 'palette',
]
const SCENES: EmptyArtName[] = ['notes', 'memories', 'audio', 'search', 'images', 'trash', 'studio', 'users', 'generic']

describe('logo + illustrations', () => {
  it('LogoMark is a labelled image when titled, decorative otherwise', () => {
    const { container } = render(
      <>
        <LogoMark title="VibeVoice" />
        <LogoMark />
      </>,
    )
    expect(screen.getByRole('img', { name: 'VibeVoice' })).toBeInTheDocument()
    const svgs = container.querySelectorAll('svg')
    expect(svgs[1]).toHaveAttribute('aria-hidden', 'true')
    // Gradient ids are unique per instance and valid URL fragments.
    const ids = [...container.querySelectorAll('linearGradient')].map((g) => g.id)
    expect(new Set(ids).size).toBe(ids.length)
    ids.forEach((id) => expect(id).toMatch(/^[a-zA-Z0-9_-]+$/))
  })

  it('the wordmark shows the gradient "Voice"', () => {
    render(<Logo />)
    expect(screen.getByText('Voice')).toHaveClass('text-brand')
  })

  it.each(ART)('ArtTile "%s" renders a decorative SVG tile coloured by identity tokens', (name) => {
    const { container } = render(<ArtTile name={name} />)
    const tile = container.querySelector('[data-art]') as HTMLElement
    expect(tile).toHaveAttribute('data-art', name)
    expect(tile).toHaveAttribute('aria-hidden', 'true')
    expect(tile.style.getPropertyValue('--art-a')).toMatch(/^var\(--chart-\d\)$/)
    expect(tile.querySelector('svg')!.children.length).toBeGreaterThan(0)
  })

  it('ArtTile can be labelled and shows the active state', () => {
    render(<ArtTile name="mic" label="Microphone" active />)
    const img = screen.getByRole('img', { name: 'Microphone' })
    expect(img).toHaveAttribute('data-active', 'true')
  })

  it.each(SCENES)('EmptyArt "%s" is decorative', (name) => {
    const { container } = render(<EmptyArt name={name} />)
    const svg = container.querySelector('svg')!
    expect(svg).toHaveAttribute('aria-hidden', 'true')
    expect(svg.querySelectorAll('rect, path, circle').length).toBeGreaterThan(1)
  })

  it('EmptyState renders the illustration with an accessible heading', () => {
    const { container } = render(<EmptyState art="notes" title="No notes yet" body="Write something" />)
    expect(screen.getByRole('heading', { name: 'No notes yet' })).toBeInTheDocument()
    expect(container.querySelector('svg[aria-hidden="true"]')).not.toBeNull()
  })
})
