import { useInfiniteQuery } from '@tanstack/react-query'
import { Archive, AudioLines, Image as ImageIcon, LayoutGrid, List, NotebookPen, Plus, Search, X } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { useSearchParams } from 'react-router'
import { useCreateNote, useTags } from '@/api/hooks'
import { qk } from '@/api/keys'
import { notesApi } from '@/api/notes'
import type { NoteListQuery, NoteSort } from '@/api/types'
import { CardSkeletonGrid, NoteCard } from '@/components/Cards'
import { Button } from '@/components/ui/Button'
import { Chip, EmptyState, ErrorState, PageHeader, Segmented } from '@/components/ui/Display'
import { Input, Select } from '@/components/ui/Field'
import { useDebouncedValue, useLocalStorageState } from '@/hooks/misc'
import { cn } from '@/lib/utils'

const SORTS: NoteSort[] = ['updated_desc', 'updated_asc', 'created_desc', 'created_asc', 'title_asc']
const PAGE_SIZE = 24

export function NotesPage() {
  const { t } = useTranslation()
  const [params, setParams] = useSearchParams()
  const [view, setView] = useLocalStorageState<'grid' | 'list'>('vv.notesView', 'grid')
  const createNote = useCreateNote()
  const tags = useTags()

  const search = params.get('q') ?? ''
  const tag = params.get('tag') ?? ''
  const hasAudio = params.get('audio') === '1'
  const hasImages = params.get('images') === '1'
  const archived = params.get('archived') === '1'
  const sort = (SORTS.includes(params.get('sort') as NoteSort) ? params.get('sort') : 'updated_desc') as NoteSort
  const debouncedSearch = useDebouncedValue(search.trim(), 300)

  const set = (key: string, value: string | null) => {
    const next = new URLSearchParams(params)
    if (value) next.set(key, value)
    else next.delete(key)
    setParams(next, { replace: true })
  }

  const query: NoteListQuery = {
    pageSize: PAGE_SIZE,
    search: debouncedSearch || undefined,
    tag: tag || undefined,
    hasAudio: hasAudio || undefined,
    hasImages: hasImages || undefined,
    archivedOnly: archived || undefined,
    sort,
  }

  const list = useInfiniteQuery({
    queryKey: qk.notes.list(query),
    queryFn: ({ pageParam, signal }) => notesApi.list({ ...query, page: pageParam }, signal),
    initialPageParam: 1,
    getNextPageParam: (last) => (last.page * last.pageSize < last.total ? last.page + 1 : undefined),
  })

  const items = list.data?.pages.flatMap((p) => p.items) ?? []
  const pinned = items.filter((n) => n.isPinned)
  const others = items.filter((n) => !n.isPinned)
  const filtered = !!(debouncedSearch || tag || hasAudio || hasImages || archived)

  const renderList = (arr: typeof items) => (
    <ul className={cn(view === 'grid' ? 'grid gap-4 sm:grid-cols-2 lg:grid-cols-3' : 'flex flex-col gap-3')}>
      {arr.map((n) => (
        <li key={n.id}>
          <NoteCard note={n} layout={view} />
        </li>
      ))}
    </ul>
  )

  return (
    <div>
      <PageHeader
        icon={NotebookPen}
        title={t('notes.title')}
        subtitle={t('notes.subtitle')}
        actions={
          <>
            <Segmented
              label={t('common.viewMode')}
              value={view}
              onChange={setView}
              options={[
                { value: 'grid', label: '', icon: LayoutGrid, title: t('common.gridView') },
                { value: 'list', label: '', icon: List, title: t('common.listView') },
              ]}
            />
            <Button
              variant="primary"
              onClick={() => createNote.mutate(undefined)}
              loading={createNote.isPending}
              icon={<Plus className="size-4" aria-hidden />}
            >
              {t('notes.new')}
            </Button>
          </>
        }
      />

      <div className="mb-6 flex flex-col gap-3">
        <div className="flex flex-col gap-3 sm:flex-row">
          <div className="relative flex-1">
            <Search className="pointer-events-none absolute start-3 top-1/2 size-4 -translate-y-1/2 text-subtle" aria-hidden />
            <Input
              value={search}
              onChange={(e) => set('q', e.target.value)}
              placeholder={t('notes.searchPlaceholder')}
              aria-label={t('notes.searchPlaceholder')}
              className="ps-9"
            />
          </div>
          <Select value={tag} onChange={(e) => set('tag', e.target.value)} aria-label={t('notes.filterTag')} className="sm:w-48">
            <option value="">{t('notes.anyTag')}</option>
            {(tags.data ?? []).map((tg) => (
              <option key={tg.name} value={tg.name}>
                #{tg.name} ({tg.count})
              </option>
            ))}
          </Select>
          <Select value={sort} onChange={(e) => set('sort', e.target.value)} aria-label={t('common.sort')} className="sm:w-52">
            {SORTS.map((s) => (
              <option key={s} value={s}>
                {t(`notes.sort.${s}`)}
              </option>
            ))}
          </Select>
        </div>
        <div className="flex flex-wrap items-center gap-2" role="group" aria-label={t('common.filters')}>
          <Chip active={hasAudio} onClick={() => set('audio', hasAudio ? null : '1')} icon={AudioLines}>
            {t('notes.hasAudio')}
          </Chip>
          <Chip active={hasImages} onClick={() => set('images', hasImages ? null : '1')} icon={ImageIcon}>
            {t('notes.hasImages')}
          </Chip>
          <Chip active={archived} onClick={() => set('archived', archived ? null : '1')} icon={Archive}>
            {t('notes.showArchived')}
          </Chip>
          {filtered && (
            <Button size="sm" variant="ghost" onClick={() => setParams({}, { replace: true })} icon={<X className="size-3.5" aria-hidden />}>
              {t('common.clearFilters')}
            </Button>
          )}
        </div>
      </div>

      {list.isLoading ? (
        <CardSkeletonGrid />
      ) : list.error ? (
        <ErrorState error={list.error} onRetry={() => void list.refetch()} />
      ) : items.length === 0 ? (
        <EmptyState
          icon={NotebookPen}
          title={filtered ? t('notes.emptyFiltered') : t('notes.empty')}
          body={filtered ? undefined : t('notes.emptyBody')}
          action={
            filtered ? (
              <Button onClick={() => setParams({}, { replace: true })}>{t('common.clearFilters')}</Button>
            ) : (
              <Button variant="primary" onClick={() => createNote.mutate(undefined)} loading={createNote.isPending} icon={<Plus className="size-4" aria-hidden />}>
                {t('notes.new')}
              </Button>
            )
          }
        />
      ) : (
        <div className="flex flex-col gap-8">
          {pinned.length > 0 && (
            <section aria-label={t('notes.pinnedSection')}>
              <h2 className="mb-3 text-xs font-semibold tracking-wider text-subtle uppercase">{t('notes.pinnedSection')}</h2>
              {renderList(pinned)}
            </section>
          )}
          {others.length > 0 && (
            <section aria-label={t('notes.othersSection')}>
              {pinned.length > 0 && (
                <h2 className="mb-3 text-xs font-semibold tracking-wider text-subtle uppercase">{t('notes.othersSection')}</h2>
              )}
              {renderList(others)}
            </section>
          )}
          {list.hasNextPage && (
            <div className="flex justify-center">
              <Button onClick={() => void list.fetchNextPage()} loading={list.isFetchingNextPage}>
                {t('common.loadMore')}
              </Button>
            </div>
          )}
        </div>
      )}
    </div>
  )
}
