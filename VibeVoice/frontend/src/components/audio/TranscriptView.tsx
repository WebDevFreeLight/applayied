import { Copy, Download } from 'lucide-react'
import { useEffect, useRef } from 'react'
import { useTranslation } from 'react-i18next'
import { toast } from 'sonner'
import { transcriptUrl } from '@/api/client'
import type { Segment, TranscriptFormat } from '@/api/types'
import { copyText } from '@/hooks/misc'
import { cn, formatDuration } from '@/lib/utils'

const FORMATS: TranscriptFormat[] = ['txt', 'srt', 'vtt']

export interface TranscriptViewProps {
  audioId: string
  text: string
  segments: Segment[]
  /** Current playback position in seconds. */
  currentTime: number
  playing: boolean
  /** Seek to `seconds` and start playing. */
  onSeek: (seconds: number) => void
}

/** Index of the segment containing `time` (or -1). */
function activeSegmentIndex(segments: Segment[], time: number): number {
  return segments.findIndex((s) => time >= s.start && time < s.end)
}

/** Timestamped, clickable transcript with copy + TXT/SRT/VTT downloads. */
export function TranscriptView({ audioId, text, segments, currentTime, playing, onSeek }: TranscriptViewProps) {
  const { t } = useTranslation()
  const active = playing ? activeSegmentIndex(segments, currentTime) : -1
  const listRef = useRef<HTMLOListElement>(null)

  // Keep the active segment visible while playing.
  useEffect(() => {
    if (active < 0) return
    const el = listRef.current?.querySelector<HTMLElement>(`[data-index="${active}"]`)
    el?.scrollIntoView?.({ block: 'nearest' })
  }, [active])

  const fullText = text || segments.map((s) => s.text).join(' ')

  return (
    <div className="flex flex-col gap-2">
      <div className="flex flex-wrap items-center gap-1.5">
        <button
          type="button"
          onClick={() => void copyText(fullText).then((ok) => ok && toast.success(t('common.copied')))}
          className="inline-flex h-7 pointer-coarse:h-10 items-center gap-1 rounded-lg border border-border px-2 text-xs font-medium text-muted hover:bg-surface-2 hover:text-fg"
        >
          <Copy className="size-3.5" aria-hidden />
          {t('transcript.copy')}
        </button>
        <span className="ms-1 inline-flex items-center gap-1 text-xs text-subtle">
          <Download className="size-3.5" aria-hidden />
          {t('transcript.download')}
        </span>
        {FORMATS.map((f) => (
          <a
            key={f}
            href={transcriptUrl(audioId, f)}
            download
            aria-label={t('transcript.downloadAs', { format: f.toUpperCase() })}
            className="inline-flex h-7 pointer-coarse:h-10 items-center rounded-lg border border-border px-2 font-mono text-[11px] font-semibold text-muted uppercase hover:bg-surface-2 hover:text-fg"
          >
            {f}
          </a>
        ))}
      </div>

      {segments.length > 0 ? (
        <ol
          ref={listRef}
          aria-label={t('transcript.segments')}
          className="scrollbar-thin flex max-h-72 flex-col gap-0.5 overflow-y-auto rounded-xl bg-surface-2 p-1.5"
        >
          {segments.map((s, i) => (
            <li key={`${s.start}-${i}`} data-index={i}>
              <button
                type="button"
                onClick={() => onSeek(s.start)}
                aria-current={i === active ? 'true' : undefined}
                aria-label={`${t('transcript.seekTo', { time: formatDuration(s.start) })}: ${s.text}`}
                className={cn(
                  'flex w-full items-start gap-3 rounded-lg px-2 py-1.5 text-start text-sm transition-colors pointer-coarse:py-2.5',
                  i === active ? 'bg-accent-soft text-accent-text' : 'text-fg hover:bg-surface-3',
                )}
              >
                <span className="shrink-0 font-mono text-xs text-subtle tabular-nums" dir="ltr">
                  {formatDuration(s.start)}
                </span>
                {s.speaker && <span className="shrink-0 text-xs font-medium text-accent-text">{s.speaker}</span>}
                <span dir="auto" className="min-w-0 flex-1">
                  {s.text}
                </span>
              </button>
            </li>
          ))}
        </ol>
      ) : (
        <p dir="auto" className="rounded-xl bg-surface-2 px-3 py-2 text-sm leading-relaxed whitespace-pre-wrap text-fg">
          {fullText}
        </p>
      )}
    </div>
  )
}
