import { AlertCircle, CheckCircle2, ImagePlus, X } from 'lucide-react'
import { useEffect, useId, useRef, useState, type DragEvent } from 'react'
import { useTranslation } from 'react-i18next'
import { toast } from 'sonner'
import { errorMessage } from '@/api/client'
import { useLatest } from '@/hooks/misc'
import { cn, IMAGE_MAX_BYTES, IMAGE_TYPES, isImageFile } from '@/lib/utils'
import { ProgressBar } from '../ui/Display'
import { Checkbox, Select } from '../ui/Field'

interface QueueItem {
  key: string
  name: string
  progress: number
  status: 'uploading' | 'done' | 'error'
  error?: string
}

export interface ImageOcrChoice {
  /** Extract text from the picture after upload (form field `ocr=true`). */
  ocr: boolean
  /** "ar" / "en"; undefined = auto (Arabic + English). */
  language?: string
}

const OCR_LANGS = ['auto', 'ar', 'en'] as const

export interface ImageUploaderProps {
  /**
   * Uploads one file (called once per file, in parallel) and reports 0..1 progress.
   * Resolve with `{ ocrError }` (from the X-Ocr-Error header) to show a warning toast.
   */
  upload: (file: File, onProgress: (fraction: number) => void, options: ImageOcrChoice) => Promise<unknown>
  onUploaded?: (count: number) => void
  /**
   * Shows the "Extract text (OCR)" option (default on) when defined; `false` renders it disabled
   * with an explanation (no OCR engine configured).
   */
  ocrAvailable?: boolean
  /** Listen for images pasted anywhere on the page (default true). */
  enablePaste?: boolean
  compact?: boolean
  className?: string
}

