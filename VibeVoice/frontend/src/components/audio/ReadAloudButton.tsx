import { Info, Sparkles, Volume2 } from 'lucide-react'
import { useId, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { toast } from 'sonner'
import { ApiError, errorMessage } from '@/api/client'
import { useVoiceStatus, useVoices } from '@/api/hooks'
import type { AudioDto } from '@/api/types'
import { useAuth, useCan } from '@/auth/context'
import { Button } from '../ui/Button'
import { ProgressBar } from '../ui/Display'
import { Popover } from '../ui/Popover'
import { VoiceSelect } from './VoiceSelect'
import { isFallbackVoice } from './voiceUtils'

export interface ReadAloudButtonProps {
  /** Calls the TTS endpoint and resolves with the new AI clip. */
  onGenerate: (voice: string | undefined) => Promise<AudioDto>
  /** e.g. flush pending autosave so the server reads the latest text. */
  onBeforeGenerate?: () => Promise<void>
  onGenerated?: (audio: AudioDto) => void
  compact?: boolean
}

/** "Read aloud with VibeVoice" — voice picker popover, progress state, adds the clip. */
function ReadAloudButtonInner({ onGenerate, onBeforeGenerate, onGenerated, compact }: ReadAloudButtonProps) {
  const { t } = useTranslation()
  const { user } = useAuth()
  const status = useVoiceStatus()
  const voicesQuery = useVoices()
  const voices = voicesQuery.data ?? []
  const [picked, setPicked] = useState<string | null>(null)
  const [busy, setBusy] = useState(false)
  const hintId = useId()

  const preferred = user?.preferredVoice && voices.some((v) => v.id === user.preferredVoice) ? user.preferredVoice : null
  const voice = picked ?? preferred ?? voices[0]?.id ?? ''
  const selected = voices.find((v) => v.id === voice)
  const unavailable = status.data ? !status.data.available : status.isError

  const generate = async (close: () => void) => {
    setBusy(true)
    try {
      await onBeforeGenerate?.()
      const audio = await onGenerate(voice || undefined)
      toast.success(t('readAloud.generated'))
      onGenerated?.(audio)
      close()
    } catch (err) {
      toast.error(err instanceof ApiError && err.status === 503 ? t('readAloud.unavailable') : errorMessage(err))
    } finally {
      setBusy(false)
    }
  }

  const label = compact ? t('readAloud.short') : t('readAloud.button')

  if (unavailable) {
    return (
      <span title={t('readAloud.unavailable')} className="inline-flex">
        <Button
          size="sm"
          variant="subtle"
          disabled
          aria-describedby={hintId}
          icon={<Volume2 className="size-4" aria-hidden />}
        >
          {label}
        </Button>
        <span id={hintId} className="sr-only">
          {t('readAloud.unavailable')}
        </span>
      </span>
    )
  }

  return (
    <Popover
      label={t('readAloud.chooseVoice')}
      align="end"
      trigger={(p) => (
        <Button
          size="sm"
          variant="subtle"
          loading={busy}
          icon={<Volume2 className="size-4" aria-hidden />}
          {...p}
        >
          {busy ? t('readAloud.generating') : label}
        </Button>
      )}
    >
      {(close) => (
        <div className="flex flex-col gap-3">
          <div className="flex items-center gap-2 text-sm font-semibold text-fg">
            <Sparkles className="size-4 text-ai" aria-hidden />
            {t('readAloud.button')}
          </div>
          <label className="flex flex-col gap-1.5 text-xs font-medium text-muted">
            {t('readAloud.chooseVoice')}
            {voices.length ? (
              <VoiceSelect voices={voices} value={voice} onChange={setPicked} disabled={busy} />
            ) : (
              <span className="text-sm text-subtle">
                {voicesQuery.isLoading ? t('common.loading') : t('readAloud.noVoices')}
              </span>
            )}
          </label>
          {selected && selected.language === 'ar' && isFallbackVoice(selected) && (
            <p className="flex gap-1.5 rounded-lg bg-warning-soft px-2.5 py-2 text-xs text-warning">
              <Info className="mt-px size-3.5 shrink-0" aria-hidden />
              {t('readAloud.fallbackExplain')}
            </p>
          )}
          {busy && <ProgressBar label={t('readAloud.generating')} />}
          <Button
            variant="primary"
            size="sm"
            loading={busy}
            onClick={() => void generate(close)}
            icon={<Sparkles className="size-4" aria-hidden />}
          >
            {busy ? t('readAloud.generating') : t('readAloud.generate')}
          </Button>
          <p className="text-[11px] leading-snug text-subtle">{t('studio.responsibleUse')}</p>
        </div>
      )}
    </Popover>
  )
}

/** Hidden unless the user's role has `voice.tts`. */
export function ReadAloudButton(props: ReadAloudButtonProps) {
  const allowed = useCan('voice.tts')
  if (!allowed) return null
  return <ReadAloudButtonInner {...props} />
}
