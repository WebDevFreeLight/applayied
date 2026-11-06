import { useInfiniteQuery, useQueryClient } from '@tanstack/react-query'
import { AudioLines, Mic, Search } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { useSearchParams } from 'react-router'
import { attachmentsApi, ownerKindOf } from '@/api/attachments'
import { useCreateNote } from '@/api/hooks'
import { qk } from '@/api/keys'
import { audioApi } from '@/api/library'
import type { AudioListQuery, AudioOwnerType, AudioSource, AudioWithOwnerDto } from '@/api/types'
import { voiceApi } from '@/api/voice'
import { AudioPlayer } from '@/components/audio/AudioPlayer'
import { Button } from '@/components/ui/Button'
import { Chip, EmptyState, ErrorState, PageHeader, Skeleton } from '@/components/ui/Display'
import { Input, Select } from '@/components/ui/Field'
import { useDebouncedValue } from '@/hooks/misc'
import { useDates } from '@/lib/dates'
import { audioOwnerLink } from '@/lib/links'

const SOURCES: AudioSource[] = ['Recorded', 'Uploaded', 'Tts']
const OWNERS: AudioOwnerType[] = ['Note', 'Memory', 'Studio']

export function AudioPage() {
  const { t } = useTranslation()
  const dates = useDates()
  const qc = useQueryClient()
  const createNote = useCreateNote()
  const [params, setParams] = useSearchParams()
  const sourceParam = params.get('source') ?? ''
  const source = (SOURCES as string[]).includes(sourceParam) ? (sourceParam as AudioSource) : undefined
  const ownerParam = params.get('owner') ?? ''
  const ownerType = (OWNERS as string[]).includes(ownerParam) ? (ownerParam as AudioOwnerType) : undefined
  const search = params.get('q') ?? ''
  const debounced = useDebouncedValue(search.trim(), 300)

  const set = (key: string, value: string | null) => {
    const next = new URLSearchParams(params)
    if (value) next.set(key, value)
    else next.delete(key)
    setParams(next, { replace: true })
  }

  const query: AudioListQuery = { pageSize: 20, source, ownerType, search: debounced || undefined }
  const list = useInfiniteQuery({
    queryKey: qk.audio.list(query),
    queryFn: ({ pageParam, signal }) => audioApi.list({ ...query, page: pageParam }, signal),
    initialPageParam: 1,
    getNextPageParam: (last) => (last.page * last.pageSize < last.total ? last.page + 1 : undefined),
  })
  const items = list.data?.pages.flatMap((p) => p.items) ?? []
  const filtered = !!(source || ownerType || debounced)

  const refresh = () => {
    void qc.invalidateQueries({ queryKey: qk.audio.all })
    void qc.invalidateQueries({ queryKey: qk.dashboard })
  }

  const newAudioNote = () =>
    createNote.mutate({
      body: { title: t('audio.audioNoteTitle', { date: dates.format(new Date(), 'PPp') }) },
      search: '?record=1',
    })

  const remove = async (a: AudioWithOwnerDto) => {
    const kind = ownerKindOf(a.ownerType)
    if (kind) await attachmentsApi.deleteAudio(kind, a.ownerId, a.id)
    else await voiceApi.deleteHistory(a.id)
    refresh()
    void qc.invalidateQueries({ queryKey: qk.voice.histories() })
  }

  return (
    <div>
      <PageHeader
        icon={AudioLines}
        title={t('audio.title')}
        subtitle={t('audio.subtitle')}
        actions={
          <Button variant="primary" onClick={newAudioNote} loading={createNote.isPending} icon={<Mic className="size-4" aria-hidden />}>
            {t('audio.newAudioNote')}
          </Button>
        }
      />

      <div className="mb-6 flex flex-col gap-3">
        <div className="flex flex-col gap-3 sm:flex-row">
          <div className="relative flex-1">
            <Search className="pointer-events-none absolute start-3 top-1/2 size-4 -translate-y-1/2 text-subtle" aria-hidden />
            <Input
              value={search}
              onChange={(e) => set('q', e.target.value)}
              placeholder={t('audio.searchPlaceholder')}
              aria-label={t('audio.searchPlaceholder')}
              className="ps-9"
            />
          </div>
          <Select value={ownerType ?? ''} onChange={(e) => set('owner', e.target.value)} aria-label={t('audio.owner')} className="sm:w-44">
            <option value="">{t('audio.anyOwner')}</option>
            {OWNERS.map((o) => (
              <option key={o} value={o}>
                {t(`audio.ownerTypes.${o}`)}
              </option>
            ))}
          </Select>
        </div>
        <div className="flex flex-wrap gap-2" role="group" aria-label={t('audio.source')}>
          <Chip active={!source} onClick={() => set('source', null)}>
            {t('audio.anySource')}
          </Chip>
          {SOURCES.map((s) => (
            <Chip key={s} active={source === s} onClick={() => set('source', s)}>
              {t(`audio.sources.${s}`)}
            </Chip>
          ))}
        </div>
      </div>

      {list.isLoading ? (
        <div className="flex flex-col gap-3">
          {Array.from({ length: 4 }, (_, i) => (
            <Skeleton key={i} className="h-28" />
          ))}
        </div>
      ) : list.error ? (
        <ErrorState error={list.error} onRetry={() => void list.refetch()} />
      ) : items.length === 0 ? (
        <EmptyState
          icon={AudioLines}
          title={filtered ? t('audio.emptyFiltered') : t('audio.empty')}
          body={filtered ? undefined : t('audio.emptyBody')}
          action={
            filtered ? (
              <Button onClick={() => setParams({}, { replace: true })}>{t('common.clearFilters')}</Button>
            ) : (
              <Button variant="primary" onClick={newAudioNote} icon={<Mic className="size-4" aria-hidden />}>
                {t('audio.newAudioNote')}
              </Button>
            )
          }
        />
      ) : (
        <>
          <ul className="flex flex-col gap-3" aria-label={t('audio.listLabel')}>
            {items.map((a) => {
              const kind = ownerKindOf(a.ownerType)
              return (
                <li key={a.id}>
                  <AudioPlayer
                    audio={a}
                    owner={{
                      to: audioOwnerLink(a.ownerType, a.ownerId),
                      title: a.ownerTitle ?? t(`audio.ownerTypes.${a.ownerType}`),
                      kindLabel: t(`audio.ownerTypes.${a.ownerType}`),
                    }}
                    canAppend={!!kind}
                    onTranscribe={
                      kind
                        ? async (append) => {
                            const updated = await attachmentsApi.transcribe(kind, a.ownerId, a.id, { appendToBody: append })
                            refresh()
                            if (append) void qc.invalidateQueries({ queryKey: [kind] })
                            return updated
                          }
                        : undefined
                    }
                    onDelete={() => remove(a)}
                  />
                </li>
              )
            })}
          </ul>
          {list.hasNextPage && (
            <div className="mt-6 flex justify-center">
              <Button onClick={() => void list.fetchNextPage()} loading={list.isFetchingNextPage}>
                {t('common.loadMore')}
              </Button>
            </div>
          )}
        </>
      )}
    </div>
  )
}
