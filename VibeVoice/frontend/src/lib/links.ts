import { AudioLines, BookHeart, Image, NotebookPen, type LucideIcon } from 'lucide-react'
import type { ActivityItem, SearchKind, SearchResultDto } from '@/api/types'

export const KIND_ICON: Record<SearchKind, LucideIcon> = {
  note: NotebookPen,
  memory: BookHeart,
  audio: AudioLines,
  image: Image,
}

/** Where a search result should navigate to. */
export function searchResultLink(r: SearchResultDto): string {
  if (r.kind === 'note') return `/notes/${r.id}`
  if (r.kind === 'memory') return `/memories/${r.id}`
  // audio / image → its owner (library images → /images)
  if (r.ownerKind === 'note' && r.ownerId) return `/notes/${r.ownerId}`
  if (r.ownerKind === 'memory' && r.ownerId) return `/memories/${r.ownerId}`
  return r.kind === 'image' ? '/images' : '/studio'
}

export function audioOwnerLink(ownerType: string, ownerId: string): string {
  if (ownerType === 'Note') return `/notes/${ownerId}`
  if (ownerType === 'Memory') return `/memories/${ownerId}`
  return '/studio'
}

export function activityLink(a: ActivityItem): string {
  if (a.kind === 'note') return `/notes/${a.id}`
  if (a.kind === 'memory') return `/memories/${a.id}`
  if (a.kind === 'image') return '/images'
  return '/audio'
}
