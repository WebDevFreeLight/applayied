import { ChevronLeft, ChevronRight, GripVertical, Star, Trash2 } from 'lucide-react'
import { useState, type DragEvent, type ReactNode } from 'react'
import { useTranslation } from 'react-i18next'
import { toast } from 'sonner'
import { errorMessage, fileUrl } from '@/api/client'
import type { ImageDto } from '@/api/types'
import { cn } from '@/lib/utils'
import { Badge } from '../ui/Display'
import { ConfirmDialog } from '../ui/Modal'
import { Lightbox } from './Lightbox'

export interface ImageGalleryProps {
  images: ImageDto[]
  onReorder?: (ids: string[]) => Promise<unknown>
  onCaption?: (id: string, caption: string | null) => Promise<unknown>
  onDelete?: (id: string) => Promise<unknown>
  /** Memories only. */
  onSetCover?: (id: string | null) => Promise<unknown>
  coverImageId?: string | null
}

/** Thumbnail grid: lightbox, drag (or ←/→ buttons) to reorder, captions, delete, set cover. */
export function ImageGallery({ images, onReorder, onCaption, onDelete, onSetCover, coverImageId }: ImageGalleryProps) {
  const { t } = useTranslation()
  const sorted = [...images].sort((a, b) => a.sortOrder - b.sortOrder)
  // Optimistic order while a reorder request is in flight.
  const [order, setOrder] = useState<string[] | null>(null)
  const [openIndex, setOpenIndex] = useState<number | null>(null)
  const [dragId, setDragId] = useState<string | null>(null)
  const [overId, setOverId] = useState<string | null>(null)
  const [pendingDelete, setPendingDelete] = useState<ImageDto | null>(null)
  const [deleting, setDeleting] = useState(false)

  const byId = new Map(sorted.map((i) => [i.id, i]))
  const list =
    order && order.length === sorted.length && order.every((id) => byId.has(id))
      ? order.map((id) => byId.get(id)!)
      : sorted

  const commitOrder = async (ids: string[]) => {
    if (!onReorder) return
    setOrder(ids)
    try {
      await onReorder(ids)
    } catch (err) {
      toast.error(errorMessage(err))
    } finally {
      setOrder(null)
    }
  }

  const move = (index: number, delta: number) => {
    const target = index + delta
    if (target < 0 || target >= list.length) return
    const ids = list.map((i) => i.id)
    ;[ids[index], ids[target]] = [ids[target], ids[index]]
    void commitOrder(ids)
  }

  const onDrop = (e: DragEvent, targetId: string) => {
    e.preventDefault()
    const from = dragId
    setDragId(null)
    setOverId(null)
    if (!from || from === targetId) return
    const ids = list.map((i) => i.id).filter((id) => id !== from)
    ids.splice(ids.indexOf(targetId), 0, from)
    void commitOrder(ids)
  }

  const confirmDelete = async () => {
    if (!pendingDelete || !onDelete) return
    setDeleting(true)
    try {
      await onDelete(pendingDelete.id)
      toast.success(t('images.deleted'))
      setPendingDelete(null)
    } catch (err) {
      toast.error(errorMessage(err))
    } finally {
      setDeleting(false)
    }
  }

  if (!list.length) return null

  return (
    <>
      <ul className="grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-4">
        {list.map((img, i) => {
          const isCover = coverImageId === img.id
          return (
            <li
              key={img.id}
              draggable={!!onReorder}
              onDragStart={(e) => {
                setDragId(img.id)
                e.dataTransfer.effectAllowed = 'move'
                e.dataTransfer.setData('text/plain', img.id)
              }}
              onDragOver={(e) => {
                if (!dragId) return
                e.preventDefault()
                setOverId(img.id)
              }}
              onDragEnd={() => {
                setDragId(null)
                setOverId(null)
              }}
              onDrop={(e) => onDrop(e, img.id)}
              className={cn(
                'group relative flex flex-col overflow-hidden rounded-2xl border bg-surface shadow-card transition',
                overId === img.id && dragId !== img.id ? 'border-accent ring-2 ring-accent/30' : 'border-border',
                dragId === img.id && 'opacity-50',
              )}
            >
              <button
                type="button"
                onClick={() => setOpenIndex(i)}
                aria-label={t('images.openImage', { index: i + 1 })}
                className="relative block aspect-square w-full overflow-hidden bg-surface-2"
              >
                <img
                  src={fileUrl(img.thumbnailPath || img.filePath)}
                  alt={img.caption ?? ''}
                  loading="lazy"
                  className="size-full object-cover transition-transform duration-300 group-hover:scale-[1.03]"
                />
                {isCover && (
                  <Badge tone="accent" className="absolute top-2 start-2 shadow-sm">
                    <Star className="size-3 fill-current" aria-hidden />
                    {t('images.coverBadge')}
                  </Badge>
                )}
              </button>

              {onCaption ? (
                <CaptionInput
                  key={`${img.id}-${img.caption ?? ''}`}
                  initial={img.caption ?? ''}
                  onSave={(c) => onCaption(img.id, c)}
                />
              ) : (
                img.caption && (
                  <p dir="auto" className="truncate px-2.5 py-1.5 text-xs text-muted">
                    {img.caption}
                  </p>
                )
              )}

              <div className="flex items-center gap-0.5 border-t border-border px-1.5 py-1">
                {onReorder && (
                  <>
                    <span className="hidden cursor-grab p-1 text-subtle sm:inline" title={t('images.dragToReorder')} aria-hidden>
                      <GripVertical className="size-4" />
                    </span>
                    <TileButton label={t('common.moveEarlier')} disabled={i === 0} onClick={() => move(i, -1)}>
                      <ChevronLeft className="size-4 rtl:rotate-180" aria-hidden />
                    </TileButton>
                    <TileButton label={t('common.moveLater')} disabled={i === list.length - 1} onClick={() => move(i, 1)}>
                      <ChevronRight className="size-4 rtl:rotate-180" aria-hidden />
                    </TileButton>
                  </>
                )}
                <span className="flex-1" />
                {onSetCover && (
                  <TileButton
                    label={isCover ? t('memories.removeCover') : t('memories.setCover')}
                    pressed={isCover}
                    onClick={() =>
                      void onSetCover(isCover ? null : img.id).then(
                        () => toast.success(t('memories.coverSet')),
                        (err: unknown) => toast.error(errorMessage(err)),
                      )
                    }
                  >
                    <Star className={cn('size-4', isCover && 'fill-current text-warning')} aria-hidden />
                  </TileButton>
                )}
                {onDelete && (
                  <TileButton label={t('common.delete')} danger onClick={() => setPendingDelete(img)}>
                    <Trash2 className="size-4" aria-hidden />
                  </TileButton>
                )}
              </div>
            </li>
          )
        })}
      </ul>

      {openIndex !== null && (
        <Lightbox
          images={list}
          index={Math.min(openIndex, list.length - 1)}
          onIndexChange={setOpenIndex}
          onClose={() => setOpenIndex(null)}
        />
      )}

      <ConfirmDialog
        open={!!pendingDelete}
        title={t('images.deleteTitle')}
        body={t('images.deleteBody')}
        confirmLabel={t('common.delete')}
        loading={deleting}
        onConfirm={confirmDelete}
        onCancel={() => setPendingDelete(null)}
      />
    </>
  )
}

