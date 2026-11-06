import { Mic, Sparkles, Upload } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import type { AudioSource } from '@/api/types'
import { Badge } from '../ui/Display'

/** Recorded / Uploaded / AI voice badge. AI clips always carry a visible "AI-generated" label. */
export function SourceBadge({ source, isAiGenerated }: { source: AudioSource; isAiGenerated: boolean }) {
  const { t } = useTranslation()
  const ai = isAiGenerated || source === 'Tts'
  if (ai) {
    return (
      <span className="inline-flex flex-wrap items-center gap-1">
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
    <Badge tone={source === 'Recorded' ? 'accent' : 'neutral'}>
      {source === 'Recorded' ? <Mic className="size-3" aria-hidden /> : <Upload className="size-3" aria-hidden />}
      {t(`audio.sources.${source}`)}
    </Badge>
  )
}
