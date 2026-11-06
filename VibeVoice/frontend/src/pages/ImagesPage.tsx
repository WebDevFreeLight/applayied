import { useInfiniteQuery, useQuery, useQueryClient } from '@tanstack/react-query'
import { BookHeart, ExternalLink, Images, Link2, NotebookPen, Search, Trash2 } from 'lucide-react'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router'
import { toast } from 'sonner'
import { errorMessage, fileUrl } from '@/api/client'
import { qk } from '@/api/keys'
import { imagesApi } from '@/api/library'
import { memoriesApi } from '@/api/memories'
import { searchApi } from '@/api/misc'
import { notesApi } from '@/api/notes'
import type { ImageOwnerType, ImageWithOwnerDto } from '@/api/types'
import { ImageUploader } from '@/components/images/ImageUploader'
import { Lightbox } from '@/components/images/Lightbox'
import { Button, IconButton } from '@/components/ui/Button'
import { Badge, Chip, EmptyState, ErrorState, PageHeader, Skeleton, Spinner } from '@/components/ui/Display'
import { Input } from '@/components/ui/Field'
import { ConfirmDialog, Modal } from '@/components/ui/Modal'
import { useDebouncedValue } from '@/hooks/misc'
import { formatBytes } from '@/lib/utils'

const OWNER_TYPES: ImageOwnerType[] = ['Library', 'Note', 'Memory']