function TileButton({
  label,
  onClick,
  disabled,
  danger,
  pressed,
  children,
}: {
  label: string
  onClick: () => void
  disabled?: boolean
  danger?: boolean
  pressed?: boolean
  children: ReactNode
}) {
  return (
    <button
      type="button"
      onClick={onClick}
      disabled={disabled}
      aria-label={label}
      title={label}
      aria-pressed={pressed}
      className={cn(
        'inline-flex size-8 items-center justify-center rounded-lg text-muted transition-colors hover:bg-surface-2 disabled:opacity-30',
        danger ? 'hover:text-danger' : 'hover:text-fg',
      )}
    >
      {children}
    </button>
  )
}

function CaptionInput({ initial, onSave }: { initial: string; onSave: (c: string | null) => Promise<unknown> }) {
  const { t } = useTranslation()
  const [value, setValue] = useState(initial)
  const commit = async () => {
    const next = value.trim()
    if (next === initial.trim()) return
    try {
      await onSave(next || null)
      toast.success(t('images.captionSaved'))
    } catch (err) {
      toast.error(errorMessage(err))
    }
  }
  return (
    <input
      dir="auto"
      value={value}
      onChange={(e) => setValue(e.target.value)}
      onBlur={() => void commit()}
      onKeyDown={(e) => {
        if (e.key === 'Enter') {
          e.preventDefault()
          ;(e.target as HTMLInputElement).blur()
        }
        if (e.key === 'Escape') setValue(initial)
      }}
      aria-label={t('images.caption')}
      placeholder={t('images.captionPlaceholder')}
      maxLength={300}
      className="w-full bg-transparent px-2.5 py-1.5 text-xs text-fg placeholder:text-subtle focus:bg-surface-2 focus:outline-none"
    />
  )
}
