import {
  ChevronDown,
  Download,
  ExternalLink,
  FileText,
  Loader2,
  Pause,
  Play,
  Trash2,
  Wand2,
} from 'lucide-react'
import { useContext, useEffect, useRef, useState, type KeyboardEvent } from 'react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router'
import { toast } from 'sonner'
import WaveSurfer from 'wavesurfer.js'
import { ApiError, errorMessage, fileUrl } from '@/api/client'
import type { AudioDto } from '@/api/types'
import { useDates } from '@/lib/dates'
import { cn, downloadUrl, extensionForMime, formatBytes, formatDuration } from '@/lib/utils'
import { ThemeContext } from '@/theme/theme'
import { Button, IconButton } from '../ui/Button'
import { Checkbox } from '../ui/Field'
import { ConfirmDialog } from '../ui/Modal'
import { SourceBadge } from './SourceBadge'
import { TranscriptView } from './TranscriptView'

export interface AudioPlayerProps {
  audio: AudioDto
  /** Shown when provided. */
  onDelete?: () => Promise<unknown>
  /** Shown when provided. Receives the "append to body" choice. */
  onTranscribe?: (appendToBody: boolean) => Promise<AudioDto>
  /** Show the "Append to text" option next to Transcribe. */
  canAppend?: boolean
  owner?: { to: string; title: string; kindLabel: string }
  /** Expand the transcript initially (e.g. right after a Studio transcription). */
  defaultShowTranscript?: boolean
  className?: string
}

function cssVar(name: string, fallback: string): string {
  if (typeof window === 'undefined') return fallback
  const v = getComputedStyle(document.documentElement).getPropertyValue(name).trim()
  return v || fallback
}

function safePlay(el: HTMLMediaElement) {
  try {
    const p = el.play() as Promise<void> | undefined
    p?.catch?.(() => undefined)
  } catch {
    /* autoplay blocked / not supported */
  }
}

/**
 * Media clip card. Audio → wavesurfer waveform; Video → native <video>. Shows source/AI badges,
 * Transcribe, and a clickable timestamped transcript that seeks the media.
 */
