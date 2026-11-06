import { useInfiniteQuery } from '@tanstack/react-query'
import { BookHeart, Plus, Search, X } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { useSearchParams } from 'react-router'
import { useCreateMemory, usePeople, useTags } from '@/api/hooks'
import { qk } from '@/api/keys'
import { memoriesApi } from '@/api/memories'
import { MOODS, type MemoryListItemDto, type MemoryListQuery, type MemorySort, type Mood } from '@/api/types'
import { CardSkeletonGrid, MemoryCard } from '@/components/Cards'
import { Button } from '@/components/ui/Button'
import { EmptyState, ErrorState, PageHeader } from '@/components/ui/Display'
import { Input, Select } from '@/components/ui/Field'
import { useDates } from '@/lib/dates'
import { useDebouncedValue } from '@/hooks/misc'
import { MOOD_EMOJI } from '@/lib/utils'

const SORTS: MemorySort[] = ['date_desc', 'date_asc', 'updated_desc', 'title_asc']
const PAGE_SIZE = 24

function groupByMonth(items: MemoryListItemDto[]): { key: string; items: MemoryListItemDto[] }[] {
  const groups: { key: string; items: MemoryListItemDto[] }[] = []
  for (const m of items) {
    const key = m.memoryDate.slice(0, 7) // yyyy-MM
    const last = groups[groups.length - 1]
    if (last && last.key === key) last.items.push(m)
    else groups.push({ key, items: [m] })
  }
  return groups
}

