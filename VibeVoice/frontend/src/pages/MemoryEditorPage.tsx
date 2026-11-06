import { useQuery, useQueryClient } from '@tanstack/react-query'
import { CalendarDays, MapPin, Smile } from 'lucide-react'
import { useCallback, useMemo, useRef, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate, useParams } from 'react-router'
import { toast } from 'sonner'
import { z } from 'zod'
import { errorMessage } from '@/api/client'
import { usePeople, useTags } from '@/api/hooks'
import { qk } from '@/api/keys'
import { memoriesApi } from '@/api/memories'
import { MOODS, type MemoryDto, type Mood } from '@/api/types'
import { UnsavedChangesDialog } from '@/components/editor/EditorChrome'
import { AttachmentsPanel, EditorError, EditorSkeleton, EditorTopBar } from '@/components/editor/EditorParts'
import { RichTextEditor, type RichTextHandle } from '@/components/editor/RichTextEditor'
import { TagInput } from '@/components/TagInput'
import { Field, Input, Select } from '@/components/ui/Field'
import { useHotkey } from '@/hooks/misc'
import { useAutosave } from '@/hooks/useAutosave'
import { useUnsavedGuard } from '@/hooks/useUnsavedGuard'
import { MOOD_EMOJI, todayIso } from '@/lib/utils'

export function MemoryEditorPage() {
  const { id = '' } = useParams()
  const { t } = useTranslation()
  const q = useQuery({
    queryKey: qk.memories.detail(id),
    queryFn: ({ signal }) => memoriesApi.get(id, signal),
    enabled: !!id,
  })
  if (!q.data) {
    if (q.error)
      return <EditorError error={q.error} onRetry={() => void q.refetch()} backTo="/memories" backLabel={t('nav.memories')} />
    return <EditorSkeleton />
  }
  return <MemoryEditor key={id} memory={q.data} />
}

interface MemoryDraft {
  title: string
  story: string
  memoryDate: string
  location: string
  mood: Mood | null
  tags: string[]
  people: string[]
}

