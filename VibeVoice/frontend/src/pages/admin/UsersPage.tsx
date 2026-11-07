import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { KeyRound, Pencil, Search, Trash2, UserPlus, X } from 'lucide-react'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useSearchParams } from 'react-router'
import { toast } from 'sonner'
import { usersApi } from '@/api/auth'
import { useRoles } from '@/api/hooks'
import { qk } from '@/api/keys'
import type { AdminUserDto, RoleDto, UserDto, UserListQuery } from '@/api/types'
import { useAuth } from '@/auth/context'
import { DeleteUserDialog, ResetPasswordDialog, UserFormDialog } from '@/components/admin/UserDialogs'
import { Button, IconButton } from '@/components/ui/Button'
import { Badge, Card, EmptyState, ErrorState, PageHeader, Skeleton } from '@/components/ui/Display'
import { Input, Select } from '@/components/ui/Field'
import { useDebouncedValue } from '@/hooks/misc'
import { roleLabel } from '@/lib/admin'
import { useDates } from '@/lib/dates'
import { toastError } from '@/lib/notify'
import { cn, formatBytes } from '@/lib/utils'

const PAGE_SIZE = 20

function Avatar({ user, size = 'md' }: { user: UserDto; size?: 'md' | 'lg' }) {
  return (
    <span
      aria-hidden
      className={cn(
        'flex shrink-0 items-center justify-center rounded-full bg-brand font-semibold text-white uppercase shadow-sm',
        size === 'lg' ? 'size-11 text-base' : 'size-9 text-sm',
      )}
    >
      {(user.displayName || user.email).trim().charAt(0)}
    </span>
  )
}

