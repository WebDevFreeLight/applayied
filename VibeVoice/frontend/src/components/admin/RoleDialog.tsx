import { useMutation, useQueryClient } from '@tanstack/react-query'
import { AlertTriangle, Lock } from 'lucide-react'
import { useEffect, useId, useRef, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { toast } from 'sonner'
import { rolesApi } from '@/api/auth'
import { ApiError, errorMessage } from '@/api/client'
import { usePermissionCatalog } from '@/api/hooks'
import { qk } from '@/api/keys'
import type { Permission, RoleDto } from '@/api/types'
import { useAuth } from '@/auth/context'
import { groupPermissions, isAdminRole, roleLabel } from '@/lib/admin'
import { Button } from '../ui/Button'
import { Field, Input, Textarea } from '../ui/Field'
import { Modal } from '../ui/Modal'

/** Group "select all" checkbox with an indeterminate state. */
function GroupToggle({
  checked,
  indeterminate,
  disabled,
  onChange,
  label,
}: {
  checked: boolean
  indeterminate: boolean
  disabled?: boolean
  onChange: (v: boolean) => void
  label: string
}) {
  const ref = useRef<HTMLInputElement>(null)
  useEffect(() => {
    if (ref.current) ref.current.indeterminate = indeterminate
  }, [indeterminate])
  return (
    <label className="inline-flex cursor-pointer items-center gap-2 text-xs font-medium text-muted select-none">
      <input
        ref={ref}
        type="checkbox"
        className="size-4 rounded accent-[var(--accent)] pointer-coarse:size-5"
        checked={checked}
        disabled={disabled}
        onChange={(e) => onChange(e.target.checked)}
      />
      {label}
    </label>
  )
}

/** Permission checkboxes grouped Content / Media / AI / Administration with explanations. */
export function PermissionPicker({
  value,
  onChange,
  readOnly,
}: {
  value: readonly Permission[]
  onChange: (v: Permission[]) => void
  readOnly?: boolean
}) {
  const { t } = useTranslation()
  const catalog = usePermissionCatalog()
  const groups = groupPermissions(catalog)
  const has = (p: Permission) => value.includes(p)
  const toggle = (p: Permission, on: boolean) => onChange(on ? [...new Set([...value, p])] : value.filter((x) => x !== p))

  return (
    <div className="flex flex-col gap-4">
      {groups.map(({ group, keys }) => {
        const selected = keys.filter(has).length
        const headingId = `perm-group-${group}`
        return (
          <fieldset key={group} className="rounded-2xl border border-border p-3 sm:p-4" aria-labelledby={headingId}>
            <div className="mb-2 flex flex-wrap items-start justify-between gap-2">
              <div>
                <legend id={headingId} className="text-sm font-semibold text-fg">
                  {t(`permissions.groups.${group}`)}
                </legend>
                <p className="text-xs text-muted">{t(`permissions.groupDescriptions.${group}`)}</p>
              </div>
              {!readOnly && (
                <GroupToggle
                  label={t('roles.selectAll')}
                  checked={selected === keys.length}
                  indeterminate={selected > 0 && selected < keys.length}
                  onChange={(on) =>
                    onChange(on ? [...new Set([...value, ...keys])] : value.filter((p) => !keys.includes(p)))
                  }
                />
              )}
            </div>
            <ul className="flex flex-col gap-1">
              {keys.map((p) => (
                <li key={p}>
                  <label className="flex cursor-pointer items-start gap-3 rounded-xl px-2 py-2 hover:bg-surface-2 pointer-coarse:py-3">
                    <input
                      type="checkbox"
                      className="mt-0.5 size-4 shrink-0 rounded accent-[var(--accent)] pointer-coarse:size-5"
                      checked={has(p)}
                      disabled={readOnly}
                      onChange={(e) => toggle(p, e.target.checked)}
                      data-permission={p}
                    />
                    <span className="min-w-0">
                      <span className="block text-sm font-medium text-fg">{t(`permissions.names.${p}`)}</span>
                      <span className="block text-xs text-muted">{t(`permissions.descriptions.${p}`)}</span>
                    </span>
                  </label>
                </li>
              ))}
            </ul>
          </fieldset>
        )
      })}
    </div>
  )
}

/** Create / edit role. The Admin role opens read-only; built-in roles can't be renamed. */
export function RoleDialog({ open, role, onClose }: { open: boolean; role: RoleDto | null; onClose: () => void }) {
  const { t } = useTranslation()
  const readOnly = !!role && isAdminRole(role)
  const title = !role
    ? t('roles.createTitle')
    : readOnly
      ? t('roles.viewTitle', { name: roleLabel(t, role.name) })
      : t('roles.editTitle', { name: roleLabel(t, role.name) })
  return (
    <Modal open={open} onClose={onClose} title={title} size="lg">
      {open && <RoleForm key={role?.id ?? 'new'} role={role} readOnly={readOnly} onDone={onClose} />}
    </Modal>
  )
}

function RoleForm({ role, readOnly, onDone }: { role: RoleDto | null; readOnly: boolean; onDone: () => void }) {
  const { t } = useTranslation()
  const qc = useQueryClient()
  const { reload } = useAuth()
  const nameId = useId()
  const [name, setName] = useState(role?.name ?? '')
  const [description, setDescription] = useState(role?.description ?? '')
  const [permissions, setPermissions] = useState<Permission[]>(role?.permissions ?? [])
  const [touched, setTouched] = useState(false)
  const builtIn = !!role?.isSystem
  const nameError = touched && !name.trim() ? t('roles.nameRequired') : undefined

  const save = useMutation({
    mutationFn: () => {
      const body = { name: name.trim(), description: description.trim() || null, permissions }
      return role ? rolesApi.update(role.id, body) : rolesApi.create(body)
    },
    onSuccess: () => {
      toast.success(role ? t('roles.saved') : t('roles.created'))
      void qc.invalidateQueries({ queryKey: qk.roles })
      void qc.invalidateQueries({ queryKey: qk.users.all })
      // My own permissions may have changed.
      reload()
      onDone()
    },
  })
  const server = save.error instanceof ApiError ? save.error.fieldErrors() : {}

  return (
    <form
      noValidate
      onSubmit={(e) => {
        e.preventDefault()
        setTouched(true)
        if (!readOnly && name.trim()) save.mutate()
      }}
      className="flex flex-col gap-4"
    >
      {readOnly && (
        <p className="flex items-start gap-2 rounded-xl bg-accent-soft px-3 py-2 text-sm text-accent-text">
          <Lock className="mt-0.5 size-4 shrink-0" aria-hidden />
          {t('roles.adminReadOnly')}
        </p>
      )}
      {builtIn && !readOnly && <p className="rounded-xl bg-surface-2 px-3 py-2 text-sm text-muted">{t('roles.userNotRenamable')}</p>}
      <div className="grid gap-4 sm:grid-cols-2">
        <Field label={t('roles.name')} error={nameError ?? server.name}>
          {(p) => (
            <Input
              {...p}
              id={p.id || nameId}
              value={name}
              onChange={(e) => setName(e.target.value)}
              disabled={builtIn}
              maxLength={50}
              autoComplete="off"
            />
          )}
        </Field>
        <Field label={t('roles.description')} error={server.description}>
          {(p) => (
            <Textarea
              {...p}
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              disabled={readOnly}
              placeholder={t('roles.descriptionPlaceholder')}
              maxLength={300}
              className="min-h-10"
              rows={1}
            />
          )}
        </Field>
      </div>
      <div>
        <h3 className="mb-2 text-sm font-semibold text-fg">{t('roles.permissions')}</h3>
        <PermissionPicker value={permissions} onChange={setPermissions} readOnly={readOnly} />
      </div>
      {save.error && !Object.keys(server).length && (
        <p role="alert" className="flex items-start gap-2 rounded-xl bg-danger-soft px-3 py-2 text-sm text-danger">
          <AlertTriangle className="mt-0.5 size-4 shrink-0" aria-hidden />
          {errorMessage(save.error)}
        </p>
      )}
      <div className="flex flex-wrap justify-end gap-2 border-t border-border pt-4">
        <Button onClick={onDone}>{readOnly ? t('common.close') : t('common.cancel')}</Button>
        {!readOnly && (
          <Button type="submit" variant="primary" loading={save.isPending}>
            {role ? t('common.save') : t('roles.create')}
          </Button>
        )}
      </div>
    </form>
  )
}