export function MemoriesPage() {
  const { t } = useTranslation()
  const dates = useDates()
  const [params, setParams] = useSearchParams()
  const createMemory = useCreateMemory()
  const tags = useTags()
  const people = usePeople()

  const search = params.get('q') ?? ''
  const tag = params.get('tag') ?? ''
  const person = params.get('person') ?? ''
  const moodParam = params.get('mood') ?? ''
  const mood = (MOODS as readonly string[]).includes(moodParam) ? (moodParam as Mood) : undefined
  const from = params.get('from') ?? ''
  const to = params.get('to') ?? ''
  const sort = (SORTS.includes(params.get('sort') as MemorySort) ? params.get('sort') : 'date_desc') as MemorySort
  const debouncedSearch = useDebouncedValue(search.trim(), 300)

  const set = (key: string, value: string | null) => {
    const next = new URLSearchParams(params)
    if (value) next.set(key, value)
    else next.delete(key)
    setParams(next, { replace: true })
  }

  const query: MemoryListQuery = {
    pageSize: PAGE_SIZE,
    search: debouncedSearch || undefined,
    tag: tag || undefined,
    person: person || undefined,
    mood,
    from: from || undefined,
    to: to || undefined,
    sort,
  }
  const list = useInfiniteQuery({
    queryKey: qk.memories.list(query),
    queryFn: ({ pageParam, signal }) => memoriesApi.list({ ...query, page: pageParam }, signal),
    initialPageParam: 1,
    getNextPageParam: (last) => (last.page * last.pageSize < last.total ? last.page + 1 : undefined),
  })
  const items = list.data?.pages.flatMap((p) => p.items) ?? []
  const groups = sort === 'date_desc' || sort === 'date_asc' ? groupByMonth(items) : [{ key: '', items }]
  const filtered = !!(debouncedSearch || tag || person || mood || from || to)

  return (
    <div>
      <PageHeader
        icon={BookHeart}
        title={t('memories.title')}
        subtitle={t('memories.subtitle')}
        actions={
          <Button
            variant="primary"
            onClick={() => createMemory.mutate()}
            loading={createMemory.isPending}
            icon={<Plus className="size-4" aria-hidden />}
          >
            {t('memories.new')}
          </Button>
        }
      />

      <div className="mb-8 grid gap-3 sm:grid-cols-2 lg:grid-cols-4" role="group" aria-label={t('common.filters')}>
        <div className="relative sm:col-span-2">
          <Search className="pointer-events-none absolute start-3 top-1/2 size-4 -translate-y-1/2 text-subtle" aria-hidden />
          <Input
            value={search}
            onChange={(e) => set('q', e.target.value)}
            placeholder={t('search.placeholder')}
            aria-label={t('search.label')}
            className="ps-9"
          />
        </div>
        <Select value={tag} onChange={(e) => set('tag', e.target.value)} aria-label={t('notes.filterTag')}>
          <option value="">{t('notes.anyTag')}</option>
          {(tags.data ?? []).map((tg) => (
            <option key={tg.name} value={tg.name}>
              #{tg.name} ({tg.count})
            </option>
          ))}
        </Select>
        <Select value={person} onChange={(e) => set('person', e.target.value)} aria-label={t('memories.person')}>
          <option value="">{t('memories.anyPerson')}</option>
          {(people.data ?? []).map((p) => (
            <option key={p.name} value={p.name}>
              {p.name} ({p.count})
            </option>
          ))}
        </Select>
        <Select value={mood ?? ''} onChange={(e) => set('mood', e.target.value)} aria-label={t('memories.mood')}>
          <option value="">{t('memories.anyMood')}</option>
          {MOODS.map((m) => (
            <option key={m} value={m}>
              {MOOD_EMOJI[m]} {t(`memories.moods.${m}`)}
            </option>
          ))}
        </Select>
        <label className="flex items-center gap-2 text-sm text-muted">
          <span className="shrink-0">{t('common.from')}</span>
          <Input type="date" value={from} max={to || undefined} onChange={(e) => set('from', e.target.value)} />
        </label>
        <label className="flex items-center gap-2 text-sm text-muted">
          <span className="shrink-0">{t('common.to')}</span>
          <Input type="date" value={to} min={from || undefined} onChange={(e) => set('to', e.target.value)} />
        </label>
        <div className="flex gap-2">
          <Select value={sort} onChange={(e) => set('sort', e.target.value)} aria-label={t('common.sort')}>
            {SORTS.map((s) => (
              <option key={s} value={s}>
                {t(`memories.sort.${s}`)}
              </option>
            ))}
          </Select>
          {filtered && (
            <Button variant="ghost" size="icon" aria-label={t('common.clearFilters')} title={t('common.clearFilters')} onClick={() => setParams({}, { replace: true })}>
              <X className="size-4" aria-hidden />
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
          icon={BookHeart}
          title={filtered ? t('memories.emptyFiltered') : t('memories.empty')}
          body={filtered ? undefined : t('memories.emptyBody')}
          action={
            filtered ? (
              <Button onClick={() => setParams({}, { replace: true })}>{t('common.clearFilters')}</Button>
            ) : (
              <Button variant="primary" onClick={() => createMemory.mutate()} loading={createMemory.isPending} icon={<Plus className="size-4" aria-hidden />}>
                {t('memories.new')}
              </Button>
            )
          }
        />
      ) : (
        <div className="relative flex flex-col gap-10">
          {groups.map((g) => (
            <section key={g.key || 'all'} aria-label={g.key ? dates.format(`${g.key}-01`, 'LLLL yyyy') : undefined} className="relative">
              {g.key && (
                <div className="sticky top-16 z-10 -mx-1 mb-4 flex items-center gap-3 bg-bg/85 px-1 py-2 backdrop-blur">
                  <span className="size-2.5 rounded-full bg-accent ring-4 ring-accent-soft" aria-hidden />
                  <h2 className="text-sm font-semibold text-fg">{dates.format(`${g.key}-01`, 'LLLL yyyy')}</h2>
                  <span className="text-xs text-subtle">{t('common.items', { count: g.items.length })}</span>
                  <span className="h-px flex-1 bg-border" aria-hidden />
                </div>
              )}
              <ul className="grid gap-4 border-border sm:grid-cols-2 lg:grid-cols-3 sm:border-s sm:ps-6">
                {g.items.map((m) => (
                  <li key={m.id}>
                    <MemoryCard memory={m} />
                  </li>
                ))}
              </ul>
            </section>
          ))}
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
