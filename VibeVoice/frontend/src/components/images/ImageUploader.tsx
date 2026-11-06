import { AlertCircle, CheckCircle2, ImagePlus, X } from 'lucide-react'
import { useEffect, useId, useRef, useState, type DragEvent } from 'react'
import { useTranslation } from 'react-i18next'
import { toast } from 'sonner'
import { errorMessage } from '@/api/client'
import { useLatest } from '@/hooks/misc'
import { cn, IMAGE_MAX_BYTES, IMAGE_TYPES, isImageFile } from '@/lib/utils'
import { ProgressBar } from '../ui/Display'

interface QueueItem {
  key: string
  name: string
  progress: number
  status: 'uploading' | 'done' | 'error'
  error?: string
}

export interface ImageUploaderProps {
  /** Uploads one file (called once per file, in parallel) and reports 0..1 progress. */
  upload: (file: File, onProgress: (fraction: number) => void) => Promise<unknown>
  onUploaded?: (count: number) => void
  /** Listen for images pasted anywhere on the page (default true). */
  enablePaste?: boolean
  compact?: boolean
  className?: string
}

/** Drag & drop / paste / file-picker multi-upload with per-file progress and validation. */
export function ImageUploader({ upload, onUploaded, enablePaste = true, compact, className }: ImageUploaderProps) {
  const { t } = useTranslation()
  const [dragging, setDragging] = useState(false)
  const [queue, setQueue] = useState<QueueItem[]>([])
  const [rejected, setRejected] = useState<string[]>([])
  const inputRef = useRef<HTMLInputElement>(null)
  const seqRef = useRef(0)
  const inputId = useId()

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

    const results = await Promise.allSettled(
      valid.map((file, i) =>
        upload(file, (p) => patch(items[i].key, { progress: p })).then(
          () => patch(items[i].key, { status: 'done', progress: 1 }),
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
