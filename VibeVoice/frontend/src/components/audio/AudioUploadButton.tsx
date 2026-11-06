import { Upload } from 'lucide-react'
import { useRef, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { toast } from 'sonner'
import { errorMessage } from '@/api/client'
import { AUDIO_ACCEPT, AUDIO_MAX_BYTES, isAudioFile } from '@/lib/utils'
import { Button } from '../ui/Button'

/** Picks an audio file, validates type/size, and uploads it with progress. */
export function AudioUploadButton({
  upload,
  onUploaded,
  label,
  size = 'sm',
}: {
  upload: (file: File, onProgress: (fraction: number) => void) => Promise<unknown>
  onUploaded?: () => void
  label?: string
  size?: 'sm' | 'md'
}) {
  const { t } = useTranslation()
  const inputRef = useRef<HTMLInputElement>(null)
  const [progress, setProgress] = useState<number | null>(null)

  const onFile = async (file: File | undefined) => {
    if (!file) return
    if (!isAudioFile(file)) {
      toast.error(t('audio.invalidType', { name: file.name }))
      return
    }
    if (file.size > AUDIO_MAX_BYTES) {
      toast.error(t('audio.tooLarge', { name: file.name }))
      return
    }
    setProgress(0)
    try {
      await upload(file, setProgress)
      toast.success(t('audio.uploaded'))
      onUploaded?.()
    } catch (err) {
      toast.error(errorMessage(err))
    } finally {
      setProgress(null)
    }
  }

  return (
    <>
      <input
        ref={inputRef}
        type="file"
        accept={AUDIO_ACCEPT}
        className="sr-only"
        tabIndex={-1}
        aria-hidden
        onChange={(e) => {
          void onFile(e.target.files?.[0])
          e.target.value = ''
        }}
      />
      <Button
        size={size}
        loading={progress !== null}
        onClick={() => inputRef.current?.click()}
        icon={<Upload className="size-4" aria-hidden />}
      >
        {progress !== null
          ? t('audio.uploading', { percent: Math.round(progress * 100) })
          : (label ?? t('audio.uploadAudio'))}
      </Button>
    </>
  )
}
