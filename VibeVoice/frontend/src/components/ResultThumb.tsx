import { Search } from 'lucide-react'
import { fileUrl } from '@/api/client'
import type { SearchResultDto } from '@/api/types'
import { KIND_ICON } from '@/lib/links'
import { cn } from '@/lib/utils'

/** Icon tile, or the picture's thumbnail for image results (when the API provides one). */
export function ResultThumb({ r, size = 'md' }: { r: SearchResultDto; size?: 'sm' | 'md' }) {
  const Icon = KIND_ICON[r.kind] ?? Search
  const box = size === 'sm' ? 'size-8 rounded-lg' : 'size-9 rounded-xl'
  if (r.kind === 'image' && r.thumbnailPath) {
    return (
      <img
        src={fileUrl(r.thumbnailPath)}
        alt=""
        loading="lazy"
        data-testid="result-thumb"
        className={cn(box, 'shrink-0 bg-surface-2 object-cover', size === 'md' && 'size-12')}
      />
    )
  }
  return (
    <span className={cn(box, 'flex shrink-0 items-center justify-center bg-accent-soft text-accent-text')}>
      <Icon className="size-4" aria-hidden />
    </span>
  )
}
