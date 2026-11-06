import type { Mood } from '@/api/types'

/** Tiny className joiner. */
export function cn(...parts: Array<string | false | null | undefined>): string {
  return parts.filter(Boolean).join(' ')
}

export function formatDuration(seconds: number | null | undefined): string {
  if (seconds == null || !Number.isFinite(seconds) || seconds < 0) return '--:--'
  const total = Math.floor(seconds)
  const h = Math.floor(total / 3600)
  const m = Math.floor((total % 3600) / 60)
  const s = total % 60
  const mm = h ? String(m).padStart(2, '0') : String(m)
  return `${h ? `${h}:` : ''}${mm}:${String(s).padStart(2, '0')}`
}

export function formatBytes(bytes: number): string {
  if (!Number.isFinite(bytes) || bytes <= 0) return '0 B'
  const units = ['B', 'KB', 'MB', 'GB']
  const i = Math.min(units.length - 1, Math.floor(Math.log(bytes) / Math.log(1024)))
  const v = bytes / 1024 ** i
  return `${v >= 10 || i === 0 ? Math.round(v) : v.toFixed(1)} ${units[i]}`
}

export function escapeHtml(text: string): string {
  return text
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&#39;')
}

/** Plain text → simple HTML paragraphs (for TipTap bodies). */
export function textToHtml(text: string): string {
  return text
    .split(/\n{2,}|\r\n\r\n/)
    .map((p) => p.trim())
    .filter(Boolean)
    .map((p) => `<p>${escapeHtml(p).replace(/\n/g, '<br>')}</p>`)
    .join('')
}

export function stripHtml(html: string): string {
  if (!html) return ''
  const doc = new DOMParser().parseFromString(html, 'text/html')
  return (doc.body.textContent ?? '').replace(/\s+/g, ' ').trim()
}

export function todayIso(): string {
  const d = new Date()
  const mm = String(d.getMonth() + 1).padStart(2, '0')
  const dd = String(d.getDate()).padStart(2, '0')
  return `${d.getFullYear()}-${mm}-${dd}`
}

/** Parses a date-only string (yyyy-MM-dd) as a local date (avoids UTC shifting). */
export function parseDateOnly(value: string): Date {
  const [y, m, d] = value.slice(0, 10).split('-').map(Number)
  return new Date(y, (m || 1) - 1, d || 1)
}

export const NOTE_COLORS = ['yellow', 'green', 'blue', 'purple', 'pink', 'orange', 'gray'] as const
export type NoteColor = (typeof NOTE_COLORS)[number]

export function noteColorVar(color: string | null | undefined): string | undefined {
  if (!color) return undefined
  return (NOTE_COLORS as readonly string[]).includes(color) ? `var(--note-${color})` : color
}

export const MOOD_EMOJI: Record<Mood, string> = {
  Happy: '😊',
  Grateful: '🙏',
  Calm: '😌',
  Excited: '🤩',
  Nostalgic: '🥹',
  Proud: '🏆',
  Sad: '😢',
  Loved: '🥰',
}

export const IMAGE_TYPES = ['image/jpeg', 'image/png', 'image/webp', 'image/gif'] as const
export const IMAGE_MAX_BYTES = 10 * 1024 * 1024
export const AUDIO_EXTENSIONS = ['webm', 'ogg', 'oga', 'opus', 'wav', 'mp3', 'm4a', 'aac', 'flac']
export const AUDIO_MAX_BYTES = 50 * 1024 * 1024
export const VIDEO_EXTENSIONS = ['mp4', 'm4v', 'mov', 'webm', 'mkv', 'avi']
export const VIDEO_MAX_BYTES = 500 * 1024 * 1024
export const AUDIO_ACCEPT = 'audio/*,.webm,.ogg,.opus,.wav,.mp3,.m4a,.aac,.flac'
/** Accept list for "Upload audio / video" pickers. */
export const MEDIA_ACCEPT = `${AUDIO_ACCEPT},video/*,.mp4,.m4v,.mov,.mkv,.avi`

const extOf = (name: string) => (name.includes('.') ? (name.split('.').pop() ?? '').toLowerCase() : '')

/** Audio, video or neither — by MIME type first, then by extension. */
export function mediaKindOf(file: File): 'Audio' | 'Video' | null {
  const type = file.type.split(';')[0].toLowerCase()
  if (type.startsWith('video/')) return 'Video'
  if (type.startsWith('audio/')) return 'Audio'
  if (type && type !== 'application/octet-stream') return null
  const ext = extOf(file.name)
  // .webm without a MIME type is ambiguous; treat it as video so the larger limit applies.
  if (VIDEO_EXTENSIONS.includes(ext)) return 'Video'
  if (AUDIO_EXTENSIONS.includes(ext)) return 'Audio'
  return null
}

export function isAudioFile(file: File): boolean {
  return mediaKindOf(file) === 'Audio'
}

export type MediaValidation =
  | { ok: true; kind: 'Audio' | 'Video' }
  | { ok: false; reason: 'invalidType' | 'tooLarge'; kind: 'Audio' | 'Video' | null; maxMb: number }

/** Validates an audio/video upload (audio ≤ 50 MB, video ≤ 500 MB). */
export function validateMediaFile(file: File): MediaValidation {
  const kind = mediaKindOf(file)
  if (!kind) return { ok: false, reason: 'invalidType', kind: null, maxMb: 0 }
  const max = kind === 'Video' ? VIDEO_MAX_BYTES : AUDIO_MAX_BYTES
  if (file.size > max) return { ok: false, reason: 'tooLarge', kind, maxMb: max / 1024 / 1024 }
  return { ok: true, kind }
}

export function isImageFile(file: File): boolean {
  return (IMAGE_TYPES as readonly string[]).includes(file.type.toLowerCase())
}

/** Saves text as a UTF-8 .txt file (client-side Blob). */
export function downloadText(text: string, filename: string): void {
  const url = URL.createObjectURL(new Blob(['\ufeff', text], { type: 'text/plain;charset=utf-8' }))
  const a = document.createElement('a')
  a.href = url
  a.download = filename.endsWith('.txt') ? filename : `${filename}.txt`
  document.body.appendChild(a)
  a.click()
  a.remove()
  setTimeout(() => URL.revokeObjectURL(url), 10_000)
}

/**
 * Downloads a (possibly cross-origin) URL as a file. The `download` attribute is ignored
 * cross-origin, so we fetch a blob first and fall back to opening the URL.
 */
export async function downloadUrl(url: string, filename: string): Promise<void> {
  try {
    const res = await fetch(url)
    if (!res.ok) throw new Error(String(res.status))
    const blobUrl = URL.createObjectURL(await res.blob())
    const a = document.createElement('a')
    a.href = blobUrl
    a.download = filename
    document.body.appendChild(a)
    a.click()
    a.remove()
    setTimeout(() => URL.revokeObjectURL(blobUrl), 10_000)
  } catch {
    window.open(url, '_blank', 'noopener')
  }
}

export function extensionForMime(mime: string): string {
  const m = mime.toLowerCase()
  if (m.includes('webm')) return 'webm'
  if (m.includes('ogg')) return 'ogg'
  if (m.includes('mp4') || m.includes('m4a') || m.includes('aac')) return 'm4a'
  if (m.includes('mpeg') || m.includes('mp3')) return 'mp3'
  if (m.includes('wav')) return 'wav'
  return 'bin'
}
