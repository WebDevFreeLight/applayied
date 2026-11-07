import { AudioLines, CalendarDays, Image as ImageIcon, MapPin, Pin, Users } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router'
import { fileUrl } from '@/api/client'
import type { MemoryListItemDto, NoteListItemDto } from '@/api/types'
import { useDates } from '@/lib/dates'
import { cn, MOOD_EMOJI, noteColorVar } from '@/lib/utils'
import { Badge } from './ui/Display'

function Counts({ audio, images }: { audio: number; images: number }) {
  const { t } = useTranslation()
  if (!audio && !images) return null
  return (
    <span className="flex items-center gap-2 text-xs text-subtle">
      {audio > 0 && (
        <span className="inline-flex items-center gap-1" title={t('notes.audioCount', { count: audio })}>
          <AudioLines className="size-3.5" aria-hidden />
          <span aria-hidden>{audio}</span>
          <span className="sr-only">{t('notes.audioCount', { count: audio })}</span>
        </span>
      )}
      {images > 0 && (
        <span className="inline-flex items-center gap-1" title={t('notes.imageCount', { count: images })}>
          <ImageIcon className="size-3.5" aria-hidden />
          <span aria-hidden>{images}</span>
          <span className="sr-only">{t('notes.imageCount', { count: images })}</span>
        </span>
      )}
    </span>
  )
}

function TagList({ tags, max = 3 }: { tags: string[]; max?: number }) {
  if (!tags.length) return null
  return (
    <span className="flex min-w-0 flex-wrap gap-1">
      {tags.slice(0, max).map((tag) => (
        <Badge key={tag} tone="accent" className="max-w-[10rem]">
          <bdi className="truncate">#{tag}</bdi>
        </Badge>
      ))}
      {tags.length > max && <Badge>+{tags.length - max}</Badge>}
    </span>
  )
}

export function NoteCard({ note, layout = 'grid' }: { note: NoteListItemDto; layout?: 'grid' | 'list' }) {
  const { t } = useTranslation()
  const dates = useDates()
  const bg = noteColorVar(note.color)
  return (
    <Link
      to={`/notes/${note.id}`}
      style={bg ? { backgroundColor: bg } : undefined}
      className={cn(
        'group flex overflow-hidden rounded-2xl border border-border bg-surface shadow-card transition hover:-translate-y-0.5 hover:border-border-strong hover:shadow-pop',
        layout === 'grid' ? 'h-full flex-col' : 'flex-row items-stretch',
      )}
    >
      {note.thumbnailPath && (
        <div className={cn('shrink-0 overflow-hidden bg-surface-2', layout === 'grid' ? 'aspect-[16/9] w-full' : 'w-24 sm:w-32')}>
          <img src={fileUrl(note.thumbnailPath)} alt="" loading="lazy" className="size-full object-cover" />
        </div>
      )}
      <div className="flex min-w-0 flex-1 flex-col gap-2 p-3.5 sm:p-4">
        <div className="flex items-start gap-2">
          <h3 dir="auto" className="line-clamp-2 flex-1 font-semibold text-fg">
            {note.title || t('common.untitled')}
          </h3>
          {note.isPinned && <Pin className="mt-0.5 size-4 shrink-0 fill-current text-accent-text" aria-label={t('common.pinned')} />}
        </div>
        {note.excerpt && (
          <p dir="auto" className={cn('text-sm text-muted', layout === 'grid' ? 'line-clamp-4' : 'line-clamp-2')}>
            {note.excerpt}
          </p>
        )}
        <div className="mt-auto flex flex-wrap items-center justify-between gap-2 pt-1">
          <TagList tags={note.tags} />
          <span className="flex items-center gap-3">
            <Counts audio={note.audioCount} images={note.imageCount} />
            <span className="text-xs text-subtle">{dates.relative(note.updatedAt)}</span>
          </span>
        </div>
        {note.isArchived && <Badge className="self-start">{t('common.archived')}</Badge>}
      </div>
    </Link>
  )
}

