import { useMutation, useQueryClient } from '@tanstack/react-query'
import { Eye, Pencil, ShieldPlus, Trash2, Users } from 'lucide-react'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router'
import { toast } from 'sonner'
import { rolesApi } from '@/api/auth'
import { useRoles } from '@/api/hooks'
import { qk } from '@/api/keys'
import type { RoleDto } from '@/api/types'
import { useAuth } from '@/auth/context'
import { RoleDialog } from '@/components/admin/RoleDialog'
import { ArtTile } from '@/components/art/ArtTile'
import { Button } from '@/components/ui/Button'
import { Badge, Card, EmptyState, ErrorState, PageHeader, Skeleton } from '@/components/ui/Display'
import { ConfirmDialog } from '@/components/ui/Modal'
import { isAdminRole, roleLabel } from '@/lib/admin'
import { toastError } from '@/lib/notify'

export function RolesPage() {
  const { t } = useTranslation()
  const qc = useQueryClient()
  const { can } = useAuth()
  const canManage = can('roles.manage')
  const roles = useRoles()
  const [dialog, setDialog] = useState<{ role: RoleDto | null } | null>(null)
  const [pendingDelete, setPendingDelete] = useState<RoleDto | null>(null)

  const remove = useMutation({
    mutationFn: (role: RoleDto) => rolesApi.remove(role.id),
    onSuccess: () => {
      toast.success(t('roles.deleted'))
      setPendingDelete(null)
      void qc.invalidateQueries({ queryKey: qk.roles })
    },
    // 409 (still in use) / 400 (built-in) messages come from the API.
    onError: (err) => toastError(err),
  })

  const deleteBlocker = (r: RoleDto): string | null =>
    r.isSystem ? t('roles.deleteBuiltIn') : r.userCount > 0 ? t('roles.deleteInUse', { count: r.userCount }) : null

  return (
    <div>
      <PageHeader
        art="roles"
        title={t('roles.title')}
        subtitle={t('roles.subtitle')}
        actions={
          canManage && (
            <Button variant="primary" onClick={() => setDialog({ role: null })} icon={<ShieldPlus className="size-4" aria-hidden />}>
              {t('roles.create')}
            </Button>
          )
        }
      />

      {roles.isLoading ? (
        <div className="grid gap-4 md:grid-cols-2">
          {Array.from({ length: 4 }, (_, i) => (
            <Skeleton key={i} className="h-48" />
          ))}
        </div>
      ) : roles.error ? (
        <ErrorState error={roles.error} onRetry={() => void roles.refetch()} />
      ) : !roles.data?.length ? (
        <EmptyState art="users" title={t('roles.empty')} />
      ) : (
        <ul className="grid gap-4 md:grid-cols-2 2xl:grid-cols-3">
          {roles.data.map((r) => {
            const admin = isAdminRole(r)
            const blocker = deleteBlocker(r)
            return (
              <li key={r.id} className="min-w-0">
                <Card className="flex h-full flex-col gap-3 p-4 sm:p-5" data-testid={`role-${r.name}`}>
                  <div className="flex items-start gap-3">
                    <ArtTile name={admin ? 'roles' : 'users'} size="md" />
                    <div className="min-w-0 flex-1">
                      <h2 className="flex flex-wrap items-center gap-2 text-base font-semibold text-fg" dir="auto">
                        {roleLabel(t, r.name)}
                        {r.isSystem && <Badge tone="accent">{t('roles.builtIn')}</Badge>}
                      </h2>
                      {r.description && (
                        <p className="text-sm text-muted" dir="auto">
                          {r.description}
                        </p>
                      )}
                    </div>
                  </div>

                  <Link
                    to={`/users?roleId=${encodeURIComponent(r.id)}`}
                    className="inline-flex items-center gap-1.5 self-start text-sm font-medium text-accent-text hover:underline"
                  >
                    <Users className="size-4" aria-hidden />
                    {t('roles.usersCount', { count: r.userCount })} · {t('roles.viewUsers')}
                  </Link>

                  <div>
                    <h3 className="sr-only">{t('roles.permissions')}</h3>
                    {r.permissions.length ? (
                      <ul className="flex flex-wrap gap-1.5" aria-label={t('roles.permissions')}>
                        {r.permissions.map((p) => (
                          <li key={p}>
                            <Badge tone={p.startsWith('users') || p.startsWith('roles') ? 'warning' : 'neutral'}>
                              {t(`permissions.names.${p}`)}
                            </Badge>
                          </li>
                        ))}
                      </ul>
                    ) : (
                      <p className="text-sm text-subtle">{t('roles.noPermissions')}</p>
                    )}
                  </div>

                  <div className="mt-auto flex flex-wrap items-center gap-2 border-t border-border pt-3">
                    {canManage ? (
                      <>
                        <Button
                          size="sm"
                          onClick={() => setDialog({ role: r })}
                          icon={admin ? <Eye className="size-4" aria-hidden /> : <Pencil className="size-4" aria-hidden />}
                        >
                          {admin ? t('common.open') : t('common.edit')}
                        </Button>
                        <span title={blocker ?? undefined} className="inline-flex">
                          <Button
                            size="sm"
                            variant="ghost"
                            className="text-danger hover:bg-danger-soft hover:text-danger"
                            disabled={!!blocker}
                            aria-describedby={blocker ? `role-block-${r.id}` : undefined}
                            onClick={() => setPendingDelete(r)}
                            icon={<Trash2 className="size-4" aria-hidden />}
                          >
                            {t('common.delete')}
                          </Button>
                        </span>
                        {blocker && (
                          <span id={`role-block-${r.id}`} className="text-xs text-subtle">
                            {blocker}
                          </span>
                        )}
                      </>
                    ) : (
                      <Button size="sm" onClick={() => setDialog({ role: r })} icon={<Eye className="size-4" aria-hidden />}>
                        {t('common.open')}
                      </Button>
                    )}
                  </div>
                </Card>
              </li>
            )
          })}
        </ul>
      )}

      <RoleDialog open={!!dialog} role={dialog?.role ?? null} onClose={() => setDialog(null)} />
      <ConfirmDialog
        open={!!pendingDelete}
        title={pendingDelete ? t('roles.deleteTitle', { name: roleLabel(t, pendingDelete.name) }) : ''}
        body={t('roles.deleteBody')}
        confirmLabel={t('common.delete')}
        loading={remove.isPending}
        onConfirm={() => pendingDelete && remove.mutate(pendingDelete)}
        onCancel={() => setPendingDelete(null)}
      />
    </div>
  )
}