export function UsersPage() {
  const { t } = useTranslation()
  const dates = useDates()
  const qc = useQueryClient()
  const { user: me, can, reload } = useAuth()
  const canManage = can('users.manage')
  const [params, setParams] = useSearchParams()
  const roles = useRoles()
  const [creating, setCreating] = useState(false)
  const [editing, setEditing] = useState<UserDto | null>(null)
  const [resetting, setResetting] = useState<UserDto | null>(null)
  const [deleting, setDeleting] = useState<UserDto | null>(null)

  const search = params.get('q') ?? ''
  const roleId = params.get('roleId') ?? ''
  const activeParam = params.get('active')
  const active = activeParam === 'true' ? true : activeParam === 'false' ? false : undefined
  const page = Math.max(1, Number(params.get('page')) || 1)
  const debounced = useDebouncedValue(search.trim(), 300)

  const set = (key: string, value: string | null, resetPage = true) => {
    const next = new URLSearchParams(params)
    if (value) next.set(key, value)
    else next.delete(key)
    if (resetPage && key !== 'page') next.delete('page')
    setParams(next, { replace: true })
  }

  const query: UserListQuery = { page, pageSize: PAGE_SIZE, search: debounced || undefined, roleId: roleId || undefined, active }
  const list = useQuery({
    queryKey: qk.users.list(query),
    queryFn: ({ signal }) => usersApi.list(query, signal),
    placeholderData: keepPreviousData,
  })
  const items = list.data?.items ?? []
  const total = list.data?.total ?? 0
  const pages = Math.max(1, Math.ceil(total / PAGE_SIZE))
  const filtered = !!(debounced || roleId || active !== undefined)

  const afterChange = (u: UserDto) => {
    void qc.invalidateQueries({ queryKey: qk.users.all })
    void qc.invalidateQueries({ queryKey: qk.roles })
    void qc.invalidateQueries({ queryKey: qk.adminOverview })
    if (u.id === me?.id) reload()
  }
  const setRole = useMutation({
    mutationFn: ({ id, roleId: r }: { id: string; roleId: string }) => usersApi.setRole(id, r),
    onSuccess: (u) => {
      toast.success(t('users.updated'))
      afterChange(u)
    },
    onError: (err) => toastError(err),
  })
  const setActive = useMutation({
    mutationFn: ({ id, isActive }: { id: string; isActive: boolean }) => usersApi.setActive(id, isActive),
    onSuccess: (u) => {
      toast.success(u.isActive ? t('users.activated') : t('users.deactivated'))
      afterChange(u)
    },
    onError: (err) => toastError(err),
  })

  const roleList: RoleDto[] = roles.data ?? []

  const renderRole = (u: UserDto, className?: string) => {
    const self = u.id === me?.id
    if (!canManage || !roleList.length) return <Badge tone={u.role === 'Admin' ? 'accent' : 'neutral'}>{roleLabel(t, u.role)}</Badge>
    return (
      <span title={self ? t('users.selfRole') : undefined} className="inline-flex">
        <Select
          value={u.roleId}
          disabled={self || setRole.isPending}
          aria-label={`${t('users.role')} — ${u.displayName}`}
          onChange={(e) => setRole.mutate({ id: u.id, roleId: e.target.value })}
          className={cn('h-9 w-36', className)}
        >
          {roleList.map((r) => (
            <option key={r.id} value={r.id}>
              {roleLabel(t, r.name)}
            </option>
          ))}
        </Select>
      </span>
    )
  }

  const renderActions = (u: UserDto) => {
    if (!canManage) return null
    const self = u.id === me?.id
    return (
      <div className="flex flex-wrap items-center gap-1">
        <span title={self ? t('users.selfDeactivate') : undefined} className="inline-flex">
          <Button
            size="sm"
            variant={u.isActive ? 'outline' : 'secondary'}
            disabled={self || setActive.isPending}
            onClick={() => setActive.mutate({ id: u.id, isActive: !u.isActive })}
            aria-label={`${u.isActive ? t('users.deactivate') : t('users.activate')} — ${u.displayName}`}
          >
            {u.isActive ? t('users.deactivate') : t('users.activate')}
          </Button>
        </span>
        <IconButton label={`${t('users.edit')} — ${u.displayName}`} size="icon-sm" onClick={() => setEditing(u)}>
          <Pencil className="size-4" aria-hidden />
        </IconButton>
        <IconButton label={`${t('users.resetPassword')} — ${u.displayName}`} size="icon-sm" onClick={() => setResetting(u)}>
          <KeyRound className="size-4" aria-hidden />
        </IconButton>
        <span title={self ? t('users.selfDelete') : undefined} className="inline-flex">
          <IconButton
            label={`${t('users.delete')} — ${u.displayName}`}
            size="icon-sm"
            className="hover:text-danger"
            disabled={self}
            onClick={() => setDeleting(u)}
          >
            <Trash2 className="size-4" aria-hidden />
          </IconButton>
        </span>
      </div>
    )
  }

  const status = (u: UserDto) => (
    <Badge tone={u.isActive ? 'success' : 'danger'}>{u.isActive ? t('users.active') : t('users.inactive')}</Badge>
  )
  const counts = (a: AdminUserDto) => t('users.counts', { notes: a.notes, memories: a.memories, media: a.mediaFiles })
  const lastLogin = (u: UserDto) => (u.lastLoginAt ? dates.relative(u.lastLoginAt) : t('users.never'))

  return (
    <div>
      <PageHeader
        art="users"
        title={t('users.title')}
        subtitle={t('users.subtitle')}
        actions={
          canManage && (
            <Button variant="primary" onClick={() => setCreating(true)} icon={<UserPlus className="size-4" aria-hidden />}>
              {t('users.create')}
            </Button>
          )
        }
      />

      <div className="mb-5 grid gap-3 sm:grid-cols-[1fr_auto_auto]" role="group" aria-label={t('common.filters')}>
        <div className="relative">
          <Search className="pointer-events-none absolute start-3 top-1/2 size-4 -translate-y-1/2 text-subtle" aria-hidden />
          <Input
            type="search"
            value={search}
            onChange={(e) => set('q', e.target.value)}
            placeholder={t('users.searchPlaceholder')}
            aria-label={t('users.searchPlaceholder')}
            className="ps-9"
          />
        </div>
        <Select value={roleId} onChange={(e) => set('roleId', e.target.value)} aria-label={t('users.role')} className="sm:w-44">
          <option value="">{t('users.anyRole')}</option>
          {roleList.map((r) => (
            <option key={r.id} value={r.id}>
              {roleLabel(t, r.name)}
            </option>
          ))}
        </Select>
        <div className="flex gap-2">
          <Select
            value={activeParam ?? ''}
            onChange={(e) => set('active', e.target.value)}
            aria-label={t('users.status')}
            className="sm:w-40"
          >
            <option value="">{t('users.anyStatus')}</option>
            <option value="true">{t('users.active')}</option>
            <option value="false">{t('users.inactive')}</option>
          </Select>
          {filtered && (
            <IconButton label={t('common.clearFilters')} onClick={() => setParams({}, { replace: true })}>
              <X className="size-4" aria-hidden />
            </IconButton>
          )}
        </div>
      </div>

      {list.isLoading ? (
        <div className="flex flex-col gap-2">
          {Array.from({ length: 5 }, (_, i) => (
            <Skeleton key={i} className="h-16" />
          ))}
        </div>
      ) : list.error ? (
        <ErrorState error={list.error} onRetry={() => void list.refetch()} />
      ) : items.length === 0 ? (
        <EmptyState art="users" title={filtered ? t('users.emptyFiltered') : t('users.empty')} />
      ) : (
        <>
          <p className="mb-2 text-sm text-muted" role="status">
            {t('users.total', { count: total })}
          </p>

          {/* Phones / small tablets: one card per user */}
          <ul className="flex flex-col gap-3 md:hidden" aria-label={t('users.title')}>
            {items.map((a) => {
              const u = a.user
              return (
                <li key={u.id}>
                  <Card className="flex flex-col gap-3 p-4">
                    <div className="flex items-start gap-3">
                      <Avatar user={u} size="lg" />
                      <div className="min-w-0 flex-1">
                        <p className="font-medium text-fg" dir="auto">
                          {u.displayName}
                          {u.id === me?.id && <span className="ms-1 text-xs font-normal text-subtle">({t('users.you')})</span>}
                        </p>
                        <p className="text-xs text-muted" dir="ltr">
                          {u.email}
                        </p>
                      </div>
                      {status(u)}
                    </div>
                    <dl className="grid grid-cols-2 gap-x-3 gap-y-1 text-xs">
                      <dt className="text-subtle">{t('users.lastLogin')}</dt>
                      <dd className="text-fg">{lastLogin(u)}</dd>
                      <dt className="text-subtle">{t('users.content')}</dt>
                      <dd className="text-fg">{counts(a)}</dd>
                      <dt className="text-subtle">{t('users.storage')}</dt>
                      <dd className="text-fg" dir="ltr">
                        {formatBytes(a.storageBytes)}
                      </dd>
                    </dl>
                    <div className="flex flex-wrap items-center gap-2">
                      {renderRole(u, 'w-full min-w-40 flex-1')}
                      {renderActions(u)}
                    </div>
                  </Card>
                </li>
              )
            })}
          </ul>

          {/* ≥ 768px: table */}
          <Card className="hidden overflow-x-auto md:block">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-border text-xs tracking-wide text-subtle uppercase">
                  <th scope="col" className="px-4 py-3 text-start font-semibold">{t('users.name')}</th>
                  <th scope="col" className="px-4 py-3 text-start font-semibold">{t('users.role')}</th>
                  <th scope="col" className="px-4 py-3 text-start font-semibold">{t('users.status')}</th>
                  <th scope="col" className="px-4 py-3 text-start font-semibold">{t('users.lastLogin')}</th>
                  <th scope="col" className="hidden px-4 py-3 text-start font-semibold lg:table-cell">{t('users.content')}</th>
                  <th scope="col" className="px-4 py-3 text-start font-semibold">{t('users.storage')}</th>
                  {canManage && (
                    <th scope="col" className="px-4 py-3 text-end font-semibold">
                      <span className="sr-only">{t('users.actions')}</span>
                    </th>
                  )}
                </tr>
              </thead>
              <tbody>
                {items.map((a) => {
                  const u = a.user
                  return (
                    <tr key={u.id} className="border-b border-border last:border-0">
                      <td className="px-4 py-3">
                        <div className="flex items-center gap-3">
                          <Avatar user={u} />
                          <div className="min-w-0">
                            <p className="font-medium text-fg" dir="auto">
                              {u.displayName}
                              {u.id === me?.id && <span className="ms-1 text-xs font-normal text-subtle">({t('users.you')})</span>}
                            </p>
                            <p className="text-xs text-muted" dir="ltr">
                              {u.email}
                            </p>
                          </div>
                        </div>
                      </td>
                      <td className="px-4 py-3">{renderRole(u)}</td>
                      <td className="px-4 py-3">{status(u)}</td>
                      <td className="px-4 py-3 whitespace-nowrap text-muted">{lastLogin(u)}</td>
                      <td className="hidden px-4 py-3 text-xs text-muted lg:table-cell">{counts(a)}</td>
                      <td className="px-4 py-3 whitespace-nowrap text-muted" dir="ltr">
                        {formatBytes(a.storageBytes)}
                      </td>
                      {canManage && <td className="px-4 py-3">{renderActions(u)}</td>}
                    </tr>
                  )
                })}
              </tbody>
            </table>
          </Card>

          {pages > 1 && (
            <nav className="mt-5 flex items-center justify-between gap-3" aria-label={t('users.pageOf', { page, pages })}>
              <Button disabled={page <= 1} onClick={() => set('page', String(page - 1), false)}>
                {t('common.previous')}
              </Button>
              <span className="text-sm text-muted tabular-nums">{t('users.pageOf', { page, pages })}</span>
              <Button disabled={page >= pages} onClick={() => set('page', String(page + 1), false)}>
                {t('common.next')}
              </Button>
            </nav>
          )}
        </>
      )}

      {canManage && (
        <>
          <UserFormDialog open={creating} onClose={() => setCreating(false)} roles={roleList} />
          <UserFormDialog open={!!editing} onClose={() => setEditing(null)} roles={roleList} user={editing} />
          <ResetPasswordDialog user={resetting} onClose={() => setResetting(null)} />
          <DeleteUserDialog user={deleting} onClose={() => setDeleting(null)} />
        </>
      )}
    </div>
  )
}
