import { ChevronLeft, ChevronRight, Download, X } from 'lucide-react'
import { useEffect, useRef } from 'react'
import { createPortal } from 'react-dom'
import { useTranslation } from 'react-i18next'
import { fileUrl } from '@/api/client'
import { useLatest } from '@/hooks/misc'
import { downloadUrl } from '@/lib/utils'

export interface LightboxImage {
  id: string
  filePath: string
  caption: string | null
  width?: number
  height?: number
  mimeType?: string
}

/** Full-screen image viewer. Keyboard: ←/→ to navigate (mirrored in RTL), Esc to close. */
export function Lightbox({
  images,
  index,
  onIndexChange,
  onClose,
}: {
  images: LightboxImage[]
  index: number
  onIndexChange: (i: number) => void
  onClose: () => void
}) {
  const { t } = useTranslation()
  const closeRef = useRef<HTMLButtonElement>(null)
  const count = images.length
  const img = images[index]
  const prev = () => onIndexChange((index - 1 + count) % count)
  const next = () => onIndexChange((index + 1) % count)
  const keyRef = useLatest({ prev, next, onClose })

  useEffect(() => {
    const previouslyFocused = document.activeElement as HTMLElement | null
    closeRef.current?.focus()
    const prevOverflow = document.body.style.overflow
    document.body.style.overflow = 'hidden'
    const onKey = (e: KeyboardEvent) => {
      const rtl = document.documentElement.dir === 'rtl'
      if (e.key === 'Escape') {
        e.preventDefault()
        keyRef.current.onClose()
      } else if (e.key === 'ArrowLeft') {
        e.preventDefault()
        if (rtl) keyRef.current.next()
        else keyRef.current.prev()
      } else if (e.key === 'ArrowRight') {
        e.preventDefault()
        if (rtl) keyRef.current.prev()
        else keyRef.current.next()
      }
    }
    document.addEventListener('keydown', onKey)
    return () => {
      document.removeEventListener('keydown', onKey)
      document.body.style.overflow = prevOverflow
      previouslyFocused?.focus?.()
    }
  }, [keyRef])

  if (!img) return null

  return createPortal(
    <div
      role="dialog"
      aria-modal="true"
      aria-label={t('images.lightbox')}
      className="fixed inset-0 z-[60] flex animate-fade-in flex-col bg-black/90 text-white"
    >
      <div className="flex items-center justify-between gap-2 p-3">
        <span className="text-sm text-white/70 tabular-nums" aria-live="polite">
          {t('images.position', { current: index + 1, total: count })}
        </span>
        <div className="flex items-center gap-1">
          <button
            type="button"
            onClick={() => void downloadUrl(fileUrl(img.filePath), img.filePath.split('/').pop() ?? 'image')}
            aria-label={t('common.download')}
            title={t('common.download')}
            className="rounded-full p-2 hover:bg-white/10"
          >
            <Download className="size-5" aria-hidden />
          </button>
          <button
            ref={closeRef}
            type="button"
            onClick={onClose}
            aria-label={t('common.close')}
            title={t('common.close')}
            className="rounded-full p-2 hover:bg-white/10"
          >
            <X className="size-5" aria-hidden />
          </button>
        </div>
      </div>

      <div className="relative flex min-h-0 flex-1 items-center justify-center px-2 sm:px-14" onClick={onClose}>
        <img
          key={img.id}
          src={fileUrl(img.filePath)}
          alt={img.caption ?? ''}
          className="max-h-full max-w-full animate-pop-in rounded-lg object-contain shadow-2xl"
          onClick={(e) => e.stopPropagation()}
        />
        {count > 1 && (
          <>
            <button
              type="button"
              onClick={(e) => {
                e.stopPropagation()
                prev()
              }}
              aria-label={t('common.previous')}
              className="absolute start-2 top-1/2 -translate-y-1/2 rounded-full bg-white/10 p-2.5 backdrop-blur hover:bg-white/20 sm:start-4"
            >
              <ChevronLeft className="size-6 rtl:rotate-180" aria-hidden />
            </button>
            <button
              type="button"
              onClick={(e) => {
                e.stopPropagation()
                next()
              }}
              aria-label={t('common.next')}
              className="absolute end-2 top-1/2 -translate-y-1/2 rounded-full bg-white/10 p-2.5 backdrop-blur hover:bg-white/20 sm:end-4"
            >
              <ChevronRight className="size-6 rtl:rotate-180" aria-hidden />
            </button>
          </>
        )}
      </div>

      {img.caption && (
        <p dir="auto" className="mx-auto max-w-2xl px-4 pt-2 pb-5 text-center text-sm text-white/85">
          {img.caption}
        </p>
      )}
    </div>,
    document.body,
  )
}
