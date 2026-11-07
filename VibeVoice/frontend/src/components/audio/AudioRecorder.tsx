import { Mic, MicOff, Pause, Play, Save, Square, Trash2 } from 'lucide-react'
import { useEffect, useRef, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { toast } from 'sonner'
import { cn, extensionForMime, formatDuration } from '@/lib/utils'
import { Button } from '../ui/Button'
import { ProgressBar } from '../ui/Display'
import {
  classifyMediaError,
  isInsecureContext,
  isRecordingSupported,
  pickRecorderMimeType,
  type RecorderErrorKind,
} from './recording'
import { toastError } from '@/lib/notify'

type Phase = 'idle' | 'requesting' | 'recording' | 'paused' | 'stopped' | 'saving' | 'error'

export interface AudioRecorderProps {
  /**
   * Persists the recording (e.g. multipart upload with `source=Recorded` + `durationSeconds`).
   * `onProgress` receives a 0..1 upload fraction.
   */
  onSave: (file: File, durationSeconds: number, onProgress: (fraction: number) => void) => Promise<unknown>
  onSaved?: () => void
  saveLabel?: string
  className?: string
}

function timestamp(): string {
  const d = new Date()
  const p = (n: number) => String(n).padStart(2, '0')
  return `${d.getFullYear()}${p(d.getMonth() + 1)}${p(d.getDate())}-${p(d.getHours())}${p(d.getMinutes())}${p(d.getSeconds())}`
}

/** MediaRecorder-based voice recorder with live timer, level meter and preview. */
export function AudioRecorder({ onSave, onSaved, saveLabel, className }: AudioRecorderProps) {
  const { t } = useTranslation()
  const [phase, setPhase] = useState<Phase>('idle')
  const [errorKind, setErrorKind] = useState<RecorderErrorKind | null>(null)
  const [elapsedMs, setElapsedMs] = useState(0)
  const [level, setLevel] = useState(0)
  const [previewUrl, setPreviewUrl] = useState<string | null>(null)
  const [progress, setProgress] = useState(0)

  const recorderRef = useRef<MediaRecorder | null>(null)
  const streamRef = useRef<MediaStream | null>(null)
  const chunksRef = useRef<Blob[]>([])
  const blobRef = useRef<Blob | null>(null)
  const audioCtxRef = useRef<AudioContext | null>(null)
  const rafRef = useRef<number | null>(null)
  const intervalRef = useRef<ReturnType<typeof setInterval> | null>(null)
  const startedAtRef = useRef(0)
  const accumulatedRef = useRef(0)
  const durationRef = useRef(0)
  const previewUrlRef = useRef<string | null>(null)

  const stopMeters = () => {
    if (intervalRef.current) clearInterval(intervalRef.current)
    intervalRef.current = null
    if (rafRef.current !== null) cancelAnimationFrame(rafRef.current)
    rafRef.current = null
  }

  const releaseDevices = () => {
    stopMeters()
    streamRef.current?.getTracks().forEach((tr) => tr.stop())
    streamRef.current = null
    const ctx = audioCtxRef.current
    audioCtxRef.current = null
    if (ctx && ctx.state !== 'closed') void ctx.close().catch(() => undefined)
  }

  const revokePreview = () => {
    if (previewUrlRef.current) URL.revokeObjectURL(previewUrlRef.current)
    previewUrlRef.current = null
  }

  // Release the microphone, analyser and preview URL on unmount.
  useEffect(
    () => () => {
      const rec = recorderRef.current
      if (rec && rec.state !== 'inactive') {
        rec.onstop = null
        try {
          rec.stop()
        } catch {
          /* ignore */
        }
      }
      if (intervalRef.current) clearInterval(intervalRef.current)
      if (rafRef.current !== null) cancelAnimationFrame(rafRef.current)
      streamRef.current?.getTracks().forEach((tr) => tr.stop())
      const ctx = audioCtxRef.current
      if (ctx && ctx.state !== 'closed') void ctx.close().catch(() => undefined)
      if (previewUrlRef.current) URL.revokeObjectURL(previewUrlRef.current)
    },
    [],
  )

  const currentElapsed = () =>
    accumulatedRef.current + (startedAtRef.current ? performance.now() - startedAtRef.current : 0)

  const startTicking = () => {
    startedAtRef.current = performance.now()
    intervalRef.current = setInterval(() => setElapsedMs(currentElapsed()), 200)
  }

  const startLevelMeter = (stream: MediaStream) => {
    const Ctx =
      window.AudioContext ??
      (window as unknown as { webkitAudioContext?: typeof AudioContext }).webkitAudioContext
    if (!Ctx) return
    try {
      const ctx = new Ctx()
      const source = ctx.createMediaStreamSource(stream)
      const analyser = ctx.createAnalyser()
      analyser.fftSize = 1024
      source.connect(analyser)
      audioCtxRef.current = ctx
      const buf = new Uint8Array(analyser.fftSize)
      const tick = () => {
        analyser.getByteTimeDomainData(buf)
        let sum = 0
        for (let i = 0; i < buf.length; i++) {
          const v = (buf[i] - 128) / 128
          sum += v * v
        }
        const rms = Math.sqrt(sum / buf.length)
        setLevel(Math.min(1, rms * 3.2))
        rafRef.current = requestAnimationFrame(tick)
      }
      rafRef.current = requestAnimationFrame(tick)
    } catch {
      /* the meter is optional */
    }
  }

  const fail = (kind: RecorderErrorKind) => {
    releaseDevices()
    setErrorKind(kind)
    setPhase('error')
  }

  const start = async () => {
    if (isInsecureContext()) {
      fail('insecure')
      return
    }
    if (!isRecordingSupported()) {
      fail('unsupported')
      return
    }
    setErrorKind(null)
    setPhase('requesting')
    let stream: MediaStream
    try {
      stream = await navigator.mediaDevices.getUserMedia({ audio: true })
    } catch (err) {
      fail(classifyMediaError(err))
      return
    }
    streamRef.current = stream
    try {
      const mimeType = pickRecorderMimeType()
      const rec = mimeType ? new MediaRecorder(stream, { mimeType }) : new MediaRecorder(stream)
      chunksRef.current = []
      rec.ondataavailable = (e: BlobEvent) => {
        if (e.data && e.data.size > 0) chunksRef.current.push(e.data)
      }
      rec.onstop = () => {
        const type = (rec.mimeType || mimeType || 'audio/webm').split(';')[0]
        const blob = new Blob(chunksRef.current, { type })
        blobRef.current = blob
        revokePreview()
        const url = URL.createObjectURL(blob)
        previewUrlRef.current = url
        setPreviewUrl(url)
        setPhase('stopped')
      }
      rec.onerror = () => fail('failed')
      recorderRef.current = rec
      accumulatedRef.current = 0
      setElapsedMs(0)
      rec.start(250)
      startTicking()
      startLevelMeter(stream)
      setPhase('recording')
    } catch (err) {
      fail(classifyMediaError(err))
    }
  }

  const pause = () => {
    const rec = recorderRef.current
    if (!rec || rec.state !== 'recording') return
    rec.pause()
    accumulatedRef.current = currentElapsed()
    startedAtRef.current = 0
    stopMeters()
    setLevel(0)
    setElapsedMs(accumulatedRef.current)
    setPhase('paused')
  }

  const resume = () => {
    const rec = recorderRef.current
    if (!rec || rec.state !== 'paused') return
    rec.resume()
    startTicking()
    if (streamRef.current && audioCtxRef.current) {
      // Restart the analyser loop on the existing context.
      const stream = streamRef.current
      void audioCtxRef.current.close().catch(() => undefined)
      audioCtxRef.current = null
      startLevelMeter(stream)
    }
    setPhase('recording')
  }

  const stop = () => {
    const rec = recorderRef.current
    if (!rec || rec.state === 'inactive') return
    durationRef.current = currentElapsed()
    accumulatedRef.current = durationRef.current
    startedAtRef.current = 0
    setElapsedMs(durationRef.current)
    setLevel(0)
    rec.stop()
    releaseDevices()
  }

  const discard = () => {
    revokePreview()
    setPreviewUrl(null)
    blobRef.current = null
    chunksRef.current = []
    recorderRef.current = null
    accumulatedRef.current = 0
    setElapsedMs(0)
    setProgress(0)
    setPhase('idle')
  }

  const save = async () => {
    const blob = blobRef.current
    if (!blob) return
    const type = blob.type || 'audio/webm'
    const file = new File([blob], `recording-${timestamp()}.${extensionForMime(type)}`, { type })
    setProgress(0)
    setPhase('saving')
    try {
      await onSave(file, Math.round(durationRef.current / 100) / 10, setProgress)
      toast.success(t('recorder.saved'))
      discard()
      onSaved?.()
    } catch (err) {
      toastError(err)
      setPhase('stopped')
    }
  }

  const live = phase === 'recording' || phase === 'paused'

  return (
    <section
      aria-label={t('recorder.title')}
      className={cn('rounded-2xl border border-border bg-surface-2/50 p-4', className)}
    >
      {phase === 'idle' && (
        <div className="flex flex-wrap items-center gap-3">
          <Button variant="primary" onClick={start} icon={<Mic className="size-4" aria-hidden />}>
            {t('recorder.start')}
          </Button>
          <span className="text-sm text-muted">{t('recorder.title')}</span>
        </div>
      )}

      {phase === 'requesting' && (
        <p role="status" className="flex items-center gap-2 text-sm text-muted">
          <Mic className="size-4 animate-pulse-soft" aria-hidden />
          {t('recorder.requesting')}
        </p>
      )}

      {live && (
        <div className="flex flex-col gap-3">
          <div className="flex flex-wrap items-center gap-3">
            <span
              className={cn(
                'inline-flex items-center gap-2 rounded-full px-2.5 py-1 text-xs font-semibold',
                phase === 'recording' ? 'bg-danger-soft text-danger' : 'bg-surface-3 text-muted',
              )}
            >
              <span
                className={cn('size-2 rounded-full bg-current', phase === 'recording' && 'animate-pulse-soft')}
                aria-hidden
              />
              {phase === 'recording' ? t('recorder.recording') : t('recorder.paused')}
            </span>
            <span
              className="font-mono text-lg font-semibold tabular-nums text-fg"
              aria-live="off"
              data-testid="recorder-timer"
              dir="ltr"
            >
              {formatDuration(elapsedMs / 1000)}
            </span>
            <div
              role="meter"
              aria-label={t('recorder.level')}
              aria-valuemin={0}
              aria-valuemax={100}
              aria-valuenow={Math.round(level * 100)}
              className="flex h-6 min-w-24 flex-1 items-end gap-0.5"
            >
              {Array.from({ length: 16 }, (_, i) => {
                const on = level * 16 > i
                return (
                  <span
                    key={i}
                    className={cn(
                      'w-full rounded-sm transition-[height,background-color] duration-75',
                      on ? (i > 12 ? 'bg-danger' : i > 9 ? 'bg-warning' : 'bg-accent') : 'bg-surface-3',
                    )}
                    style={{ height: `${30 + (i / 16) * 70}%` }}
                  />
                )
              })}
            </div>
          </div>
          <div className="flex flex-wrap gap-2">
            {phase === 'recording' ? (
              <Button size="sm" onClick={pause} icon={<Pause className="size-4" aria-hidden />}>
                {t('recorder.pause')}
              </Button>
            ) : (
              <Button size="sm" onClick={resume} icon={<Play className="size-4" aria-hidden />}>
                {t('recorder.resume')}
              </Button>
            )}
            <Button size="sm" variant="danger" onClick={stop} icon={<Square className="size-3.5" aria-hidden />}>
              {t('recorder.stop')}
            </Button>
          </div>
        </div>
      )}

      {(phase === 'stopped' || phase === 'saving') && previewUrl && (
        <div className="flex flex-col gap-3">
          <p className="text-sm text-muted">{t('recorder.preview')}</p>
          <div className="flex flex-wrap items-center gap-3">
            <audio
              controls
              src={previewUrl}
              className="h-10 w-full max-w-md"
              data-testid="recorder-preview"
              aria-label={t('recorder.preview')}
            />
            <span className="font-mono text-sm tabular-nums text-muted" dir="ltr">
              {formatDuration(elapsedMs / 1000)}
            </span>
          </div>
          {phase === 'saving' && <ProgressBar value={progress} label={t('recorder.saving')} />}
          <div className="flex flex-wrap gap-2">
            <Button
              size="sm"
              variant="ghost"
              onClick={discard}
              disabled={phase === 'saving'}
              icon={<Trash2 className="size-4" aria-hidden />}
            >
              {t('recorder.discard')}
            </Button>
            <Button
              size="sm"
              variant="primary"
              onClick={save}
              loading={phase === 'saving'}
              icon={<Save className="size-4" aria-hidden />}
            >
              {phase === 'saving' ? t('recorder.saving') : (saveLabel ?? t('recorder.save'))}
            </Button>
          </div>
        </div>
      )}

      {phase === 'error' && errorKind && (
        <div role="alert" className="flex flex-col gap-3 rounded-xl bg-danger-soft/60 p-3 text-sm text-danger sm:flex-row sm:items-center">
          <MicOff className="size-5 shrink-0" aria-hidden />
          <p className="flex-1">{t(`recorder.${errorKind}`)}</p>
          {errorKind !== 'unsupported' && errorKind !== 'insecure' && (
            <Button size="sm" onClick={start}>
              {t('recorder.tryAgain')}
            </Button>
          )}
        </div>
      )}
    </section>
  )
}