/** Drag & drop / paste / file-picker multi-upload with per-file progress and validation. */
export function ImageUploader({
  upload,
  onUploaded,
  ocrAvailable,
  enablePaste = true,
  compact,
  className,
}: ImageUploaderProps) {
  const { t } = useTranslation()
  const [dragging, setDragging] = useState(false)
  const [queue, setQueue] = useState<QueueItem[]>([])
  const [rejected, setRejected] = useState<string[]>([])
  const inputRef = useRef<HTMLInputElement>(null)
  const seqRef = useRef(0)
  const inputId = useId()
  const ocrLangId = useId()
  const [ocrChecked, setOcrChecked] = useState(true)
  const [ocrLang, setOcrLang] = useState<(typeof OCR_LANGS)[number]>('auto')
  const showOcr = ocrAvailable !== undefined
  const ocrOn = showOcr && !!ocrAvailable && ocrChecked

  const patch = (key: string, p: Partial<QueueItem>) =>
    setQueue((q) => q.map((it) => (it.key === key ? { ...it, ...p } : it)))

  const handleFiles = async (list: FileList | File[] | null | undefined) => {
    const files = Array.from(list ?? [])
    if (!files.length) return
    const problems: string[] = []
    const valid: File[] = []
    for (const f of files) {
      if (!isImageFile(f)) problems.push(t('images.invalidType', { name: f.name || 'file' }))
      else if (f.size > IMAGE_MAX_BYTES) problems.push(t('images.tooLarge', { name: f.name }))
      else valid.push(f)
    }
    setRejected(problems)
    if (!valid.length) return

    const items = valid.map((f) => ({ key: `u${++seqRef.current}`, name: f.name || 'image', progress: 0, status: 'uploading' as const }))
    setQueue((q) => [...q, ...items])

    const choice: ImageOcrChoice = { ocr: ocrOn, language: ocrOn && ocrLang !== 'auto' ? ocrLang : undefined }
    const ocrErrors: string[] = []
    const results = await Promise.allSettled(
      valid.map((file, i) =>
        upload(file, (p) => patch(items[i].key, { progress: p }), choice).then(
          (res) => {
            const ocrError = (res as { ocrError?: unknown } | undefined)?.ocrError
            if (typeof ocrError === 'string' && ocrError) ocrErrors.push(ocrError)
            patch(items[i].key, { status: 'done', progress: 1 })
          },
          (err: unknown) => {
            patch(items[i].key, { status: 'error', error: errorMessage(err) })
            throw err
          },
        ),
      ),
    )
    const ok = results.filter((r) => r.status === 'fulfilled').length
    const failed = results.length - ok
    if (ok) {
      toast.success(t('images.uploaded', { count: ok }))
      onUploaded?.(ok)
    }
    if (failed) toast.error(t('images.uploadFailed'))
    // Text extraction failures never fail the upload — warn once per batch.
    if (ocrErrors.length) toast.warning(t('ocr.failed', { message: [...new Set(ocrErrors)].join(' · ') }))
    setTimeout(() => {
      const done = new Set(items.filter((_, i) => results[i].status === 'fulfilled').map((it) => it.key))
      setQueue((q) => q.filter((it) => !done.has(it.key)))
    }, 1500)
  }

  const handleFilesRef = useLatest(handleFiles)

  useEffect(() => {
    if (!enablePaste) return
    const onPaste = (e: ClipboardEvent) => {
      const files = Array.from(e.clipboardData?.files ?? []).filter((f) => f.type.startsWith('image/'))
      if (!files.length) return
      e.preventDefault()
      void handleFilesRef.current(files)
    }
    window.addEventListener('paste', onPaste)
    return () => window.removeEventListener('paste', onPaste)
  }, [enablePaste, handleFilesRef])

  const onDrop = (e: DragEvent<HTMLDivElement>) => {
    e.preventDefault()
    setDragging(false)
    void handleFiles(e.dataTransfer?.files)
  }

  return (
    <div className={cn('flex flex-col gap-2', className)}>
      <div
        data-testid="image-dropzone"
        onDragOver={(e) => {
          e.preventDefault()
          if (!dragging) setDragging(true)
        }}
        onDragEnter={(e) => {
          e.preventDefault()
          setDragging(true)
        }}
        onDragLeave={(e) => {
          if (!e.currentTarget.contains(e.relatedTarget as Node | null)) setDragging(false)
        }}
        onDrop={onDrop}
        className={cn(
          'flex flex-col items-center justify-center gap-2 rounded-2xl border-2 border-dashed text-center transition-colors',
          compact ? 'px-4 py-4' : 'px-6 py-8',
          dragging ? 'border-accent bg-accent-soft/60' : 'border-border-strong bg-surface-2/40 hover:border-accent/60',
        )}
      >
        <ImagePlus className={cn('text-accent-text', compact ? 'size-5' : 'size-7')} aria-hidden />
        <p className="text-sm text-muted">
          {dragging ? (
            t('images.dropzoneActive')
          ) : (
            <>
              {t('images.dropzone')}{' '}
              <label htmlFor={inputId} className="cursor-pointer font-medium text-accent-text underline-offset-2 hover:underline">
                {t('images.choose')}
              </label>
            </>
          )}
        </p>
        <p className="text-xs text-subtle">{t('images.accepted')}</p>
        <input
          ref={inputRef}
          id={inputId}
          type="file"
          multiple
          accept={IMAGE_TYPES.join(',')}
          className="sr-only"
          data-testid="image-input"
          onChange={(e) => {
            void handleFiles(e.target.files)
            e.target.value = ''
          }}
        />
      </div>

      {showOcr && (
        <div
          className="flex flex-wrap items-center gap-x-4 gap-y-2 px-1"
          title={ocrAvailable ? t('ocr.extractHint') : t('ocr.unavailable')}
        >
          <Checkbox
            label={t('ocr.extractOption')}
            checked={ocrOn}
            disabled={!ocrAvailable}
            onChange={(e) => setOcrChecked(e.target.checked)}
          />
          <span className="flex items-center gap-2">
            <label htmlFor={ocrLangId} className="text-xs text-muted">
              {t('ocr.language')}
            </label>
            <Select
              id={ocrLangId}
              value={ocrLang}
              disabled={!ocrOn}
              onChange={(e) => setOcrLang(e.target.value as (typeof OCR_LANGS)[number])}
              className="h-8 w-auto py-0 text-xs"
            >
              {OCR_LANGS.map((l) => (
                <option key={l} value={l}>
                  {t(`ocr.languages.${l}`)}
                </option>
              ))}
            </Select>
          </span>
          {!ocrAvailable && <span className="text-xs text-warning">{t('ocr.unavailable')}</span>}
        </div>
      )}

      {rejected.length > 0 && (
        <div role="alert" className="flex items-start gap-2 rounded-xl bg-danger-soft/70 px-3 py-2 text-sm text-danger">
          <AlertCircle className="mt-0.5 size-4 shrink-0" aria-hidden />
          <ul className="flex-1">
            {rejected.map((r) => (
              <li key={r}>{r}</li>
            ))}
          </ul>
          <button
            type="button"
            onClick={() => setRejected([])}
            aria-label={t('common.dismiss')}
            className="rounded p-0.5 hover:bg-danger/10"
          >
            <X className="size-4" aria-hidden />
          </button>
        </div>
      )}

      {queue.length > 0 && (
        <ul className="flex flex-col gap-2" aria-label={t('images.uploading')}>
          {queue.map((it) => (
            <li key={it.key} className="flex items-center gap-3 rounded-xl border border-border bg-surface px-3 py-2">
              <span className="min-w-0 flex-1">
                <span className="block truncate text-xs font-medium text-fg">{it.name}</span>
                {it.status === 'error' ? (
                  <span className="text-xs text-danger">{it.error}</span>
                ) : (
                  <ProgressBar value={it.progress} className="mt-1" label={it.name} />
                )}
              </span>
              {it.status === 'done' && <CheckCircle2 className="size-4 text-success" aria-hidden />}
              {it.status === 'error' && (
                <button
                  type="button"
                  aria-label={t('common.dismiss')}
                  onClick={() => setQueue((q) => q.filter((x) => x.key !== it.key))}
                >
                  <X className="size-4 text-muted" aria-hidden />
                </button>
              )}
              {it.status === 'uploading' && (
                <span className="font-mono text-xs tabular-nums text-muted" dir="ltr">
                  {Math.round(it.progress * 100)}%
                </span>
              )}
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}
