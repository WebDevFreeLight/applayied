import { useQuery, useQueryClient } from '@tanstack/react-query'
import { Check, Palette } from 'lucide-react'
import { useCallback, useMemo, useRef, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate, useParams, useSearchParams } from 'react-router'
import { toast } from 'sonner'
import { z } from 'zod'
import { useTags } from '@/api/hooks'
import { qk } from '@/api/keys'
import { notesApi } from '@/api/notes'
import type { NoteDto } from '@/api/types'
import { UnsavedChangesDialog } from '@/components/editor/EditorChrome'
import { AttachmentsPanel, EditorError, EditorSkeleton, EditorTopBar } from '@/components/editor/EditorParts'
import { RichTextEditor, type RichTextHandle } from '@/components/editor/RichTextEditor'
import { TagInput } from '@/components/TagInput'
import { useHotkey } from '@/hooks/misc'
import { useAutosave } from '@/hooks/useAutosave'
import { useUnsavedGuard } from '@/hooks/useUnsavedGuard'
import { cn, NOTE_COLORS, noteColorVar } from '@/lib/utils'
import { toastError } from '@/lib/notify'

export function NoteEditorPage() {
  const { id = '' } = useParams()
  const { t } = useTranslation()
  const q = useQuery({
    queryKey: qk.notes.detail(id),
    queryFn: ({ signal }) => notesApi.get(id, signal),
    enabled: !!id,
  })
  if (!q.data) {
    if (q.error)
      return <EditorError error={q.error} onRetry={() => void q.refetch()} backTo="/notes" backLabel={t('nav.notes')} />
    return <EditorSkeleton />
  }
  return <NoteEditor key={id} note={q.data} />
}

interface NoteDraft {
  title: string
  body: string
  color: string | null
  tags: string[]
}

/** `note` is the live query data; local draft state is initialised from it once. */
function NoteEditor({ note }: { note: NoteDto }) {
  const { t } = useTranslation()
  const qc = useQueryClient()
  const navigate = useNavigate()
  const [params] = useSearchParams()
  const tags = useTags()
  const editorRef = useRef<RichTextHandle>(null)
  const [title, setTitle] = useState(note.title)
  const [body, setBody] = useState(note.body)
  const [color, setColor] = useState<string | null>(note.color)
  const [tagList, setTagList] = useState<string[]>(note.tags)
  const draft = useMemo<NoteDraft>(() => ({ title, body, color, tags: tagList }), [title, body, color, tagList])
  const detailKey = qk.notes.detail(note.id)

  const titleSchema = z.string().max(200, t('validation.titleTooLong'))
  const titleError = titleSchema.safeParse(title).error?.issues[0]?.message

  const save = useCallback(
    async (v: NoteDraft) => {
      const res = await notesApi.update(note.id, {
        title: v.title.trim().slice(0, 200) || t('notes.untitled'),
        body: v.body,
        color: v.color,
        tags: v.tags,
      })
      qc.setQueryData(qk.notes.detail(note.id), res)
      void qc.invalidateQueries({ queryKey: qk.notes.lists() })
      void qc.invalidateQueries({ queryKey: qk.dashboard })
      void qc.invalidateQueries({ queryKey: qk.tags })
    },
    [note.id, qc, t],
  )

  const autosave = useAutosave(draft, save, { delay: 1500 })
  const { blocker, bypass } = useUnsavedGuard(autosave.isDirty)
  const flush = autosave.flush
  const saveNow = () => flush().catch((err: unknown) => toastError(err))
  useHotkey('s', () => void saveNow(), { mod: true })

  const update = async (fn: () => Promise<NoteDto>) => {
    try {
      const res = await fn()
      qc.setQueryData(detailKey, res)
      void qc.invalidateQueries({ queryKey: qk.notes.lists() })
    } catch (err) {
      toastError(err)
    }
  }

  const moveToTrash = async () => {
    try {
      await flush().catch(() => undefined)
      await notesApi.remove(note.id)
      void qc.invalidateQueries({ queryKey: qk.notes.all })
      void qc.invalidateQueries({ queryKey: qk.dashboard })
      toast(t('notes.deleted'), {
        action: {
          label: t('notes.deletedUndo'),
          onClick: () => {
            void notesApi.restore(note.id).then(() => {
              void qc.invalidateQueries({ queryKey: qk.notes.all })
              navigate(`/notes/${note.id}`)
            })
          },
        },
      })
      bypass()
      navigate('/notes')
    } catch (err) {
      toastError(err)
    }
  }

  return (
    <div className="mx-auto max-w-3xl">
      <EditorTopBar
        backTo="/notes"
        backLabel={t('nav.notes')}
        status={autosave.status}
        onSave={() => void saveNow()}
        isPinned={note.isPinned}
        isArchived={note.isArchived}
        onTogglePin={() => void update(() => notesApi.pin(note.id, !note.isPinned))}
        onToggleArchive={() => void update(() => notesApi.archive(note.id, !note.isArchived))}
        onDelete={() => void moveToTrash()}
      />

      <div
        className="mb-8 flex flex-col gap-4 rounded-3xl p-1 transition-colors"
        style={color ? { backgroundColor: noteColorVar(color), padding: '1rem' } : undefined}
      >
        <div>
          <input
            dir="auto"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            placeholder={t('notes.titlePlaceholder')}
            aria-label={t('notes.titlePlaceholder')}
            aria-invalid={!!titleError || undefined}
            maxLength={200}
            className="w-full bg-transparent text-2xl font-bold tracking-tight text-fg placeholder:text-subtle focus:outline-none sm:text-3xl"
          />
          {titleError && <p className="mt-1 text-xs text-danger">{titleError}</p>}
        </div>

        <div className="flex flex-wrap items-center gap-2" role="radiogroup" aria-label={t('notes.color')}>
          <Palette className="size-4 text-subtle" aria-hidden />
          {[null, ...NOTE_COLORS].map((c) => {
            const active = color === c
            const label = t(`notes.colors.${c ?? 'default'}`)
            return (
              <button
                key={c ?? 'default'}
                type="button"
                role="radio"
                aria-checked={active}
                aria-label={label}
                title={label}
                onClick={() => setColor(c)}
                className={cn(
                  'flex size-7 items-center justify-center rounded-full border transition-transform hover:scale-110',
                  active ? 'border-accent ring-2 ring-accent/30' : 'border-border-strong',
                )}
                style={{ backgroundColor: c ? noteColorVar(c) : 'var(--surface)' }}
              >
                {active && <Check className="size-3.5 text-fg" aria-hidden />}
              </button>
            )
          })}
        </div>

        <TagInput
          label={t('tags.label')}
          value={tagList}
          onChange={setTagList}
          suggestions={tags.data}
          placeholder={t('tags.placeholder')}
        />

        <RichTextEditor
          handleRef={editorRef}
          initialHtml={note.body}
          onChange={setBody}
          placeholder={t('notes.bodyPlaceholder')}
          label={t('editor.content')}
        />
      </div>

      <AttachmentsPanel
        kind="notes"
        ownerId={note.id}
        audio={note.audio}
        images={note.images}
        detailKey={detailKey}
        editorRef={editorRef}
        flush={flush}
        recorderOpen={params.get('record') === '1'}
      />

      <UnsavedChangesDialog blocker={blocker} onSaveAndLeave={flush} />
    </div>
  )
}
