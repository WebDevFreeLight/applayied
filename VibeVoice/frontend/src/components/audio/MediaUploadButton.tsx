import { Upload } from 'lucide-react'
import { useId, useRef, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { toast } from 'sonner'
import { errorMessage } from '@/api/client'
import type { MediaKind } from '@/api/types'
import { MEDIA_ACCEPT, validateMediaFile } from '@/lib/utils'
import { Button } from '../ui/Button'
import { ProgressBar } from '../ui/Display'
import { Checkbox, Select } from '../ui/Field'
import { Popover } from '../ui/Popover'

export interface MediaUploadOptions {
  kind: MediaKind
  transcribe: boolean
  /** undefined = auto-detect */
  language?: string
}

export interface MediaUploadButtonProps {
  /** Uploads the file; resolves with the X-Transcription-Error message when auto-transcription failed. */
  upload: (
    file: File,
    options: MediaUploadOptions,
    onProgress: (fraction: number) => void,
  ) => Promise<{ transcriptionError: string | null } | void>
  onUploaded?: () => void
  label?: string
  size?: 'sm' | 'md'
}

const LANGS = ['auto', 'en', 'ar'] as const

/**
 * "Upload audio / video": options popover (auto-transcribe + language), type/size validation with a
 * visible reason for every rejected file, upload progress, and a warning when transcription failed.
 */
export function MediaUploadButton({ upload, onUploaded, label, size = 'sm' }: MediaUploadButtonProps) {
  const { t } = useTranslation()
  const inputRef = useRef<HTMLInputElement>(null)
  const [progress, setProgress] = useState<number | null>(null)
  const [transcribe, setTranscribe] = useState(true)
  const [language, setLanguage] = useState<(typeof LANGS)[number]>('auto')
  const langId = useId()

  const onFile = async (file: File | undefined) => {
    if (!file) return
    const check = validateMediaFile(file)
    if (!check.ok) {
      if (check.reason === 'invalidType') toast.error(t('audio.invalidType', { name: file.name }))
      else
        toast.error(
          t('audio.tooLarge', { name: file.name, kind: t(`audio.kindLower.${check.kind ?? 'Audio'}`), max: check.maxMb }),
        )
      return
    }
    setProgress(0)
    try {
      const res = await upload(
        file,
        { kind: check.kind, transcribe, language: language === 'auto' ? undefined : language },
        setProgress,
      )
      const failure = res ? res.transcriptionError : null
      if (failure) {
        toast.warning(t('audio.transcriptionFailed', { message: failure }))
      } else {
        toast.success(transcribe ? t('audio.uploadedTranscribed') : t('audio.uploaded'))
      }
      onUploaded?.()
    } catch (err) {
      toast.error(errorMessage(err))
    } finally {
      setProgress(null)
    }
  }

  const busy = progress !== null

  return (
    <>
      <input
        ref={inputRef}
        type="file"
        accept={MEDIA_ACCEPT}
        className="sr-only"
        tabIndex={-1}
        aria-hidden
        data-testid="media-input"
        onChange={(e) => {
          void onFile(e.target.files?.[0])
          e.target.value = ''
        }}
      />
      <Popover
        label={label ?? t('audio.uploadAudio')}
        align="end"
        trigger={(p) => (
          <Button size={size} loading={busy} disabled={busy} icon={<Upload className="size-4" aria-hidden />} {...p}>
            {busy ? t('audio.uploading', { percent: Math.round((progress ?? 0) * 100) }) : (label ?? t('audio.uploadAudio'))}
          </Button>
        )}
      >
        {(close) => (
          <div className="flex flex-col gap-3">
            <Checkbox
              label={t('audio.autoTranscribe')}
              checked={transcribe}
              onChange={(e) => setTranscribe(e.target.checked)}
            />
            <p className="-mt-2 text-xs text-subtle">{t('audio.autoTranscribeHint')}</p>
            <div className="flex flex-col gap-1">
              <label htmlFor={langId} className="text-xs font-medium text-muted">
                {t('audio.transcriptionLanguage')}
              </label>
              <Select
                id={langId}
                value={language}
                disabled={!transcribe}
                onChange={(e) => setLanguage(e.target.value as (typeof LANGS)[number])}
                className="h-9"
              >
                {LANGS.map((l) => (
                  <option key={l} value={l}>
                    {t(`studio.languages.${l}`)}
                  </option>
                ))}
              </Select>
            </div>
            <Button
              variant="primary"
              size="sm"
              icon={<Upload className="size-4" aria-hidden />}
              onClick={() => {
                close()
                inputRef.current?.click()
              }}
            >
              {t('audio.chooseFile')}
            </Button>
            <p className="text-xs text-subtle">{t('audio.uploadHint')}</p>
          </div>
        )}
      </Popover>
      {busy && <ProgressBar value={progress ?? 0} label={t('audio.uploadAudio')} className="w-24 self-center" />}
    </>
  )
}
