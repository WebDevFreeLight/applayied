import { AudioLines, Mic, X } from 'lucide-react'
import { useState, type ReactNode } from 'react'
import { useTranslation } from 'react-i18next'
import { attachmentsApi, type OwnerKind } from '@/api/attachments'
import type { AudioDto } from '@/api/types'
import { Button } from '../ui/Button'
import { AudioPlayer } from './AudioPlayer'
import { AudioRecorder } from './AudioRecorder'
import { AudioUploadButton } from './AudioUploadButton'

export interface AudioListProps {
  items: AudioDto[]
  ownerKind: OwnerKind
  ownerId: string
  /** Refresh the owner after any change. */
  onChanged: () => void
  /** When provided, "Append to text" inserts the transcript into the open editor. */
  onAppendTranscript?: (text: string) => void
  defaultRecorderOpen?: boolean
  /** Extra actions in the header (e.g. Read aloud). */
  actions?: ReactNode
}

/** Audio section of a note / memory: recorder, upload, and the clip list. */
export function AudioList({
  items,
  ownerKind,
  ownerId,
  onChanged,
  onAppendTranscript,
  defaultRecorderOpen = false,
  actions,
}: AudioListProps) {
  const { t } = useTranslation()
  const [recorderOpen, setRecorderOpen] = useState(defaultRecorderOpen)
  const sorted = [...items].sort((a, b) => b.createdAt.localeCompare(a.createdAt))

  const transcribe = async (audio: AudioDto, append: boolean) => {
    if (onAppendTranscript) {
      // Append client-side so the editor (and autosave) stay the single source of truth.
      const updated = await attachmentsApi.transcribe(ownerKind, ownerId, audio.id, { appendToBody: false })
      if (append && updated.transcript) onAppendTranscript(updated.transcript)
      onChanged()
      return updated
    }
    const updated = await attachmentsApi.transcribe(ownerKind, ownerId, audio.id, { appendToBody: append })
    onChanged()
    return updated
  }

  return (
    <section aria-labelledby={`audio-${ownerId}`} className="flex flex-col gap-3">
      <div className="flex flex-wrap items-center justify-between gap-2">
        <h2 id={`audio-${ownerId}`} className="flex items-center gap-2 text-sm font-semibold text-fg">
          <AudioLines className="size-4 text-accent-text" aria-hidden />
          {t('editor.audioSection')}
          {items.length > 0 && <span className="text-xs font-normal text-subtle">({items.length})</span>}
        </h2>
        <div className="flex flex-wrap items-center gap-2">
          {actions}
          <Button
            size="sm"
            variant={recorderOpen ? 'ghost' : 'secondary'}
            onClick={() => setRecorderOpen((o) => !o)}
            aria-expanded={recorderOpen}
            icon={recorderOpen ? <X className="size-4" aria-hidden /> : <Mic className="size-4" aria-hidden />}
          >
            {recorderOpen ? t('audio.hideRecorder') : t('audio.record')}
          </Button>
          <AudioUploadButton
            upload={(file, onProgress) =>
              attachmentsApi.uploadAudio(ownerKind, ownerId, file, 'Uploaded', null, onProgress)
            }
            onUploaded={onChanged}
          />
        </div>
      </div>

      {recorderOpen && (
        <AudioRecorder
          onSave={(file, durationSeconds, onProgress) =>
            attachmentsApi.uploadAudio(ownerKind, ownerId, file, 'Recorded', durationSeconds, onProgress)
          }
          onSaved={onChanged}
        />
      )}

      {sorted.length > 0 && (
        <ul className="flex flex-col gap-3" aria-label={t('audio.listLabel')}>
          {sorted.map((a) => (
            <li key={a.id}>
              <AudioPlayer
                audio={a}
                canAppend
                onTranscribe={(append) => transcribe(a, append)}
                onDelete={async () => {
                  await attachmentsApi.deleteAudio(ownerKind, ownerId, a.id)
                  onChanged()
                }}
              />
            </li>
          ))}
        </ul>
      )}
    </section>
  )
}
