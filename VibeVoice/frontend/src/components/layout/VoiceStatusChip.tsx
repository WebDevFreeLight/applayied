import { useTranslation } from 'react-i18next'
import { useVoiceStatus } from '@/api/hooks'
import { cn } from '@/lib/utils'

/** Small live indicator of the VibeVoice service health. */
export function VoiceStatusChip({ className, showLabel = true }: { className?: string; showLabel?: boolean }) {
  const { t } = useTranslation()
  const { data, isLoading, isError } = useVoiceStatus()
  const available = !!data?.available
  const label = isLoading ? t('voice.statusChecking') : available ? t('voice.statusReady') : t('voice.statusOffline')
  const detail = data
    ? data.available
      ? t('voice.statusDetail', {
          tts: data.ttsEngine ?? '—',
          asr: data.asrEngine ?? '—',
          device: data.device ?? '—',
        })
      : (data.message ?? t('errors.voiceUnavailable'))
    : isError
      ? t('errors.voiceUnavailable')
      : label

  return (
    <span
      role="status"
      title={detail}
      aria-label={`${label}. ${detail}`}
      className={cn(
        'inline-flex h-8 items-center gap-2 rounded-full border px-3 text-xs font-medium whitespace-nowrap',
        isLoading
          ? 'border-border text-muted'
          : available
            ? 'border-success/30 bg-success-soft text-success'
            : 'border-danger/30 bg-danger-soft text-danger',
        className,
      )}
    >
      <span className="relative flex size-2">
        {available && <span className="absolute inline-flex size-full animate-ping rounded-full bg-current opacity-50" />}
        <span className="relative inline-flex size-2 rounded-full bg-current" />
      </span>
      {showLabel && <span className="hidden sm:inline">{label}</span>}
    </span>
  )
}
