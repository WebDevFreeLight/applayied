import { Mic, Sparkles, Upload, Video } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import type { AudioSource, MediaKind } from '@/api/types'
import { Badge } from '../ui/Display'

/**
 * Recorded / Uploaded / AI voice badge (+ "Video" for video clips).
 * AI clips always carry a visible "AI-generated" label.
 */
export function SourceBadge({
  source,
  isAiGenerated,
  mediaKind,
}: {
  source: AudioSource
  isAiGenerated: boolean
  mediaKind?: MediaKind
}) {
  const { t } = useTranslation()
  const ai = isAiGenerated || source === 'Tts'
  const videoBadge =
    mediaKind === 'Video' ? (
      <Badge tone="warning">
        <Video className="size-3" aria-hidden />
        {t('audio.kinds.Video')}
      </Badge>
    ) : null
  if (ai) {
    return (
      <span className="inline-flex flex-wrap items-center gap-1">
        {videoBadge}
        <Badge tone="ai">
          <Sparkles className="size-3" aria-hidden />
          {t('audio.sources.Tts')}
        </Badge>
        <Badge tone="ai" className="uppercase tracking-wide" title={t('audio.aiGeneratedHint')}>
          {t('audio.aiGenerated')}
        </Badge>
      </span>
    )
  }
  return (
    <span className="inline-flex flex-wrap items-center gap-1">
      {videoBadge}
      <Badge tone={source === 'Recorded' ? 'accent' : 'neutral'}>
        {source === 'Recorded' ? <Mic className="size-3" aria-hidden /> : <Upload className="size-3" aria-hidden />}
        {t(`audio.sources.${source}`)}
      </Badge>
    </span>
  )
}
