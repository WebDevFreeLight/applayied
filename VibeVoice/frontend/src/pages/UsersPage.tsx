import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Users } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { toast } from 'sonner'
import { usersApi } from '@/api/auth'
import { errorMessage } from '@/api/client'
import { qk } from '@/api/keys'
import type { Role, UserDto } from '@/api/types'
import { useAuth } from '@/auth/context'
import { Button } from '@/components/ui/Button'
import { Badge, Card, EmptyState, ErrorState, PageHeader, Skeleton } from '@/components/ui/Display'
import { Select } from '@/components/ui/Field'
import { useDates } from '@/lib/dates'

export function UsersPage() {
  const { t } = useTranslation()
  const dates = useDates()
  const qc = useQueryClient()
  const { user: me } = useAuth()
  const users = useQuery({ queryKey: qk.users, queryFn: usersApi.list })

  const onUpdated = (u: UserDto) => {
    qc.setQueryData<UserDto[]>(qk.users, (old) => old?.map((x) => (x.id === u.id ? u : x)))
    toast.success(t('users.updated'))
  }
  const setRole = useMutation({
    mutationFn: ({ id, role }: { id: string; role: Role }) => usersApi.setRole(id, role),
    onSuccess: onUpdated,
    onError: (err) => toast.error(errorMessage(err)),
  })
  const setActive = useMutation({
    mutationFn: ({ id, isActive }: { id: string; isActive: boolean }) => usersApi.setActive(id, isActive),
    onSuccess: onUpdated,
    onError: (err) => toast.error(errorMessage(err)),
  })

  return (
    <div>
      <PageHeader icon={Users} title={t('users.title')} subtitle={t('users.subtitle')} />
      {users.isLoading ? (
        <div className="flex flex-col gap-2">
          {Array.from({ length: 4 }, (_, i) => (
            <Skeleton key={i} className="h-14" />
          ))}
        </div>
      ) : users.error ? (
        <ErrorState error={users.error} onRetry={() => void users.refetch()} />
      ) : !users.data?.length ? (
        <EmptyState icon={Users} title={t('users.empty')} />
      ) : (
        <Card className="overflow-x-auto">
          <table className="w-full min-w-[640px] text-sm">
            <thead>
              <tr className="border-b border-border text-start text-xs tracking-wide text-subtle uppercase">
                <th scope="col" className="px-4 py-3 text-start font-semibold">{t('users.name')}</th>
                <th scope="col" className="px-4 py-3 text-start font-semibold">{t('users.role')}</th>
                <th scope="col" className="px-4 py-3 text-start font-semibold">{t('users.status')}</th>
                <th scope="col" className="px-4 py-3 text-start font-semibold">{t('users.created')}</th>
                <th scope="col" className="px-4 py-3"><span className="sr-only">{t('common.edit')}</span></th>
              </tr>
            </thead>
            <tbody>
              {users.data.map((u) => {
                const self = u.id === me?.id
                return (
                  <tr key={u.id} className="border-b border-border last:border-0">
                    <td className="px-4 py-3">
                      <div className="flex items-center gap-3">
                        <span className="flex size-8 shrink-0 items-center justify-center rounded-full bg-accent-soft text-xs font-semibold text-accent-text uppercase">
                          {(u.displayName || u.email).charAt(0)}
                        </span>
                        <div className="min-w-0">
                          <p className="truncate font-medium text-fg" dir="auto">
                            {u.displayName} {self && <span className="text-xs font-normal text-subtle">({t('common.you')})</span>}
                          </p>
                          <p className="truncate text-xs text-muted">{u.email}</p>
                        </div>
                      </div>
                    </td>
                    <td className="px-4 py-3">
                      <Select
                        value={u.role}
                        disabled={self || setRole.isPending}
                        title={self ? t('users.self') : undefined}
                        aria-label={`${t('users.role')} — ${u.displayName}`}
                        onChange={(e) => setRole.mutate({ id: u.id, role: e.target.value as Role })}
                        className="h-9 w-32"
                      >
                        <option value="Admin">{t('users.roles.Admin')}</option>
                        <option value="User">{t('users.roles.User')}</option>
                      </Select>
                    </td>
                    <td className="px-4 py-3">
                      <Badge tone={u.isActive ? 'success' : 'danger'}>{u.isActive ? t('users.active') : t('users.inactive')}</Badge>
                    </td>
                    <td className="px-4 py-3 text-muted">{dates.format(u.createdAt, 'PP')}</td>
                    <td className="px-4 py-3 text-end">
                      <Button
                        size="sm"
                        variant={u.isActive ? 'ghost' : 'secondary'}
                        disabled={self || setActive.isPending}
                        title={self ? t('users.self') : undefined}
                        onClick={() => setActive.mutate({ id: u.id, isActive: !u.isActive })}
                      >
                        {u.isActive ? t('users.deactivate') : t('users.activate')}
                      </Button>
                    </td>
                  </tr>
                )
              })}
            </tbody>
          </table>
        </Card>
      )}
    </div>
  )
}