/** `memory` is the live query data; local draft state is initialised from it once. */
function MemoryEditor({ memory }: { memory: MemoryDto }) {
  const { t } = useTranslation()
  const qc = useQueryClient()
  const navigate = useNavigate()
  const tags = useTags()
  const people = usePeople()
  const editorRef = useRef<RichTextHandle>(null)
  const [title, setTitle] = useState(memory.title)
  const [story, setStory] = useState(memory.story)
  const [memoryDate, setMemoryDate] = useState(memory.memoryDate.slice(0, 10))
  const [location, setLocation] = useState(memory.location ?? '')
  const [mood, setMood] = useState<Mood | null>(memory.mood)
  const [tagList, setTagList] = useState(memory.tags)
  const [peopleList, setPeopleList] = useState(memory.people)
  const detailKey = qk.memories.detail(memory.id)

  const draft = useMemo<MemoryDraft>(
    () => ({ title, story, memoryDate, location, mood, tags: tagList, people: peopleList }),
    [title, story, memoryDate, location, mood, tagList, peopleList],
  )

  const schema = z.object({
    title: z.string().max(200, t('validation.titleTooLong')),
    memoryDate: z.string().regex(/^\d{4}-\d{2}-\d{2}$/, t('validation.dateRequired')),
  })
  const check = schema.safeParse(draft)
  const issues = check.success ? {} : Object.fromEntries(check.error.issues.map((i) => [String(i.path[0]), i.message]))

  const save = useCallback(
    async (v: MemoryDraft) => {
      const res = await memoriesApi.update(memory.id, {
        title: v.title.trim().slice(0, 200) || t('memories.untitled'),
        story: v.story,
        memoryDate: /^\d{4}-\d{2}-\d{2}$/.test(v.memoryDate) ? v.memoryDate : todayIso(),
        location: v.location.trim() || null,
        mood: v.mood,
        tags: v.tags,
        people: v.people,
      })
      qc.setQueryData(qk.memories.detail(memory.id), res)
      void qc.invalidateQueries({ queryKey: qk.memories.lists() })
      void qc.invalidateQueries({ queryKey: qk.dashboard })
      void qc.invalidateQueries({ queryKey: qk.tags })
      void qc.invalidateQueries({ queryKey: qk.people })
    },
    [memory.id, qc, t],
  )

  const autosave = useAutosave(draft, save, { delay: 1500, enabled: check.success })
  const { blocker, bypass } = useUnsavedGuard(autosave.isDirty)
  const flush = autosave.flush
  const saveNow = () => flush().catch((err: unknown) => toast.error(errorMessage(err)))
  useHotkey('s', () => void saveNow(), { mod: true })

  const update = async (fn: () => Promise<MemoryDto>) => {
    try {
      const res = await fn()
      qc.setQueryData(detailKey, res)
      void qc.invalidateQueries({ queryKey: qk.memories.lists() })
    } catch (err) {
      toast.error(errorMessage(err))
    }
  }

  const moveToTrash = async () => {
    try {
      await flush().catch(() => undefined)
      await memoriesApi.remove(memory.id)
      void qc.invalidateQueries({ queryKey: qk.memories.all })
      void qc.invalidateQueries({ queryKey: qk.dashboard })
      toast(t('memories.deleted'), {
        action: {
          label: t('notes.deletedUndo'),
          onClick: () => {
            void memoriesApi.restore(memory.id).then(() => {
              void qc.invalidateQueries({ queryKey: qk.memories.all })
              navigate(`/memories/${memory.id}`)
            })
          },
        },
      })
      bypass()
      navigate('/memories')
    } catch (err) {
      toast.error(errorMessage(err))
    }
  }

  return (
    <div className="mx-auto max-w-3xl">
      <EditorTopBar
        backTo="/memories"
        backLabel={t('nav.memories')}
        status={autosave.status}
        onSave={() => void saveNow()}
        isPinned={memory.isPinned}
        isArchived={memory.isArchived}
        onTogglePin={() => void update(() => memoriesApi.pin(memory.id, !memory.isPinned))}
        onToggleArchive={() => void update(() => memoriesApi.archive(memory.id, !memory.isArchived))}
        onDelete={() => void moveToTrash()}
      />

      <div className="mb-8 flex flex-col gap-5">
        <div>
          <input
            dir="auto"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            placeholder={t('memories.titlePlaceholder')}
            aria-label={t('memories.titlePlaceholder')}
            aria-invalid={!!issues.title || undefined}
            maxLength={200}
            className="w-full bg-transparent text-2xl font-bold tracking-tight text-fg placeholder:text-subtle focus:outline-none sm:text-3xl"
          />
          {issues.title && <p className="mt-1 text-xs text-danger">{issues.title}</p>}
        </div>

        <div className="grid gap-4 sm:grid-cols-3">
          <Field
            label={
              <span className="inline-flex items-center gap-1.5">
                <CalendarDays className="size-3.5 text-subtle" aria-hidden />
                {t('memories.date')}
              </span>
            }
            error={issues.memoryDate}
          >
            {(p) => <Input {...p} type="date" required value={memoryDate} onChange={(e) => setMemoryDate(e.target.value)} />}
          </Field>
          <Field
            label={
              <span className="inline-flex items-center gap-1.5">
                <MapPin className="size-3.5 text-subtle" aria-hidden />
                {t('memories.location')}
              </span>
            }
          >
            {(p) => (
              <Input
                {...p}
                value={location}
                maxLength={200}
                placeholder={t('memories.locationPlaceholder')}
                onChange={(e) => setLocation(e.target.value)}
              />
            )}
          </Field>
          <Field
            label={
              <span className="inline-flex items-center gap-1.5">
                <Smile className="size-3.5 text-subtle" aria-hidden />
                {t('memories.mood')}
              </span>
            }
          >
            {(p) => (
              <Select {...p} value={mood ?? ''} onChange={(e) => setMood((e.target.value || null) as Mood | null)}>
                <option value="">{t('memories.noMood')}</option>
                {MOODS.map((m) => (
                  <option key={m} value={m}>
                    {MOOD_EMOJI[m]} {t(`memories.moods.${m}`)}
                  </option>
                ))}
              </Select>
            )}
          </Field>
        </div>

        <div className="grid gap-4 sm:grid-cols-2">
          <div className="flex flex-col gap-1.5">
            <span className="text-sm font-medium text-fg">{t('memories.people')}</span>
            <TagInput
              variant="person"
              label={t('memories.people')}
              value={peopleList}
              onChange={setPeopleList}
              suggestions={people.data}
              placeholder={t('memories.peoplePlaceholder')}
              maxLength={100}
            />
          </div>
          <div className="flex flex-col gap-1.5">
            <span className="text-sm font-medium text-fg">{t('tags.label')}</span>
            <TagInput
              label={t('tags.label')}
              value={tagList}
              onChange={setTagList}
              suggestions={tags.data}
              placeholder={t('tags.placeholder')}
            />
          </div>
        </div>

        <RichTextEditor
          handleRef={editorRef}
          initialHtml={memory.story}
          onChange={setStory}
          placeholder={t('memories.storyPlaceholder')}
          label={t('editor.content')}
        />
      </div>

      <AttachmentsPanel
        kind="memories"
        ownerId={memory.id}
        audio={memory.audio}
        images={memory.images}
        detailKey={detailKey}
        editorRef={editorRef}
        flush={flush}
        coverImageId={memory.coverImageId}
        onSetCover={async (imageId) => {
          const res = await memoriesApi.setCover(memory.id, imageId)
          qc.setQueryData(detailKey, res)
          void qc.invalidateQueries({ queryKey: qk.memories.lists() })
        }}
      />

      <UnsavedChangesDialog blocker={blocker} onSaveAndLeave={flush} />
    </div>
  )
}