export function MemoryCard({ memory, compact }: { memory: MemoryListItemDto; compact?: boolean }) {
  const { t } = useTranslation()
  const dates = useDates()
  return (
    <Link
      to={`/memories/${memory.id}`}
      className="group flex h-full flex-col overflow-hidden rounded-2xl border border-border bg-surface shadow-card transition hover:-translate-y-0.5 hover:border-border-strong hover:shadow-pop"
    >
      <div
        className={cn('relative w-full overflow-hidden bg-brand-soft', compact ? 'aspect-[2/1]' : 'aspect-[16/10]')}
        style={
          memory.mood && !memory.coverThumbnailPath
            ? {
                backgroundImage: `linear-gradient(135deg, color-mix(in srgb, var(--mood-${memory.mood.toLowerCase()}) 35%, var(--surface)), var(--surface-3))`,
              }
            : undefined
        }
      >
        {memory.coverThumbnailPath ? (
          <img
            src={fileUrl(memory.coverThumbnailPath)}
            alt=""
            loading="lazy"
            className="size-full object-cover transition-transform duration-500 group-hover:scale-[1.04]"
          />
        ) : (
          <div className="flex size-full items-center justify-center text-4xl" aria-hidden>
            {memory.mood ? MOOD_EMOJI[memory.mood] : '✨'}
          </div>
        )}
        <span className="absolute bottom-2 start-2 inline-flex items-center gap-1 rounded-full bg-black/55 px-2 py-0.5 text-[11px] font-medium text-white backdrop-blur">
          <CalendarDays className="size-3" aria-hidden />
          {dates.format(memory.memoryDate, 'PP')}
        </span>
        {memory.isPinned && (
          <span className="absolute top-2 end-2 rounded-full bg-black/55 p-1 text-white backdrop-blur">
            <Pin className="size-3.5 fill-current" aria-label={t('common.pinned')} />
          </span>
        )}
      </div>
      <div className="flex flex-1 flex-col gap-1.5 p-4">
        <h3 dir="auto" className="line-clamp-2 font-semibold text-fg">
          {memory.mood && (
            <span
              className="me-1.5 inline-block rounded-full px-1"
              style={{ backgroundColor: `color-mix(in srgb, var(--mood-${memory.mood.toLowerCase()}) 22%, transparent)` }}
              title={t(`memories.moods.${memory.mood}`)}
            >
              {MOOD_EMOJI[memory.mood]}
            </span>
          )}
          {memory.title || t('common.untitled')}
        </h3>
        {!compact && memory.excerpt && (
          <p dir="auto" className="line-clamp-3 text-sm text-muted">
            {memory.excerpt}
          </p>
        )}
        <div className="mt-auto flex flex-wrap items-center gap-x-3 gap-y-1 pt-1 text-xs text-subtle">
          {memory.location && (
            <span className="inline-flex min-w-0 items-center gap-1">
              <MapPin className="size-3.5 shrink-0" aria-hidden />
              <bdi className="truncate">{memory.location}</bdi>
            </span>
          )}
          {memory.people.length > 0 && (
            <span className="inline-flex min-w-0 items-center gap-1">
              <Users className="size-3.5 shrink-0" aria-hidden />
              <bdi className="truncate">{memory.people.slice(0, 3).join('، ')}</bdi>
            </span>
          )}
          <Counts audio={memory.audioCount} images={memory.imageCount} />
        </div>
        {!compact && <TagList tags={memory.tags} />}
      </div>
    </Link>
  )
}

export function CardSkeletonGrid({ count = 6, className }: { count?: number; className?: string }) {
  return (
    <div className={cn('grid gap-4 sm:grid-cols-2 lg:grid-cols-3', className)} aria-hidden>
      {Array.from({ length: count }, (_, i) => (
        <div key={i} className="flex flex-col gap-3 rounded-2xl border border-border bg-surface p-4">
          <div className="h-4 w-2/3 animate-pulse-soft rounded-lg bg-surface-3" />
          <div className="h-3 w-full animate-pulse-soft rounded-lg bg-surface-3" />
          <div className="h-3 w-5/6 animate-pulse-soft rounded-lg bg-surface-3" />
          <div className="mt-3 h-3 w-1/3 animate-pulse-soft rounded-lg bg-surface-3" />
        </div>
      ))}
    </div>
  )
}
