import type { ReactNode } from 'react'
import { cn } from '@/lib/utils'
import { useSvgId } from './svgId'

export type EmptyArtName = 'notes' | 'memories' | 'audio' | 'search' | 'images' | 'trash' | 'studio' | 'users' | 'generic'

/** Larger empty-state illustrations (160×120), coloured by the active identity. Decorative. */
export function EmptyArt({ name, className }: { name: EmptyArtName; className?: string }) {
  const id = useSvgId('vv-empty')
  const g = `url(#${id}-g)`
  const soft = `url(#${id}-soft)`

  const scenes: Record<EmptyArtName, ReactNode> = {
    // Notebook with a pencil
    notes: (
      <>
        <g transform="rotate(-6 80 62)">
          <rect className="fill-surface stroke-border-strong" x="48" y="22" width="62" height="78" rx="9" strokeWidth="2" />
          <rect x="48" y="22" width="12" height="78" rx="6" fill={g} />
          <rect className="fill-border-strong" x="68" y="38" width="32" height="5" rx="2.5" />
          <rect className="fill-border-strong" x="68" y="50" width="26" height="5" rx="2.5" />
          <rect className="fill-border-strong" x="68" y="62" width="30" height="5" rx="2.5" />
          <rect className="fill-border-strong" x="68" y="74" width="18" height="5" rx="2.5" />
        </g>
        <g transform="rotate(38 112 64)">
          <rect x="106" y="26" width="12" height="62" rx="3" fill={g} />
          <path className="fill-surface stroke-border-strong" d="M106 88h12l-6 12z" strokeWidth="1.5" />
          <rect className="fill-border-strong" x="106" y="26" width="12" height="8" rx="3" />
        </g>
      </>
    ),
    // Polaroid stack
    memories: (
      <>
        <g transform="rotate(-12 70 64)">
          <rect className="fill-surface stroke-border-strong" x="40" y="26" width="56" height="66" rx="5" strokeWidth="2" />
          <rect x="46" y="32" width="44" height="40" rx="3" fill={soft} />
        </g>
        <g transform="rotate(8 92 62)">
          <rect className="fill-surface stroke-border-strong" x="64" y="22" width="58" height="70" rx="5" strokeWidth="2" />
          <rect x="70" y="28" width="46" height="44" rx="3" fill={g} />
          <circle cx="104" cy="40" r="5" fill="#fff" fillOpacity="0.85" />
          <path d="M72 70l14-16 9 10 6-6 13 12z" fill="#fff" fillOpacity="0.7" />
          <path
            d="M93 86c-.6 0-6-3.6-6-7.2 0-1.8 1.5-3.2 3.2-3.2 1.3 0 2.3.7 2.8 1.7.5-1 1.5-1.7 2.8-1.7 1.7 0 3.2 1.4 3.2 3.2 0 3.6-5.4 7.2-6 7.2z"
            fill={g}
          />
        </g>
      </>
    ),
    // Waveform + microphone
    audio: (
      <>
        {[14, 24, 34, 44, 54, 106, 116, 126, 136, 146].map((x, i) => {
          const h = [14, 30, 20, 40, 26, 26, 40, 20, 30, 14][i]
          return <rect key={x} x={x - 3} y={62 - h / 2} width="6" height={h} rx="3" fill={i % 2 ? g : undefined} className={i % 2 ? undefined : 'fill-border-strong'} />
        })}
        <rect x="66" y="16" width="28" height="52" rx="14" fill={g} />
        <rect x="72" y="28" width="16" height="3.5" rx="1.75" fill="#fff" fillOpacity="0.6" />
        <rect x="72" y="36" width="16" height="3.5" rx="1.75" fill="#fff" fillOpacity="0.6" />
        <path className="stroke-border-strong" d="M58 58a22 22 0 0 0 44 0" fill="none" strokeWidth="5" strokeLinecap="round" />
        <rect className="fill-border-strong" x="77" y="80" width="6" height="14" rx="3" />
        <rect className="fill-border-strong" x="64" y="94" width="32" height="6" rx="3" />
      </>
    ),
    // Magnifier over a document
    search: (
      <>
        <rect className="fill-surface stroke-border-strong" x="38" y="16" width="60" height="80" rx="9" strokeWidth="2" />
        <rect className="fill-border-strong" x="50" y="32" width="36" height="5" rx="2.5" />
        <rect className="fill-border-strong" x="50" y="44" width="28" height="5" rx="2.5" />
        <rect className="fill-border-strong" x="50" y="56" width="32" height="5" rx="2.5" />
        <circle cx="96" cy="70" r="22" fill={soft} stroke={g} strokeWidth="7" />
        <path d="M112 86l18 18" stroke={g} strokeWidth="10" strokeLinecap="round" />
        <circle cx="89" cy="63" r="6" fill="#fff" fillOpacity="0.5" />
      </>
    ),
    // Photo frame with mountains
    images: (
      <>
        <rect className="fill-surface stroke-border-strong" x="30" y="30" width="72" height="56" rx="8" strokeWidth="2" transform="rotate(-8 66 58)" />
        <rect x="52" y="22" width="80" height="62" rx="9" fill={g} />
        <circle cx="112" cy="40" r="7" fill="#fff" fillOpacity="0.85" />
        <path d="M56 80l22-26 14 16 10-10 26 20z" fill="#fff" fillOpacity="0.75" />
      </>
    ),
    // Empty bin with sparkles
    trash: (
      <>
        <rect x="50" y="30" width="60" height="9" rx="4.5" fill={g} />
        <rect className="fill-border-strong" x="70" y="22" width="20" height="9" rx="4.5" />
        <path className="fill-surface stroke-border-strong" d="M56 44h48l-4.5 46a7 7 0 0 1-7 6.3H67.5a7 7 0 0 1-7-6.3z" strokeWidth="2" />
        <rect className="fill-border-strong" x="70" y="54" width="5" height="30" rx="2.5" />
        <rect className="fill-border-strong" x="85" y="54" width="5" height="30" rx="2.5" />
        <path d="M128 30l2.5 7 7 2.5-7 2.5-2.5 7-2.5-7-7-2.5 7-2.5z" fill={g} />
        <path d="M30 52l1.8 5 5 1.8-5 1.8-1.8 5-1.8-5-5-1.8 5-1.8z" fill={g} />
      </>
    ),
    // Sparkles + speech bubble
    studio: (
      <>
        <path d="M40 30a10 10 0 0 1 10-10h56a10 10 0 0 1 10 10v36a10 10 0 0 1-10 10H70l-18 16V76h-2a10 10 0 0 1-10-10z" fill={g} />
        {[56, 66, 76, 86, 96].map((x, i) => {
          const h = [12, 26, 18, 30, 14][i]
          return <rect key={x} x={x} y={48 - h / 2} width="5" height={h} rx="2.5" fill="#fff" fillOpacity={0.9} />
        })}
        <path d="M128 72l3 8.5 8.5 3-8.5 3-3 8.5-3-8.5-8.5-3 8.5-3z" fill={g} />
      </>
    ),
    users: (
      <>
        <circle className="stroke-border-strong" cx="98" cy="48" r="14" fill={soft} strokeWidth="2" />
        <path className="stroke-border-strong" d="M74 98c0-14 10.5-24 24-24s24 10 24 24z" fill={soft} strokeWidth="2" />
        <circle cx="66" cy="44" r="17" fill={g} />
        <path d="M36 100c0-17 13-29 30-29s30 12 30 29z" fill={g} />
      </>
    ),
    generic: (
      <>
        <circle cx="80" cy="60" r="34" fill={g} />
        <path d="M80 38l5 14 14 5-14 5-5 14-5-14-14-5 14-5z" fill="#fff" fillOpacity="0.9" />
      </>
    ),
  }

  return (
    <svg viewBox="0 0 160 120" className={cn('h-auto w-40 max-w-full', className)} aria-hidden focusable="false">
      <defs>
        <linearGradient id={`${id}-g`} x1="0" y1="0" x2="1" y2="1">
          <stop offset="0" style={{ stopColor: 'var(--grad-from)' }} />
          <stop offset="0.55" style={{ stopColor: 'var(--grad-via)' }} />
          <stop offset="1" style={{ stopColor: 'var(--grad-to)' }} />
        </linearGradient>
        <linearGradient id={`${id}-soft`} x1="0" y1="0" x2="1" y2="1">
          <stop offset="0" style={{ stopColor: 'var(--grad-from)', stopOpacity: 0.22 }} />
          <stop offset="1" style={{ stopColor: 'var(--grad-to)', stopOpacity: 0.22 }} />
        </linearGradient>
      </defs>
      <ellipse cx="80" cy="108" rx="58" ry="7" style={{ fill: 'var(--surface-3)' }} />
      <g>{scenes[name]}</g>
    </svg>
  )
}