export function AudioPlayer({
  audio,
  onDelete,
  onTranscribe,
  canAppend,
  owner,
  defaultShowTranscript = false,
  className,
}: AudioPlayerProps) {
  const { t } = useTranslation()
  const dates = useDates()
  const themeCtx = useContext(ThemeContext)
  const resolvedTheme = themeCtx?.resolved
  const palette = themeCtx?.palette
  const containerRef = useRef<HTMLDivElement>(null)
  const videoRef = useRef<HTMLVideoElement>(null)
  const fallbackRef = useRef<HTMLAudioElement>(null)
  const wsRef = useRef<WaveSurfer | null>(null)
  const [ready, setReady] = useState(false)
  const [playing, setPlaying] = useState(false)
  const [duration, setDuration] = useState<number | null>(audio.durationSeconds)
  const [current, setCurrent] = useState(0)
  const [loadError, setLoadError] = useState(false)
  const [transcribed, setTranscribed] = useState<AudioDto | null>(null)
  const [showTranscript, setShowTranscript] = useState(defaultShowTranscript)
  const [transcribing, setTranscribing] = useState(false)
  const [append, setAppend] = useState(false)
  const [confirmDelete, setConfirmDelete] = useState(false)
  const [deleting, setDeleting] = useState(false)
  const url = fileUrl(audio.filePath)
  const ai = audio.isAiGenerated || audio.source === 'Tts'
  const isVideo = audio.mediaKind === 'Video'
  const latest = transcribed ?? audio
  const transcriptText = latest.transcript ?? ''
  const segments = latest.segments ?? []
  const hasTranscript = !!transcriptText || segments.length > 0

  useEffect(() => {
    if (isVideo) return
    const container = containerRef.current
    if (!container || !url) return
    const ws = WaveSurfer.create({
      container,
      url,
      height: 44,
      waveColor: cssVar('--border-strong', '#cfcfdc'),
      progressColor: cssVar(ai ? '--ai' : '--accent', '#c2338e'),
      cursorColor: cssVar('--text', '#17171f'),
      cursorWidth: 1,
      barWidth: 2,
      barGap: 2,
      barRadius: 2,
      normalize: true,
      dragToSeek: true,
    })
    wsRef.current = ws
    ws.on('ready', (d) => {
      setReady(true)
      if (Number.isFinite(d) && d > 0) setDuration(d)
    })
    ws.on('timeupdate', (time) => setCurrent(time))
    ws.on('play', () => setPlaying(true))
    ws.on('pause', () => setPlaying(false))
    ws.on('finish', () => setPlaying(false))
    ws.on('error', () => setLoadError(true))
    return () => {
      wsRef.current = null
      ws.destroy()
    }
  }, [url, ai, isVideo, resolvedTheme, palette])

  const toggle = () => {
    const ws = wsRef.current
    if (!ws) return
    void ws.playPause()
  }

  /** Seeks the audio/video to `seconds` and plays. */
  const seek = (seconds: number) => {
    if (isVideo) {
      const v = videoRef.current
      if (!v) return
      v.currentTime = seconds
      setCurrent(seconds)
      safePlay(v)
      return
    }
    const ws = wsRef.current
    if (ws && !loadError) {
      ws.setTime(seconds)
      setCurrent(seconds)
      void ws.play()?.catch?.(() => undefined)
      return
    }
    // Fallback <audio> element (when the waveform failed to load).
    const el = fallbackRef.current
    if (el) {
      el.currentTime = seconds
      safePlay(el)
    }
  }

  const onWaveKey = (e: KeyboardEvent<HTMLDivElement>) => {
    const ws = wsRef.current
    if (!ws) return
    const rtl = document.documentElement.dir === 'rtl'
    if (e.key === ' ' || e.key === 'Enter') {
      e.preventDefault()
      toggle()
    } else if (e.key === 'ArrowRight' || e.key === 'ArrowLeft') {
      e.preventDefault()
      const forward = (e.key === 'ArrowRight') !== rtl
      const d = ws.getDuration() || 0
      ws.setTime(Math.max(0, Math.min(d, ws.getCurrentTime() + (forward ? 5 : -5))))
    } else if (e.key === 'Home') {
      e.preventDefault()
      ws.setTime(0)
    } else if (e.key === 'End') {
      e.preventDefault()
      ws.setTime(ws.getDuration() || 0)
    }
  }

  const transcribe = async () => {
    if (!onTranscribe) return
    setTranscribing(true)
    try {
      const updated = await onTranscribe(append)
      setTranscribed(updated)
      setShowTranscript(true)
      toast.success(t('audio.transcribed'))
    } catch (err) {
      toast.error(err instanceof ApiError && err.status === 503 ? t('errors.voiceUnavailable') : errorMessage(err))
    } finally {
      setTranscribing(false)
    }
  }

  const remove = async () => {
    if (!onDelete) return
    setDeleting(true)
    try {
      await onDelete()
      toast.success(t('audio.deleted'))
      setConfirmDelete(false)
    } catch (err) {
      toast.error(errorMessage(err))
    } finally {
      setDeleting(false)
    }
  }

  const ext = isVideo ? (audio.filePath.split('.').pop() ?? 'mp4') : extensionForMime(audio.mimeType)
  const filename = `${ai ? `vibevoice-${audio.voiceName ?? 'tts'}` : isVideo ? 'video' : 'audio'}-${audio.id.slice(0, 8)}.${ext}`

  return (
    <article
      className={cn(
        'rounded-2xl border bg-surface p-3 shadow-card sm:p-4',
        ai ? 'border-ai/30' : 'border-border',
        className,
      )}
      aria-label={
        ai
          ? `${t('audio.aiGenerated')} — ${audio.voiceName ?? ''}`
          : `${t(`audio.kinds.${isVideo ? 'Video' : 'Audio'}`)} · ${t(`audio.sources.${audio.source}`)}`
      }
    >
      {isVideo ? (
        <video
          ref={videoRef}
          src={url}
          controls
          preload="metadata"
          playsInline
          data-testid="clip-video"
          className="aspect-video max-h-[60vh] w-full rounded-xl bg-black"
          onLoadedMetadata={(e) => {
            const d = e.currentTarget.duration
            if (Number.isFinite(d) && d > 0) setDuration(d)
          }}
          onTimeUpdate={(e) => setCurrent(e.currentTarget.currentTime)}
          onPlay={() => setPlaying(true)}
          onPause={() => setPlaying(false)}
          onEnded={() => setPlaying(false)}
        />
      ) : (
        <div className="flex items-center gap-3">
          <button
            type="button"
            onClick={toggle}
            disabled={!ready && !loadError}
            aria-label={playing ? t('audio.pause') : t('audio.play')}
            title={playing ? t('audio.pause') : t('audio.play')}
            className={cn(
              'flex size-11 shrink-0 items-center justify-center rounded-full shadow-sm transition-transform hover:scale-105 disabled:opacity-50',
              ai ? 'bg-ai text-ai-fg' : 'bg-accent text-accent-fg',
              loadError && 'hidden',
            )}
          >
            {!ready && !loadError ? (
              <Loader2 className="size-5 animate-spin" aria-hidden />
            ) : playing ? (
              <Pause className="size-5" aria-hidden />
            ) : (
              <Play className="size-5" aria-hidden />
            )}
          </button>

          <div className="min-w-0 flex-1">
            {loadError ? (
              <audio
                ref={fallbackRef}
                controls
                src={url}
                preload="metadata"
                className="h-10 w-full"
                aria-label={t('audio.play')}
                onTimeUpdate={(e) => setCurrent(e.currentTarget.currentTime)}
                onPlay={() => setPlaying(true)}
                onPause={() => setPlaying(false)}
              />
            ) : (
              <div
                ref={containerRef}
                role="slider"
                tabIndex={0}
                aria-label={t('audio.seek')}
                aria-valuemin={0}
                aria-valuemax={Math.round(duration ?? 0)}
                aria-valuenow={Math.round(current)}
                aria-valuetext={`${formatDuration(current)} / ${formatDuration(duration)}`}
                onKeyDown={onWaveKey}
                dir="ltr"
                className="h-11 w-full cursor-pointer rounded-lg"
              />
            )}
          </div>

          <span className="hidden shrink-0 font-mono text-xs tabular-nums text-muted sm:inline" dir="ltr">
            {formatDuration(current)} / {formatDuration(duration)}
          </span>
        </div>
      )}

      <div className="mt-3 flex flex-wrap items-center gap-x-3 gap-y-2">
        <SourceBadge source={audio.source} isAiGenerated={audio.isAiGenerated} mediaKind={audio.mediaKind} />
        {ai && audio.voiceName && (
          <span className="text-xs text-muted">
            {t('audio.voice')}: <bdi className="font-medium text-fg">{audio.voiceName}</bdi>
            {audio.ttsEngine && <> · {audio.ttsEngine}</>}
          </span>
        )}
        <span className="text-xs text-subtle">
          {isVideo && duration ? `${formatDuration(duration)} · ` : ''}
          {dates.relative(audio.createdAt)} · {formatBytes(audio.sizeBytes)}
        </span>
        {owner && (
          <Link
            to={owner.to}
            className="inline-flex min-w-0 items-center gap-1 text-xs font-medium text-accent-text hover:underline"
          >
            <ExternalLink className="size-3 shrink-0" aria-hidden />
            <span className="text-subtle">{owner.kindLabel}:</span>
            <bdi className="truncate">{owner.title}</bdi>
          </Link>
        )}

        <div className="ms-auto flex flex-wrap items-center gap-1">
          {onTranscribe && !ai && (
            <>
              {canAppend && (
                <Checkbox
                  label={t('audio.appendToBody')}
                  checked={append}
                  onChange={(e) => setAppend(e.target.checked)}
                  className="me-1 text-xs"
                />
              )}
              <Button
                size="sm"
                variant="subtle"
                onClick={transcribe}
                loading={transcribing}
                icon={<Wand2 className="size-3.5" aria-hidden />}
              >
                {transcribing ? t('audio.transcribing') : t('audio.transcribe')}
              </Button>
            </>
          )}
          <IconButton label={t('common.download')} size="icon-sm" onClick={() => void downloadUrl(url, filename)}>
            <Download className="size-4" aria-hidden />
          </IconButton>
          {onDelete && (
            <IconButton
              label={t('common.delete')}
              size="icon-sm"
              className="hover:text-danger"
              onClick={() => setConfirmDelete(true)}
            >
              <Trash2 className="size-4" aria-hidden />
            </IconButton>
          )}
        </div>
      </div>

      {ai && audio.text && (
        <p className="mt-3 line-clamp-3 rounded-xl bg-ai-soft/40 px-3 py-2 text-sm text-fg" dir="auto">
          <span className="me-1 text-xs font-medium text-ai">{t('audio.generatedFrom')}:</span>
          {audio.text}
        </p>
      )}

      {hasTranscript && (
        <div className="mt-3">
          <button
            type="button"
            onClick={() => setShowTranscript((s) => !s)}
            aria-expanded={showTranscript}
            className="inline-flex items-center gap-1 text-xs font-medium text-muted hover:text-fg"
          >
            <FileText className="size-3.5" aria-hidden />
            {showTranscript ? t('audio.hideTranscript') : t('audio.showTranscript')}
            <ChevronDown className={cn('size-3.5 transition-transform', showTranscript && 'rotate-180')} aria-hidden />
          </button>
          {showTranscript && (
            <div className="mt-2">
              <TranscriptView
                audioId={audio.id}
                text={transcriptText}
                segments={segments}
                currentTime={current}
                playing={playing}
                onSeek={seek}
              />
            </div>
          )}
        </div>
      )}

      <ConfirmDialog
        open={confirmDelete}
        title={t('audio.deleteTitle')}
        body={t('audio.deleteBody')}
        confirmLabel={t('common.delete')}
        loading={deleting}
        onConfirm={remove}
        onCancel={() => setConfirmDelete(false)}
      />
    </article>
  )
}
