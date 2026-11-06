import { ChevronLeft, ChevronRight, Download, Loader2, ScanText, X } from 'lucide-react'
import { useEffect, useRef, useState } from 'react'
import { createPortal } from 'react-dom'
import { useTranslation } from 'react-i18next'
import { toast } from 'sonner'
import { errorMessage, fileUrl } from '@/api/client'
import { useLatest } from '@/hooks/misc'
import { downloadUrl } from '@/lib/utils'
import { OcrTextPanel } from './OcrTextPanel'

export interface LightboxImage {
  id: string
  filePath: string
  caption: string | null
  ocrText?: string | null
  width?: number
  height?: number
  mimeType?: string
}

export interface LightboxProps {
  images: LightboxImage[]
  index: number
  onIndexChange: (i: number) => void
  onClose: () => void
  /** Shows an "Extract text" action for pictures without text. */
  onExtractText?: (id: string) => Promise<unknown>
  ocrAvailable?: boolean
  /** Inserts extracted text into the open editor. */
  onInsertText?: (text: string) => void
  insertLabel?: string
}

/**
 * Full-screen image viewer. Keyboard: ←/→ to navigate (mirrored in RTL), Esc to close.
 * Extracted text is shown side-by-side (stacked on mobile).
 */
export function Lightbox({
  images,
  index,
  onIndexChange,
  onClose,
  onExtractText,
  ocrAvailable = true,
  onInsertText,
  insertLabel,
}: LightboxProps) {
  const { t } = useTranslation()
  const closeRef = useRef<HTMLButtonElement>(null)
  const [extracting, setExtracting] = useState<string | null>(null)
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
      const target = e.target as HTMLElement | null
      if (target && /^(INPUT|TEXTAREA)$/.test(target.tagName)) return
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

  const extract = async () => {
    if (!onExtractText) return
    setExtracting(img.id)
    try {
      await onExtractText(img.id)
      toast.success(t('ocr.extracted'))
    } catch (err) {
      toast.error(errorMessage(err))
    } finally {
      setExtracting(null)
    }
  }

  const hasText = !!img.ocrText
  const showPanel = hasText || !!onExtractText
  const baseName = (img.filePath.split('/').pop() ?? 'image').replace(/\.[^.]+$/, '')

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

      <div className="flex min-h-0 flex-1 flex-col gap-3 overflow-y-auto pb-4 md:flex-row md:overflow-hidden">
        <div className="relative flex min-h-[45vh] flex-1 items-center justify-center px-2 sm:px-14 md:min-h-0" onClick={onClose}>
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

        {showPanel && (
          <aside
            aria-label={t('ocr.extractedText')}
            className="mx-3 flex shrink-0 flex-col gap-3 rounded-2xl bg-white/5 p-4 md:me-4 md:ms-0 md:max-h-full md:w-96"
          >
            <h2 className="flex items-center gap-2 text-sm font-semibold text-white">
              <ScanText className="size-4" aria-hidden />
              {t('ocr.extractedText')}
            </h2>
            {hasText ? (
              <OcrTextPanel
                text={img.ocrText ?? ''}
                filename={`${baseName}-text`}
                onInsert={onInsertText}
                insertLabel={insertLabel}
                tone="dark"
                className="md:min-h-0 md:flex-1"
              />
            ) : (
              <div className="flex flex-col gap-2 text-sm text-white/70">
                <p>{img.ocrText === '' ? t('ocr.noText') : t('ocr.extractHint')}</p>
                <span title={ocrAvailable ? undefined : t('ocr.unavailable')} className="self-start">
                  <button
                    type="button"
                    onClick={() => void extract()}
                    disabled={!ocrAvailable || extracting === img.id}
                    className="inline-flex h-9 items-center gap-2 rounded-xl bg-white/15 px-3 text-sm font-medium text-white hover:bg-white/25 disabled:opacity-50"
                  >
                    {extracting === img.id ? (
                      <Loader2 className="size-4 animate-spin" aria-hidden />
                    ) : (
                      <ScanText className="size-4" aria-hidden />
                    )}
                    {extracting === img.id ? t('ocr.extracting') : t('ocr.extract')}
                  </button>
                </span>
              </div>
            )}
          </aside>
        )}
      </div>

      {img.caption && (
        <p dir="auto" className="mx-auto max-w-2xl px-4 pt-1 pb-5 text-center text-sm text-white/85">
          {img.caption}
        </p>
      )}
    </div>,
    document.body,
  )
}
