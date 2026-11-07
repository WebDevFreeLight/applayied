import type { CSSProperties, ReactNode } from 'react'
import { cn } from '@/lib/utils'

/** Illustrated, identity-coloured icon tiles ("icons as images"). */
export type ArtName =
  | 'dashboard'
  | 'memories'
  | 'notes'
  | 'audio'
  | 'studio'
  | 'tts'
  | 'stt'
  | 'ocr'
  | 'images'
  | 'search'
  | 'trash'
  | 'settings'
  | 'users'
  | 'mic'
  | 'video'
  | 'ai'
  | 'pin'
  | 'transcript'
  | 'palette'
  | 'overview'
  | 'roles'
  | 'lock'
  | 'storage'

const INK = 'var(--art-ink)'
const SOFT = 0.55

/** Duotone glyphs on a 48×48 grid; ink colour follows --art-ink, accents follow the tile colours. */
const GLYPHS: Record<ArtName, ReactNode> = {
  dashboard: (
    <>
      <rect x="11" y="11" width="12" height="15" rx="3.5" style={{ fill: INK }} />
      <rect x="25" y="11" width="12" height="9" rx="3.5" fillOpacity={SOFT} style={{ fill: INK }} />
      <rect x="25" y="22" width="12" height="15" rx="3.5" style={{ fill: INK }} />
      <rect x="11" y="28" width="12" height="9" rx="3.5" fillOpacity={SOFT} style={{ fill: INK }} />
    </>
  ),
  memories: (
    <>
      <rect x="13" y="12" width="20" height="23" rx="2.5" fillOpacity={SOFT} transform="rotate(-10 23 23)" style={{ fill: INK }} />
      <rect x="16" y="13" width="20" height="24" rx="2.5" transform="rotate(6 26 25)" style={{ fill: INK }} />
      <rect x="18.5" y="16" width="15" height="12" rx="1.5" fillOpacity={0.9} transform="rotate(6 26 22)" style={{ fill: 'var(--art-b)' }} />
      <path
        d="M26 25.6c-.3 0-3.2-2-3.2-3.9 0-1 .8-1.7 1.7-1.7.7 0 1.2.4 1.5.9.3-.5.8-.9 1.5-.9.9 0 1.7.7 1.7 1.7 0 1.9-2.9 3.9-3.2 3.9z"
       
        transform="rotate(6 26 22)" style={{ fill: INK }} />
    </>
  ),
  notes: (
    <>
      <rect x="12" y="10" width="21" height="28" rx="4" style={{ fill: INK }} />
      <rect x="16" y="16" width="13" height="2.4" rx="1.2" fillOpacity={0.75} style={{ fill: 'var(--art-a)' }} />
      <rect x="16" y="21.5" width="10" height="2.4" rx="1.2" fillOpacity={0.55} style={{ fill: 'var(--art-a)' }} />
      <rect x="16" y="27" width="12" height="2.4" rx="1.2" fillOpacity={0.55} style={{ fill: 'var(--art-a)' }} />
      <path d="M36.5 18.5l3 3-9.5 9.5-4 1 1-4z" fillOpacity={0.85} strokeWidth="1.2" strokeLinejoin="round" style={{ fill: INK, stroke: 'var(--art-b)' }} />
    </>
  ),
  audio: (
    <>
      <rect x="9" y="20" width="3.6" height="8" rx="1.8" fillOpacity={SOFT} style={{ fill: INK }} />
      <rect x="14.6" y="14" width="3.6" height="20" rx="1.8" style={{ fill: INK }} />
      <rect x="20.2" y="17" width="3.6" height="14" rx="1.8" fillOpacity={SOFT} style={{ fill: INK }} />
      <rect x="25.8" y="11" width="3.6" height="26" rx="1.8" style={{ fill: INK }} />
      <circle cx="35" cy="31" r="7.5" style={{ fill: INK }} />
      <path d="M33.2 27.8v6.4l5-3.2z" style={{ fill: 'var(--art-b)' }} />
    </>
  ),
  studio: (
    <>
      <path d="M22 9l2.6 7.4L32 19l-7.4 2.6L22 29l-2.6-7.4L12 19l7.4-2.6z" style={{ fill: INK }} />
      <path d="M34 25l1.5 4 4 1.5-4 1.5-1.5 4-1.5-4-4-1.5 4-1.5z" fillOpacity={0.75} style={{ fill: INK }} />
      <path d="M11 34c3-3 5 3 8 0s5 3 8 0" strokeOpacity={SOFT} strokeWidth="2.4" fill="none" strokeLinecap="round" style={{ stroke: INK }} />
    </>
  ),
  tts: (
    <>
      <path d="M10 15a5 5 0 0 1 5-5h18a5 5 0 0 1 5 5v12a5 5 0 0 1-5 5H21l-7 6v-6h0a4 4 0 0 1-4-4z" style={{ fill: INK }} />
      <rect x="16" y="18" width="2.6" height="6" rx="1.3" style={{ fill: 'var(--art-a)' }} />
      <rect x="21" y="15" width="2.6" height="12" rx="1.3" style={{ fill: 'var(--art-b)' }} />
      <rect x="26" y="17" width="2.6" height="8" rx="1.3" style={{ fill: 'var(--art-a)' }} />
      <rect x="31" y="19" width="2.6" height="4" rx="1.3" style={{ fill: 'var(--art-b)' }} />
    </>
  ),
  stt: (
    <>
      <rect x="9" y="18" width="3" height="10" rx="1.5" fillOpacity={SOFT} style={{ fill: INK }} />
      <rect x="14" y="13" width="3" height="20" rx="1.5" style={{ fill: INK }} />
      <rect x="19" y="17" width="3" height="12" rx="1.5" fillOpacity={SOFT} style={{ fill: INK }} />
      <rect x="25" y="12" width="14" height="22" rx="3.5" style={{ fill: INK }} />
      <rect x="28" y="17" width="8" height="2.2" rx="1.1" style={{ fill: 'var(--art-a)' }} />
      <rect x="28" y="22" width="6" height="2.2" rx="1.1" style={{ fill: 'var(--art-b)' }} />
      <rect x="28" y="27" width="7" height="2.2" rx="1.1" style={{ fill: 'var(--art-a)' }} />
    </>
  ),
  ocr: (
    <>
      <rect x="9" y="11" width="24" height="20" rx="4" fillOpacity={SOFT} style={{ fill: INK }} />
      <path d="M12 28l6-7 4 4 3-3 5 6z" style={{ fill: INK }} />
      <circle cx="27" cy="17" r="2.4" style={{ fill: INK }} />
      <rect x="22" y="23" width="17" height="15" rx="3.5" style={{ fill: INK }} />
      <rect x="25.5" y="27" width="10" height="2" rx="1" style={{ fill: 'var(--art-a)' }} />
      <rect x="25.5" y="31.5" width="7" height="2" rx="1" style={{ fill: 'var(--art-b)' }} />
    </>
  ),
  images: (
    <>
      <rect x="10" y="12" width="28" height="24" rx="5" style={{ fill: INK }} />
      <circle cx="30.5" cy="19.5" r="3" style={{ fill: 'var(--art-b)' }} />
      <path d="M13 33l8-9.5 5.5 6 3.5-3.5 6 7z" fillOpacity={0.8} style={{ fill: 'var(--art-a)' }} />
    </>
  ),
  search: (
    <>
      <rect x="10" y="9" width="20" height="26" rx="4" fillOpacity={SOFT} style={{ fill: INK }} />
      <circle cx="26" cy="25" r="8.5" fill="none" strokeWidth="4" style={{ stroke: INK }} />
      <circle cx="26" cy="25" r="6" fillOpacity={0.25} style={{ fill: INK }} />
      <path d="M32.5 31.5l5.5 5.5" strokeWidth="4.5" strokeLinecap="round" style={{ stroke: INK }} />
    </>
  ),
  trash: (
    <>
      <rect x="11" y="13" width="26" height="4.5" rx="2.25" style={{ fill: INK }} />
      <rect x="20" y="9.5" width="8" height="4" rx="2" fillOpacity={SOFT} style={{ fill: INK }} />
      <path d="M14 19.5h20l-1.8 15.2a3.5 3.5 0 0 1-3.5 3.1H19.3a3.5 3.5 0 0 1-3.5-3.1z" fillOpacity={0.85} style={{ fill: INK }} />
      <rect x="20" y="23" width="2.4" height="11" rx="1.2" fillOpacity={0.7} style={{ fill: 'var(--art-a)' }} />
      <rect x="25.6" y="23" width="2.4" height="11" rx="1.2" fillOpacity={0.7} style={{ fill: 'var(--art-a)' }} />
    </>
  ),
  settings: (
    <>
      <rect x="10" y="14" width="28" height="3.2" rx="1.6" fillOpacity={SOFT} style={{ fill: INK }} />
      <rect x="10" y="22.4" width="28" height="3.2" rx="1.6" fillOpacity={SOFT} style={{ fill: INK }} />
      <rect x="10" y="30.8" width="28" height="3.2" rx="1.6" fillOpacity={SOFT} style={{ fill: INK }} />
      <circle cx="17" cy="15.6" r="4.2" style={{ fill: INK }} />
      <circle cx="31" cy="24" r="4.2" style={{ fill: INK }} />
      <circle cx="21" cy="32.4" r="4.2" style={{ fill: INK }} />
    </>
  ),
  users: (
    <>
      <circle cx="30" cy="17" r="5" fillOpacity={SOFT} style={{ fill: INK }} />
      <path d="M21 35c0-5.5 4-9 9-9s9 3.5 9 9z" fillOpacity={SOFT} style={{ fill: INK }} />
      <circle cx="19" cy="18" r="6" style={{ fill: INK }} />
      <path d="M8 37c0-6.5 5-10.5 11-10.5S30 30.5 30 37z" style={{ fill: INK }} />
    </>
  ),
  mic: (
    <>
      <rect x="18" y="8" width="12" height="21" rx="6" style={{ fill: INK }} />
      <path d="M13 23a11 11 0 0 0 22 0" fill="none" strokeOpacity={SOFT} strokeWidth="3" strokeLinecap="round" style={{ stroke: INK }} />
      <rect x="22.5" y="33" width="3" height="6" rx="1.5" fillOpacity={SOFT} style={{ fill: INK }} />
      <rect x="21" y="13" width="6" height="2" rx="1" fillOpacity={0.6} style={{ fill: 'var(--art-a)' }} />
      <rect x="21" y="17" width="6" height="2" rx="1" fillOpacity={0.6} style={{ fill: 'var(--art-a)' }} />
    </>
  ),
  video: (
    <>
      <rect x="8" y="14" width="23" height="20" rx="5" style={{ fill: INK }} />
      <path d="M31 21l9-5v16l-9-5z" fillOpacity={SOFT} style={{ fill: INK }} />
      <path d="M17 19.5v9l7-4.5z" style={{ fill: 'var(--art-b)' }} />
    </>
  ),
  ai: (
    <>
      <path d="M24 8l3.4 9.6L37 21l-9.6 3.4L24 34l-3.4-9.6L11 21l9.6-3.4z" style={{ fill: INK }} />
      <circle cx="35" cy="34" r="3.2" fillOpacity={SOFT} style={{ fill: INK }} />
      <circle cx="13" cy="35" r="2" fillOpacity={SOFT} style={{ fill: INK }} />
    </>
  ),
  pin: (
    <>
      <path d="M17 10h14l-2.5 9 5 6H14.5l5-6z" style={{ fill: INK }} />
      <rect x="22.6" y="25" width="2.8" height="13" rx="1.4" fillOpacity={SOFT} style={{ fill: INK }} />
    </>
  ),
  transcript: (
    <>
      <rect x="12" y="9" width="24" height="30" rx="4.5" style={{ fill: INK }} />
      <rect x="16" y="15" width="2.4" height="6" rx="1.2" style={{ fill: 'var(--art-a)' }} />
      <rect x="20" y="13" width="2.4" height="10" rx="1.2" style={{ fill: 'var(--art-b)' }} />
      <rect x="24" y="15.5" width="2.4" height="5" rx="1.2" style={{ fill: 'var(--art-a)' }} />
      <rect x="16" y="27" width="16" height="2.2" rx="1.1" fillOpacity={0.6} style={{ fill: 'var(--art-a)' }} />
      <rect x="16" y="32" width="11" height="2.2" rx="1.1" fillOpacity={0.6} style={{ fill: 'var(--art-a)' }} />
    </>
  ),
  palette: (
    <>
      <path d="M24 9a15 15 0 1 0 0 30c2 0 3-1.4 3-3 0-2.2-2-2.6-2-4.5 0-1.6 1.3-2.5 3-2.5h3.5A7.5 7.5 0 0 0 39 21.5C39 14.6 32.3 9 24 9z" style={{ fill: INK }} />
      <circle cx="16.5" cy="22" r="2.8" style={{ fill: 'var(--art-a)' }} />
      <circle cx="21" cy="15.5" r="2.8" style={{ fill: 'var(--art-b)' }} />
      <circle cx="29" cy="15.5" r="2.8" style={{ fill: 'var(--art-a)' }} />
    </>
  ),
  overview: (
    <>
      <rect x="10" y="10" width="28" height="28" rx="6" fillOpacity={0.3} style={{ fill: INK }} />
      <rect x="15" y="25" width="4.5" height="8" rx="2.25" style={{ fill: INK }} />
      <rect x="21.75" y="18" width="4.5" height="15" rx="2.25" style={{ fill: INK }} />
      <rect x="28.5" y="21.5" width="4.5" height="11.5" rx="2.25" style={{ fill: INK }} />
      <circle cx="35" cy="14" r="3.2" style={{ fill: 'var(--art-b)' }} />
    </>
  ),
  roles: (
    <>
      <path d="M24 8l13 5v9.5c0 8-5.6 14.6-13 17.5-7.4-2.9-13-9.5-13-17.5V13z" style={{ fill: INK }} />
      <path d="M18.5 24l4 4 7.5-8" fill="none" strokeWidth="3.2" strokeLinecap="round" strokeLinejoin="round" style={{ stroke: 'var(--art-a)' }} />
    </>
  ),
  lock: (
    <>
      <path d="M17 21v-4a7 7 0 0 1 14 0v4" fill="none" strokeWidth="3.6" strokeOpacity={SOFT} style={{ stroke: INK }} />
      <rect x="12" y="20" width="24" height="19" rx="5" style={{ fill: INK }} />
      <circle cx="24" cy="28" r="2.8" style={{ fill: 'var(--art-a)' }} />
      <rect x="22.8" y="29" width="2.4" height="5.5" rx="1.2" style={{ fill: 'var(--art-a)' }} />
    </>
  ),
  storage: (
    <>
      <ellipse cx="24" cy="14" rx="12" ry="4.5" style={{ fill: INK }} />
      <path d="M12 14v8c0 2.5 5.4 4.5 12 4.5s12-2 12-4.5v-8c0 2.5-5.4 4.5-12 4.5S12 16.5 12 14z" fillOpacity={SOFT} style={{ fill: INK }} />
      <path d="M12 24v9c0 2.5 5.4 4.5 12 4.5s12-2 12-4.5v-9c0 2.5-5.4 4.5-12 4.5S12 26.5 12 24z" style={{ fill: INK }} />
      <circle cx="30.5" cy="32" r="1.6" style={{ fill: 'var(--art-a)' }} />
    </>
  ),
}

