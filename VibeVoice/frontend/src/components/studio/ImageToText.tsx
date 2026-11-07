import { useMutation, useQueryClient } from '@tanstack/react-query'
import { AlertTriangle, FilePlus2, ImagePlus, RefreshCw, ScanText } from 'lucide-react'
import { useEffect, useId, useRef, useState, type DragEvent } from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router'
import { toast } from 'sonner'
import { ApiError, errorMessage } from '@/api/client'
import { useOcrAvailable } from '@/api/hooks'
import { qk } from '@/api/keys'
import { imagesApi } from '@/api/library'
import { notesApi } from '@/api/notes'
import type { AudioDto, OcrResultDto } from '@/api/types'
import { voiceApi } from '@/api/voice'
import { useLatest } from '@/hooks/misc'
import { cn, IMAGE_MAX_BYTES, IMAGE_TYPES, isImageFile, textToHtml } from '@/lib/utils'
import { AudioPlayer } from '../audio/AudioPlayer'
import { ReadAloudButton } from '../audio/ReadAloudButton'
import { OcrTextPanel } from '../images/OcrTextPanel'
import { Button } from '../ui/Button'
import { Card, ProgressBar } from '../ui/Display'
import { Checkbox, Select } from '../ui/Field'
import { toastError } from '@/lib/notify'

const LANGS = ['auto', 'ar', 'en'] as const
type Lang = (typeof LANGS)[number]

function confidencePct(c: number): number {
  return Math.round(c <= 1 ? c * 100 : c)
}

