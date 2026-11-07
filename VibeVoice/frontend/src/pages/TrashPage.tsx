import { useQuery, useQueryClient } from '@tanstack/react-query'
import { BookHeart, NotebookPen, RotateCcw, Trash2 } from 'lucide-react'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { toast } from 'sonner'
import { qk } from '@/api/keys'
import { memoriesApi } from '@/api/memories'
import { notesApi } from '@/api/notes'
import { Button } from '@/components/ui/Button'
import { Card, EmptyState, ErrorState, PageHeader, Skeleton } from '@/components/ui/Display'
import { ConfirmDialog } from '@/components/ui/Modal'
import { useDates } from '@/lib/dates'
import { toastError } from '@/lib/notify'

interface TrashItem {
  kind: 'note' | 'memory'
  id: string
  title: string
  excerpt: string
  updatedAt: string
}

export function TrashPage() {
  const { t } = useTranslation()
  const dates = useDates()
  const qc = useQueryClient()
  const [pending, setPending] = useState<TrashItem | null>(null)
  const [busy, setBusy] = useState<string | null>(null)

  const notes = useQuery({
    queryKey: qk.notes.list({ deleted: true, pageSize: 100 }),
    queryFn: ({ signal }) => notesApi.list({ deleted: true, pageSize: 100, page: 1 }, signal),
  })
  const memories = useQuery({
    queryKey: qk.memories.list({ deleted: true, pageSize: 100 }),
    queryFn: ({ signal }) => memoriesApi.list({ deleted: true, pageSize: 100, page: 1 }, signal),
  })

  const refresh = () => {
    void qc.invalidateQueries({ queryKey: qk.notes.all })
    void qc.invalidateQueries({ queryKey: qk.memories.all })
    void qc.invalidateQueries({ queryKey: qk.dashboard })
  }

  const restore = async (item: TrashItem) => {
    setBusy(item.id)
    try {
      if (item.kind === 'note') await notesApi.restore(item.id)
      else await memoriesApi.restore(item.id)
      toast.success(t('trash.restored'))
      refresh()
    } catch (err) {
      toastError(err)
    } finally {
      setBusy(null)
    }
  }

  const removeForever = async () => {
    if (!pending) return
    setBusy(pending.id)
    try {
      if (pending.kind === 'note') await notesApi.removePermanent(pending.id)
      else await memoriesApi.removePermanent(pending.id)
      toast.success(t('trash.deletedForever'))
      setPending(null)
      refresh()
      void qc.invalidateQueries({ queryKey: qk.audio.all })
      void qc.invalidateQueries({ queryKey: qk.images.all })
    } catch (err) {
      toastError(err)
    } finally {
      setBusy(null)
    }
  }

  const sections: { kind: 'note' | 'memory'; title: string; icon: typeof NotebookPen; items: TrashItem[] }[] = [
    {
      kind: 'note',
      title: t('nav.notes'),
      icon: NotebookPen,
      items: (notes.data?.items ?? []).map((n) => ({ kind: 'note', id: n.id, title: n.title, excerpt: n.excerpt, updatedAt: n.updatedAt })),
    },
    {
      kind: 'memory',
      title: t('nav.memories'),
      icon: BookHeart,
      items: (memories.data?.items ?? []).map((m) => ({ kind: 'memory', id: m.id, title: m.title, excerpt: m.excerpt, updatedAt: m.updatedAt })),
    },
  ]
  const loading = notes.isLoading || memories.isLoading
  const error = notes.error ?? memories.error
  const empty = !loading && sections.every((s) => !s.items.length)

  return (
    <div>
      <PageHeader art="trash" title={t('trash.title')} subtitle={t('trash.subtitle')} />
      {loading ? (
        <div className="flex flex-col gap-3">
          {Array.from({ length: 4 }, (_, i) => (
            <Skeleton key={i} className="h-16" />
          ))}
        </div>
      ) : error ? (
        <ErrorState
          error={error}
          onRetry={() => {
            void notes.refetch()
            void memories.refetch()
          }}
        />
      ) : empty ? (
        <EmptyState art="trash" title={t('trash.empty')} body={t('trash.emptyBody')} />
      ) : (
        <div className="flex flex-col gap-8">
          {sections
            .filter((s) => s.items.length)
            .map((s) => (
              <section key={s.kind} aria-labelledby={`trash-${s.kind}`}>
                <h2 id={`trash-${s.kind}`} className="mb-3 flex items-center gap-2 text-sm font-semibold text-fg">
                  <s.icon className="size-4 text-accent-text" aria-hidden />
                  {s.title} <span className="font-normal text-subtle">({s.items.length})</span>
                </h2>
                <ul className="flex flex-col gap-2">
                  {s.items.map((item) => (
                    <li key={item.id}>
                      <Card className="flex flex-col gap-3 p-4 sm:flex-row sm:items-center">
                        <div className="min-w-0 flex-1">
                          <p className="truncate font-medium text-fg" dir="auto">
                            {item.title || t('common.untitled')}
                          </p>
                          {item.excerpt && (
                            <p className="line-clamp-1 text-sm text-muted" dir="auto">
                              {item.excerpt}
                            </p>
                          )}
                          <p className="mt-0.5 text-xs text-subtle">{t('trash.deletedAt', { time: dates.relative(item.updatedAt) })}</p>
                        </div>
                        <div className="flex shrink-0 gap-2">
                          <Button
                            size="sm"
                            loading={busy === item.id && !pending}
                            disabled={!!busy}
                            onClick={() => void restore(item)}
                            icon={<RotateCcw className="size-4" aria-hidden />}
                          >
                            {t('common.restore')}
                          </Button>
                          <Button
                            size="sm"
                            variant="ghost"
                            className="text-danger hover:bg-danger-soft hover:text-danger"
                            disabled={!!busy}
                            onClick={() => setPending(item)}
                            icon={<Trash2 className="size-4" aria-hidden />}
                          >
                            {t('common.deleteForever')}
                          </Button>
                        </div>
                      </Card>
                    </li>
                  ))}
                </ul>
              </section>
            ))}
        </div>
      )}

      <ConfirmDialog
        open={!!pending}
        title={t('trash.confirmTitle')}
        body={t('trash.confirmBody', { title: pending?.title || t('common.untitled') })}
        confirmLabel={t('common.deleteForever')}
        loading={!!pending && busy === pending.id}
        onConfirm={() => void removeForever()}
        onCancel={() => setPending(null)}
      />
    </div>
  )
}