/** Tile colours: each module gets a pair of identity chart colours (recolours per identity). */
const TONES: Record<ArtName, [number, number]> = {
  dashboard: [1, 3],
  memories: [2, 5],
  notes: [4, 1],
  audio: [3, 2],
  studio: [1, 2],
  tts: [2, 3],
  stt: [3, 6],
  ocr: [6, 4],
  images: [5, 4],
  search: [3, 1],
  trash: [5, 6],
  settings: [6, 3],
  users: [4, 2],
  mic: [1, 5],
  video: [3, 4],
  ai: [2, 6],
  pin: [4, 5],
  transcript: [6, 1],
  palette: [1, 3],
  overview: [3, 1],
  roles: [2, 4],
  lock: [5, 3],
  storage: [6, 2],
}

const SIZES = {
  xs: 'size-7 rounded-[9px]',
  sm: 'size-9 rounded-[11px]',
  md: 'size-11 rounded-[13px]',
  lg: 'size-14 rounded-2xl',
  xl: 'size-[4.5rem] rounded-[1.35rem]',
} as const

export interface ArtTileProps {
  name: ArtName
  size?: keyof typeof SIZES
  /** Active/selected state → soft glow. */
  active?: boolean
  /** Accessible label; omit for decorative use (aria-hidden). */
  label?: string
  className?: string
}

/** Rounded-square, gradient, duotone illustrated icon. Colours come from the active identity. */
export function ArtTile({ name, size = 'md', active, label, className }: ArtTileProps) {
  const [a, b] = TONES[name]
  const style = { '--art-a': `var(--chart-${a})`, '--art-b': `var(--chart-${b})` } as CSSProperties
  return (
    <span
      className={cn('art-tile', SIZES[size], className)}
      data-art={name}
      data-active={active ? 'true' : undefined}
      style={style}
      role={label ? 'img' : undefined}
      aria-label={label}
      aria-hidden={label ? undefined : true}
    >
      <svg viewBox="0 0 48 48" className="relative size-full" aria-hidden focusable="false">
        {GLYPHS[name]}
      </svg>
    </span>
  )
}
