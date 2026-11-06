import type { VoiceDto } from '@/api/types'

export function isFallbackVoice(v: VoiceDto): boolean {
  return v.isFallback || v.engine !== 'vibevoice'
}

export function groupVoices(voices: VoiceDto[]): [string, VoiceDto[]][] {
  const map = new Map<string, VoiceDto[]>()
  for (const v of voices) {
    const key = v.language || 'other'
    map.set(key, [...(map.get(key) ?? []), v])
  }
  return [...map.entries()].sort(([a], [b]) => (a === 'en' ? -1 : b === 'en' ? 1 : a.localeCompare(b)))
}