export function ImagesPage() {
  const { t } = useTranslation()
  const qc = useQueryClient()
  const [ownerType, setOwnerType] = useState<ImageOwnerType | undefined>(undefined)
  const [openIndex, setOpenIndex] = useState<number | null>(null)
  const [attaching, setAttaching] = useState<ImageWithOwnerDto | null>(null)
  const [deleting, setDeleting] = useState<ImageWithOwnerDto | null>(null)
  const [busyDelete, setBusyDelete] = useState(false)

  const query = { pageSize: 48, ownerType }
  const list = useInfiniteQuery({
    queryKey: qk.images.list(query),
    queryFn: ({ pageParam, signal }) => imagesApi.list({ ...query, page: pageParam }, signal),
    initialPageParam: 1,
    getNextPageParam: (last) => (last.page * last.pageSize < last.total ? last.page + 1 : undefined),
  })
  const items = list.data?.pages.flatMap((p) => p.items) ?? []

  const refresh = () => {
    void qc.invalidateQueries({ queryKey: qk.images.all })
    void qc.invalidateQueries({ queryKey: qk.dashboard })
  }

  const confirmDelete = async () => {
    if (!deleting) return
    setBusyDelete(true)
    try {
      await imagesApi.remove(deleting.id)
      toast.success(t('images.deleted'))
      setDeleting(null)
      refresh()
      void qc.invalidateQueries({ queryKey: qk.notes.all })
      void qc.invalidateQueries({ queryKey: qk.memories.all })
    } catch (err) {
      toast.error(errorMessage(err))
    } finally {
      setBusyDelete(false)
    }
  }

  return (
    <div>
      <PageHeader icon={Images} title={t('images.title')} subtitle={t('images.subtitle')} />

      <ImageUploader
        className="mb-6"
        upload={(file, onProgress) => imagesApi.upload([file], onProgress)}
        onUploaded={refresh}
      />

      <div className="mb-5 flex flex-wrap gap-2" role="group" aria-label={t('images.ownerType')}>
        <Chip active={!ownerType} onClick={() => setOwnerType(undefined)}>
          {t('images.anyOwner')}
        </Chip>
        {OWNER_TYPES.map((o) => (
          <Chip key={o} active={ownerType === o} onClick={() => setOwnerType(o)}>
            {t(`images.ownerTypes.${o}`)}
          </Chip>
        ))}
      </div>

      {list.isLoading ? (
        <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-4">
          {Array.from({ length: 8 }, (_, i) => (
            <Skeleton key={i} className="aspect-square" />
          ))}
        </div>
      ) : list.error ? (
        <ErrorState error={list.error} onRetry={() => void list.refetch()} />
      ) : items.length === 0 ? (
        <EmptyState icon={Images} title={ownerType ? t('images.emptyFiltered') : t('images.empty')} body={t('images.emptyBody')} />
      ) : (
        <>
          <ul className="grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-4">
            {items.map((img, i) => (
              <li key={img.id} className="group flex flex-col overflow-hidden rounded-2xl border border-border bg-surface shadow-card">
                <button
                  type="button"
                  onClick={() => setOpenIndex(i)}
                  aria-label={t('images.openImage', { index: i + 1 })}
                  className="relative aspect-square overflow-hidden bg-surface-2"
                >
                  <img
                    src={fileUrl(img.thumbnailPath || img.filePath)}
                    alt={img.caption ?? ''}
                    loading="lazy"
                    className="size-full object-cover transition-transform duration-300 group-hover:scale-[1.03]"
                  />
                  <Badge className="absolute top-2 start-2 bg-black/55 text-white border-transparent backdrop-blur">
                    {img.ownerType === 'Library'
                      ? t('images.inLibrary')
                      : img.ownerType === 'Note'
                        ? t('audio.ownerTypes.Note')
                        : t('audio.ownerTypes.Memory')}
                  </Badge>
                </button>
                <div className="flex items-center gap-1 px-2 py-1.5">
                  <div className="min-w-0 flex-1">
                    {img.ownerType !== 'Library' ? (
                      <Link
                        to={img.ownerType === 'Note' ? `/notes/${img.ownerId}` : `/memories/${img.ownerId}`}
                        className="flex items-center gap-1 truncate text-xs font-medium text-accent-text hover:underline"
                      >
                        <ExternalLink className="size-3 shrink-0" aria-hidden />
                        <bdi className="truncate">{img.ownerTitle ?? t('common.untitled')}</bdi>
                      </Link>
                    ) : (
                      <span className="block truncate text-xs text-subtle" dir="auto">
                        {img.caption || `${img.width}×${img.height} · ${formatBytes(img.sizeBytes)}`}
                      </span>
                    )}
                  </div>
                  <IconButton label={t('images.attach')} size="icon-sm" onClick={() => setAttaching(img)}>
                    <Link2 className="size-4" aria-hidden />
                  </IconButton>
                  <IconButton label={t('common.delete')} size="icon-sm" className="hover:text-danger" onClick={() => setDeleting(img)}>
                    <Trash2 className="size-4" aria-hidden />
                  </IconButton>
                </div>
              </li>
            ))}
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

      {openIndex !== null && items[openIndex] && (
        <Lightbox images={items} index={openIndex} onIndexChange={setOpenIndex} onClose={() => setOpenIndex(null)} />
      )}

      <AttachDialog
        image={attaching}
        onClose={() => setAttaching(null)}
        onAttached={() => {
          refresh()
          void qc.invalidateQueries({ queryKey: qk.notes.all })
          void qc.invalidateQueries({ queryKey: qk.memories.all })
        }}
      />

      <ConfirmDialog
        open={!!deleting}
        title={t('images.deleteTitle')}
        body={t('images.deleteBody')}
        confirmLabel={t('common.delete')}
        loading={busyDelete}
        onConfirm={() => void confirmDelete()}
        onCancel={() => setDeleting(null)}
      />
    </div>
  )
}

/** Searchable picker that moves an image onto a note or memory. */
function AttachDialog({
  image,
  onClose,
  onAttached,
}: {
  image: ImageWithOwnerDto | null
  onClose: () => void
  onAttached: () => void
}) {
  const { t } = useTranslation()
  return (
    <Modal open={!!image} onClose={onClose} title={t('images.attachTitle')} size="md">
      {image && <AttachPicker image={image} onClose={onClose} onAttached={onAttached} />}
    </Modal>
  )
}

function AttachPicker({
  image,
  onClose,
  onAttached,
}: {
  image: ImageWithOwnerDto
  onClose: () => void
  onAttached: () => void
}) {
  const { t } = useTranslation()
  const [q, setQ] = useState('')
  const [busy, setBusy] = useState<string | null>(null)
  const debounced = useDebouncedValue(q.trim(), 250)
  const query = { q: debounced, types: ['notes', 'memories'] as ('notes' | 'memories')[], page: 1, pageSize: 15 }
  const results = useQuery({
    queryKey: qk.search.query(query),
    queryFn: ({ signal }) => searchApi.search(query, signal),
    enabled: debounced.length > 0,
  })
  // With an empty query, offer the most recently updated notes and memories.
  const recent = useQuery({
    queryKey: ['attach-recent'],
    queryFn: async () => {
      const [notes, memories] = await Promise.all([
        notesApi.list({ page: 1, pageSize: 6, sort: 'updated_desc' }),
        memoriesApi.list({ page: 1, pageSize: 6, sort: 'updated_desc' }),
      ])
      return [
        ...notes.items.map((n) => ({ kind: 'note' as const, id: n.id, title: n.title, snippet: n.excerpt })),
        ...memories.items.map((m) => ({ kind: 'memory' as const, id: m.id, title: m.title, snippet: m.excerpt })),
      ]
    },
    enabled: debounced.length === 0,
  })
  const items = debounced
    ? (results.data?.items ?? []).filter((r) => r.kind === 'note' || r.kind === 'memory')
    : (recent.data ?? [])

  const attach = async (kind: 'note' | 'memory', id: string, title: string) => {
    setBusy(id)
    try {
      await imagesApi.attach(image.id, kind === 'note' ? 'Note' : 'Memory', id)
      toast.success(t('images.attached', { title }))
      onAttached()
      onClose()
    } catch (err) {
      toast.error(errorMessage(err))
    } finally {
      setBusy(null)
    }
  }

  return (
    <div className="flex flex-col gap-3">
      <div className="relative">
        <Search className="pointer-events-none absolute start-3 top-1/2 size-4 -translate-y-1/2 text-subtle" aria-hidden />
        <Input
          autoFocus
          value={q}
          onChange={(e) => setQ(e.target.value)}
          placeholder={t('images.attachSearch')}
          aria-label={t('images.attachSearch')}
          className="ps-9"
        />
      </div>
      {(results.isFetching || recent.isFetching) && <Spinner className="self-center" />}
      {debounced && !results.isFetching && items.length === 0 && (
        <p className="py-4 text-center text-sm text-muted">{t('images.attachEmpty')}</p>
      )}
      <ul className="flex flex-col gap-1">
        {items.map((r) => {
          const Icon = r.kind === 'note' ? NotebookPen : BookHeart
          return (
            <li key={`${r.kind}-${r.id}`}>
              <button
                type="button"
                disabled={!!busy}
                onClick={() => void attach(r.kind as 'note' | 'memory', r.id, r.title)}
                className="flex w-full items-center gap-3 rounded-xl px-3 py-2.5 text-start hover:bg-surface-2 disabled:opacity-50"
              >
                <Icon className="size-4 shrink-0 text-accent-text" aria-hidden />
                <span className="min-w-0 flex-1">
                  <span className="block truncate text-sm font-medium text-fg" dir="auto">
                    {r.title || t('common.untitled')}
                  </span>
                  <span className="block truncate text-xs text-muted" dir="auto">
                    {r.snippet}
                  </span>
                </span>
                <span className="text-[11px] text-subtle">{t(`search.kinds.${r.kind}`)}</span>
                {busy === r.id && <Spinner />}
              </button>
            </li>
          )
        })}
      </ul>
    </div>
  )
}
