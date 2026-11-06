import { useQueryClient } from '@tanstack/react-query'
import { Archive, ArchiveRestore, ArrowLeft, Images, Pin, PinOff, Save, Trash2 } from 'lucide-react'
import type { ReactNode, RefObject } from 'react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router'
import { toast } from 'sonner'
import { attachmentsApi, type OwnerKind } from '@/api/attachments'
import type { AudioDto, ImageDto } from '@/api/types'
import type { AutosaveStatus } from '@/hooks/useAutosave'
import { textToHtml } from '@/lib/utils'
import { AudioList } from '../audio/AudioList'
import { ReadAloudButton } from '../audio/ReadAloudButton'
import { ImageGallery } from '../images/ImageGallery'
import { ImageUploader } from '../images/ImageUploader'
import { Button, IconButton } from '../ui/Button'
import { Card, ErrorState, Skeleton } from '../ui/Display'
import { SaveIndicator } from './EditorChrome'
import type { RichTextHandle } from './RichTextEditor'

export function EditorTopBar({
  backTo,
  backLabel,
  status,
  onSave,
  isPinned,
  isArchived,
  onTogglePin,
  onToggleArchive,
  onDelete,
}: {
  backTo: string
  backLabel: string
  status: AutosaveStatus
  onSave: () => void
  isPinned: boolean
  isArchived: boolean
  onTogglePin: () => void
  onToggleArchive: () => void
  onDelete: () => void
}) {
  const { t } = useTranslation()
  return (
    <div className="sticky top-16 z-20 -mx-4 mb-6 flex flex-wrap items-center gap-2 border-b border-border bg-bg/85 px-4 py-2.5 backdrop-blur sm:-mx-6 sm:px-6">
      <Link
        to={backTo}
        className="inline-flex h-9 items-center gap-1.5 rounded-xl px-2 text-sm font-medium text-muted hover:bg-surface-2 hover:text-fg"
      >
        <ArrowLeft className="size-4 rtl:rotate-180" aria-hidden />
        <span className="hidden sm:inline">{backLabel}</span>
      </Link>
      <SaveIndicator status={status} className="ms-1" />
      <div className="ms-auto flex items-center gap-1">
        <IconButton label={isPinned ? t('common.unpin') : t('common.pin')} aria-pressed={isPinned} onClick={onTogglePin}>
          {isPinned ? <PinOff className="size-4" aria-hidden /> : <Pin className="size-4" aria-hidden />}
        </IconButton>
        <IconButton label={isArchived ? t('common.unarchive') : t('common.archive')} aria-pressed={isArchived} onClick={onToggleArchive}>
          {isArchived ? <ArchiveRestore className="size-4" aria-hidden /> : <Archive className="size-4" aria-hidden />}
        </IconButton>
        <IconButton label={t('common.moveToTrash')} onClick={onDelete} className="hover:text-danger">
          <Trash2 className="size-4" aria-hidden />
        </IconButton>
        <Button
          variant="primary"
          size="sm"
          onClick={onSave}
          disabled={status === 'saved'}
          loading={status === 'saving'}
          icon={<Save className="size-4" aria-hidden />}
          title={t('save.shortcut')}
          className="ms-1"
        >
          {t('common.save')}
        </Button>
      </div>
    </div>
  )
}

/** Audio + images sections shared by the note and memory editors. */
export function AttachmentsPanel({
  kind,
  ownerId,
  audio,
  images,
  detailKey,
  editorRef,
  flush,
  recorderOpen,
  coverImageId,
  onSetCover,
}: {
  kind: OwnerKind
  ownerId: string
  audio: AudioDto[]
  images: ImageDto[]
  detailKey: readonly unknown[]
  editorRef: RefObject<RichTextHandle | null>
  flush: () => Promise<void>
  recorderOpen?: boolean
  coverImageId?: string | null
  onSetCover?: (id: string | null) => Promise<unknown>
}) {
  const { t } = useTranslation()
  const qc = useQueryClient()
  const refresh = () => {
    void qc.invalidateQueries({ queryKey: detailKey })
    void qc.invalidateQueries({ queryKey: ['audio'] })
    void qc.invalidateQueries({ queryKey: ['images'] })
    void qc.invalidateQueries({ queryKey: ['dashboard'] })
  }
  const patchImages = (next: ImageDto[]) =>
    qc.setQueryData(detailKey, (old: { images: ImageDto[] } | undefined) => (old ? { ...old, images: next } : old))

  return (
    <div className="flex flex-col gap-8">
      <Card className="p-4 sm:p-5">
        <AudioList
          items={audio}
          ownerKind={kind}
          ownerId={ownerId}
          onChanged={refresh}
          defaultRecorderOpen={recorderOpen}
          onAppendTranscript={(text) => editorRef.current?.appendHtml(textToHtml(text))}
          actions={
            <ReadAloudButton
              compact
              onBeforeGenerate={flush}
              onGenerate={(voice) => attachmentsApi.tts(kind, ownerId, { voice })}
              onGenerated={refresh}
            />
          }
        />
      </Card>

      <Card className="flex flex-col gap-4 p-4 sm:p-5">
        <h2 className="flex items-center gap-2 text-sm font-semibold text-fg">
          <Images className="size-4 text-accent-text" aria-hidden />
          {t('editor.imagesSection')}
          {images.length > 0 && <span className="text-xs font-normal text-subtle">({images.length})</span>}
        </h2>
        <ImageGallery
          images={images}
          coverImageId={coverImageId}
          onSetCover={onSetCover}
          onReorder={async (ids) => {
            const res = await attachmentsApi.reorderImages(kind, ownerId, ids)
            patchImages(res)
            toast.success(t('images.reordered'))
          }}
          onCaption={async (id, caption) => {
            const updated = await attachmentsApi.updateCaption(kind, ownerId, id, caption)
            patchImages(images.map((i) => (i.id === id ? updated : i)))
          }}
          onDelete={async (id) => {
            await attachmentsApi.deleteImage(kind, ownerId, id)
            refresh()
          }}
        />
        <ImageUploader
          compact={images.length > 0}
          upload={(file, onProgress) => attachmentsApi.uploadImages(kind, ownerId, [file], onProgress)}
          onUploaded={refresh}
        />
      </Card>
    </div>
  )
}

export function EditorSkeleton() {
  return (
    <div className="mx-auto flex max-w-3xl flex-col gap-4" aria-hidden>
      <Skeleton className="h-10 w-full" />
      <Skeleton className="h-12 w-2/3" />
      <Skeleton className="h-8 w-1/2" />
      <Skeleton className="h-72 w-full" />
    </div>
  )
}

export function EditorError({ error, onRetry, backTo, backLabel }: { error: unknown; onRetry: () => void; backTo: string; backLabel: ReactNode }) {
  return (
    <div className="mx-auto flex max-w-xl flex-col gap-4">
      <ErrorState error={error} onRetry={onRetry} />
      <Link to={backTo} className="self-center text-sm font-medium text-accent-text hover:underline">
        {backLabel}
      </Link>
    </div>
  )
}