/** Studio tab: picture → text (OCR) → note / speech. */
export function ImageToText() {
  const { t } = useTranslation()
  const qc = useQueryClient()
  const navigate = useNavigate()
  const ocrAvailable = useOcrAvailable()
  const [file, setFile] = useState<File | null>(null)
  const [preview, setPreview] = useState<string | null>(null)
  const [language, setLanguage] = useState<Lang>('auto')
  const [keep, setKeep] = useState(true)
  const [progress, setProgress] = useState<number | null>(null)
  const [result, setResult] = useState<OcrResultDto | null>(null)
  const [speech, setSpeech] = useState<AudioDto | null>(null)
  const [dragging, setDragging] = useState(false)
  const inputRef = useRef<HTMLInputElement>(null)
  const previewRef = useRef<string | null>(null)
  const langId = useId()

  useEffect(
    () => () => {
      if (previewRef.current) URL.revokeObjectURL(previewRef.current)
    },
    [],
  )

  const extract = async (target: File, save: boolean) => {
    if (!ocrAvailable) return
    setProgress(0)
    setSpeech(null)
    try {
      const res = await voiceApi.ocr(target, { language: language === 'auto' ? undefined : language, save }, setProgress)
      setResult(res)
      if (res.image) {
        void qc.invalidateQueries({ queryKey: qk.images.all })
        void qc.invalidateQueries({ queryKey: qk.dashboard })
      }
      if (res.text.trim()) toast.success(t('ocr.extracted'))
    } catch (err) {
      toast.error(err instanceof ApiError && err.status === 503 ? t('ocr.unavailable') : errorMessage(err))
    } finally {
      setProgress(null)
    }
  }

  const pick = (f: File | undefined) => {
    if (!f) return
    if (!isImageFile(f)) {
      toast.error(t('images.invalidType', { name: f.name || 'file' }))
      return
    }
    if (f.size > IMAGE_MAX_BYTES) {
      toast.error(t('images.tooLarge', { name: f.name }))
      return
    }
    if (previewRef.current) URL.revokeObjectURL(previewRef.current)
    const url = URL.createObjectURL(f)
    previewRef.current = url
    setPreview(url)
    setFile(f)
    setResult(null)
    setSpeech(null)
    void extract(f, keep)
  }

  // Paste a picture anywhere while this tab is open.
  const pickRef = useLatest(pick)
  useEffect(() => {
    const onPaste = (e: ClipboardEvent) => {
      const img = Array.from(e.clipboardData?.files ?? []).find((f) => f.type.startsWith('image/'))
      if (!img) return
      e.preventDefault()
      pickRef.current(img)
    }
    window.addEventListener('paste', onPaste)
    return () => window.removeEventListener('paste', onPaste)
  }, [pickRef])

  const onDrop = (e: DragEvent<HTMLDivElement>) => {
    e.preventDefault()
    setDragging(false)
    pick(e.dataTransfer?.files?.[0])
  }

  const text = result?.text.trim() ?? ''

  const saveAsNote = useMutation({
    mutationFn: async () => {
      const firstLine = text.split(/\n/).find((l) => l.trim()) ?? ''
      const note = await notesApi.create({
        title: firstLine.trim().slice(0, 60) || t('notes.untitled'),
        body: textToHtml(text),
        tags: [],
      })
      if (result?.image) await imagesApi.attach(result.image.id, 'Note', note.id)
      return note
    },
    onSuccess: (note) => {
      void qc.invalidateQueries({ queryKey: qk.notes.all })
      void qc.invalidateQueries({ queryKey: qk.images.all })
      void qc.invalidateQueries({ queryKey: qk.dashboard })
      toast.success(t('studio.savedAsNote'))
      navigate(`/notes/${note.id}`)
    },
    onError: (err) => toastError(err),
  })

  const busy = progress !== null

  return (
    <div className="grid gap-6 lg:grid-cols-2">
      <Card className="flex flex-col gap-4 p-5">
        {!ocrAvailable && (
          <p role="alert" className="flex items-start gap-2 rounded-xl bg-warning-soft px-3 py-2 text-sm text-warning">
            <AlertTriangle className="mt-0.5 size-4 shrink-0" aria-hidden />
            {t('ocr.unavailable')}
          </p>
        )}
        <div className="flex flex-wrap items-end gap-4">
          <div className="flex flex-col gap-1">
            <label htmlFor={langId} className="text-xs font-medium text-muted">
              {t('ocr.language')}
            </label>
            <Select id={langId} value={language} onChange={(e) => setLanguage(e.target.value as Lang)} className="h-9 w-56">
              {LANGS.map((l) => (
                <option key={l} value={l}>
                  {t(`ocr.languages.${l}`)}
                </option>
              ))}
            </Select>
          </div>
          <Checkbox label={t('studio.keepInLibrary')} checked={keep} onChange={(e) => setKeep(e.target.checked)} />
        </div>

        <div
          data-testid="ocr-dropzone"
          onDragOver={(e) => {
            e.preventDefault()
            setDragging(true)
          }}
          onDragLeave={() => setDragging(false)}
          onDrop={onDrop}
          className={cn(
            'flex flex-col items-center gap-3 rounded-2xl border-2 border-dashed p-4 text-center transition-colors',
            dragging ? 'border-accent bg-accent-soft/60' : 'border-border-strong',
          )}
        >
          {preview ? (
            <img
              src={preview}
              alt={t('studio.picture')}
              className="max-h-80 w-auto max-w-full rounded-xl object-contain shadow-card"
            />
          ) : (
            <ImagePlus className="mt-4 size-8 text-accent-text" aria-hidden />
          )}
          <p className="text-sm text-muted">
            {t('studio.ocrDrop')}{' '}
            <button
              type="button"
              onClick={() => inputRef.current?.click()}
              className="font-medium text-accent-text underline-offset-2 hover:underline"
            >
              {preview ? t('studio.changePicture') : t('studio.ocrChoose')}
            </button>
          </p>
          <p className="text-xs text-subtle">{t('images.accepted')}</p>
          <input
            ref={inputRef}
            type="file"
            accept={IMAGE_TYPES.join(',')}
            className="sr-only"
            tabIndex={-1}
            aria-hidden
            data-testid="ocr-input"
            onChange={(e) => {
              pick(e.target.files?.[0])
              e.target.value = ''
            }}
          />
        </div>

        {busy && (
          <div className="flex flex-col gap-1.5">
            <span className="text-xs text-muted">{t('ocr.extracting')}</span>
            <ProgressBar value={progress < 1 ? progress : undefined} label={t('ocr.extracting')} />
          </div>
        )}

        {file && (
          <span title={ocrAvailable ? undefined : t('ocr.unavailable')} className="self-start">
            <Button
              variant={result ? 'secondary' : 'primary'}
              loading={busy}
              disabled={!ocrAvailable}
              onClick={() => void extract(file, keep && !result?.image)}
              icon={result ? <RefreshCw className="size-4" aria-hidden /> : <ScanText className="size-4" aria-hidden />}
            >
              {busy ? t('ocr.extracting') : t('ocr.extract')}
            </Button>
          </span>
        )}
      </Card>

      <Card className="flex flex-col gap-4 p-5">
        <h2 className="flex items-center gap-2 text-sm font-semibold text-fg">
          <ScanText className="size-4 text-accent-text" aria-hidden />
          {t('ocr.extractedText')}
        </h2>
        {!result ? (
          <p className="rounded-xl bg-surface-2 p-6 text-center text-sm text-muted">{t('studio.ocrEmpty')}</p>
        ) : !text ? (
          <p role="status" className="rounded-xl bg-surface-2 p-6 text-center text-sm text-muted">
            {t('ocr.noText')}
          </p>
        ) : (
          <>
            {result.lines.length > 0 && (
              <div>
                <h3 className="mb-2 text-xs font-semibold tracking-wide text-subtle uppercase">{t('ocr.lines')}</h3>
                <ol aria-label={t('ocr.lines')} className="scrollbar-thin flex max-h-64 flex-col gap-0.5 overflow-y-auto">
                  {result.lines.map((l, i) => (
                    <li key={i} className="flex items-baseline gap-3 rounded-lg px-2 py-1 text-sm hover:bg-surface-2">
                      <span dir="auto" className="min-w-0 flex-1 text-fg">
                        {l.text}
                      </span>
                      <span
                        className="shrink-0 font-mono text-[11px] text-subtle tabular-nums"
                        title={t('ocr.confidence', { value: confidencePct(l.confidence) })}
                        dir="ltr"
                      >
                        {confidencePct(l.confidence)}%
                      </span>
                    </li>
                  ))}
                </ol>
              </div>
            )}
            <div className="flex flex-col gap-2">
              <h3 className="text-xs font-semibold tracking-wide text-subtle uppercase">{t('ocr.fullText')}</h3>
              <OcrTextPanel text={text} filename={(file?.name ?? 'picture').replace(/\.[^.]+$/, '') + '-text'} />
            </div>
            <div className="flex flex-wrap items-center gap-2 border-t border-border pt-4">
              <Button
                variant="primary"
                size="sm"
                loading={saveAsNote.isPending}
                onClick={() => saveAsNote.mutate()}
                icon={<FilePlus2 className="size-4" aria-hidden />}
              >
                {t('studio.saveAsNote')}
              </Button>
              <ReadAloudButton
                onGenerate={(voice) =>
                  voiceApi.tts({
                    text,
                    voice,
                    language: voice ? undefined : (result.language ?? undefined),
                  })
                }
                onGenerated={(audio) => {
                  setSpeech(audio)
                  void qc.invalidateQueries({ queryKey: qk.voice.histories() })
                  void qc.invalidateQueries({ queryKey: qk.audio.all })
                }}
              />
            </div>
            {speech && <AudioPlayer key={speech.id} audio={speech} />}
          </>
        )}
      </Card>
    </div>
  )
}
