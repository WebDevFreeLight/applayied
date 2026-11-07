import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { AlertTriangle, KeyRound, Trash2 } from 'lucide-react'
import { useId, useState } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { toast } from 'sonner'
import { z } from 'zod'
import { usersApi } from '@/api/auth'
import { ApiError, errorMessage } from '@/api/client'
import { qk } from '@/api/keys'
import { USER_ROLE_ID, type RoleDto, type UserDto } from '@/api/types'
import { useAuth } from '@/auth/context'
import { normalizeLanguage } from '@/i18n'
import { roleLabel } from '@/lib/admin'
import { passwordSchema } from '@/lib/schemas'
import { Button } from '../ui/Button'
import { Checkbox, Field, Input, Select } from '../ui/Field'
import { Modal } from '../ui/Modal'
import { PasswordField } from './PasswordField'

function useInvalidateUsers() {
  const qc = useQueryClient()
  const { user: me, reload } = useAuth()
  return (changed?: UserDto) => {
    void qc.invalidateQueries({ queryKey: qk.users.all })
    void qc.invalidateQueries({ queryKey: qk.roles })
    void qc.invalidateQueries({ queryKey: qk.adminOverview })
    if (changed && changed.id === me?.id) reload()
  }
}

function ServerError({ error }: { error: unknown }) {
  if (!error) return null
  return (
    <p role="alert" className="flex items-start gap-2 rounded-xl bg-danger-soft px-3 py-2 text-sm text-danger">
      <AlertTriangle className="mt-0.5 size-4 shrink-0" aria-hidden />
      {errorMessage(error)}
    </p>
  )
}

export interface UserFormDialogProps {
  open: boolean
  onClose: () => void
  roles: RoleDto[]
  /** Edit when set, create otherwise. */
  user?: UserDto | null
}

/** Create / edit user dialog (RHF + Zod, server validation errors mapped to fields). */
export function UserFormDialog({ open, onClose, roles, user }: UserFormDialogProps) {
  const { t } = useTranslation()
  return (
    <Modal
      open={open}
      onClose={onClose}
      title={user ? t('users.editTitle', { name: user.displayName }) : t('users.createTitle')}
      size="md"
    >
      {open && <UserForm key={user?.id ?? 'new'} roles={roles} user={user ?? null} onDone={onClose} />}
    </Modal>
  )
}

function UserForm({ roles, user, onDone }: { roles: RoleDto[]; user: UserDto | null; onDone: () => void }) {
  const { t, i18n } = useTranslation()
  const { user: me } = useAuth()
  const invalidate = useInvalidateUsers()
  const isEdit = !!user
  const self = !!user && user.id === me?.id
  const pwId = useId()

  const schema = z.object({
    email: z.string().trim().email(t('validation.emailInvalid')),
    displayName: z.string().trim().min(1, t('validation.nameRequired')).max(100, t('validation.nameTooLong')),
    password: isEdit ? z.string().optional() : passwordSchema(t('validation.passwordRules')),
    roleId: z.string().min(1, t('common.required')),
    preferredLanguage: z.enum(['en', 'ar']),
    isActive: z.boolean(),
  })
  type Values = z.infer<typeof schema>
  const form = useForm<Values>({
    resolver: zodResolver(schema),
    defaultValues: {
      email: user?.email ?? '',
      displayName: user?.displayName ?? '',
      password: '',
      roleId: user?.roleId ?? roles.find((r) => r.id === USER_ROLE_ID)?.id ?? roles[0]?.id ?? '',
      preferredLanguage: user?.preferredLanguage ?? normalizeLanguage(i18n.language),
      isActive: user?.isActive ?? true,
    },
  })

  const save = useMutation({
    mutationFn: (v: Values) =>
      user
        ? usersApi.update(user.id, {
            email: v.email.trim(),
            displayName: v.displayName.trim(),
            roleId: v.roleId,
            preferredLanguage: v.preferredLanguage,
            isActive: v.isActive,
          })
        : usersApi.create({
            email: v.email.trim(),
            displayName: v.displayName.trim(),
            password: v.password ?? '',
            roleId: v.roleId,
            preferredLanguage: v.preferredLanguage,
            isActive: v.isActive,
          }),
    onSuccess: (u) => {
      toast.success(isEdit ? t('users.updated') : t('users.created'))
      invalidate(u)
      onDone()
    },
  })
  const { errors } = form.formState
  const server = save.error instanceof ApiError ? save.error.fieldErrors() : {}
  const generalError = save.error && !Object.keys(server).length ? save.error : null

  return (
    <form noValidate onSubmit={form.handleSubmit((v) => save.mutate(v))} className="flex flex-col gap-4">
      <Field label={t('auth.displayName')} error={errors.displayName?.message ?? server.displayName}>
        {(p) => <Input {...p} {...form.register('displayName')} autoComplete="off" />}
      </Field>
      <Field label={t('auth.email')} error={errors.email?.message ?? server.email}>
        {(p) => <Input {...p} {...form.register('email')} type="email" dir="ltr" autoComplete="off" />}
      </Field>
      {!isEdit && (
        <Field label={t('users.password')} error={errors.password?.message ?? server.password} hint={t('validation.passwordRules')}>
          {(p) => (
            <Controller
              control={form.control}
              name="password"
              render={({ field }) => (
                <PasswordField
                  id={p.id || pwId}
                  value={field.value ?? ''}
                  onChange={field.onChange}
                  invalid={p['aria-invalid']}
                  describedBy={p['aria-describedby']}
                />
              )}
            />
          )}
        </Field>
      )}
      <div className="grid gap-4 sm:grid-cols-2">
        <Field label={t('users.role')} error={errors.roleId?.message ?? server.roleId} hint={self ? t('users.selfRole') : undefined}>
          {(p) => (
            // For yourself the field is shown read-only and not registered (the default value is submitted).
            <Select {...p} {...(self ? { value: user?.roleId, disabled: true, onChange: () => {} } : form.register('roleId'))}>
              {roles.map((r) => (
                <option key={r.id} value={r.id}>
                  {roleLabel(t, r.name)}
                </option>
              ))}
            </Select>
          )}
        </Field>
        <Field label={t('users.language')}>
          {(p) => (
            <Select {...p} {...form.register('preferredLanguage')}>
              <option value="en">{t('languages.en')}</option>
              <option value="ar">{t('languages.ar')}</option>
            </Select>
          )}
        </Field>
      </div>
      <div className="flex flex-col gap-1">
        {self ? (
          <Checkbox label={t('users.activeLabel')} checked disabled readOnly />
        ) : (
          <Checkbox label={t('users.activeLabel')} {...form.register('isActive')} />
        )}
        <p className="text-xs text-muted">{self ? t('users.selfDeactivate') : t('users.activeHint')}</p>
      </div>
      <ServerError error={generalError} />
      <div className="flex flex-wrap justify-end gap-2 border-t border-border pt-4">
        <Button onClick={onDone}>{t('common.cancel')}</Button>
        <Button type="submit" variant="primary" loading={save.isPending}>
          {isEdit ? t('common.save') : t('users.create')}
        </Button>
      </div>
    </form>
  )
}

/** Admin password reset (generate + copy). */
export function ResetPasswordDialog({ user, onClose }: { user: UserDto | null; onClose: () => void }) {
  const { t } = useTranslation()
  return (
    <Modal open={!!user} onClose={onClose} title={user ? t('users.resetTitle', { name: user.displayName }) : ''} description={t('users.resetBody')} size="md">
      {user && <ResetForm key={user.id} user={user} onDone={onClose} />}
    </Modal>
  )
}

function ResetForm({ user, onDone }: { user: UserDto; onDone: () => void }) {
  const { t } = useTranslation()
  const [pw, setPw] = useState('')
  const [touched, setTouched] = useState(false)
  const id = useId()
  const valid = passwordSchema('x').safeParse(pw).success
  const reset = useMutation({
    mutationFn: () => usersApi.resetPassword(user.id, pw),
    onSuccess: () => {
      toast.success(t('users.resetDone'))
      onDone()
    },
  })
  return (
    <form
      noValidate
      onSubmit={(e) => {
        e.preventDefault()
        setTouched(true)
        if (valid) reset.mutate()
      }}
      className="flex flex-col gap-4"
    >
      <Field label={t('users.newPassword')} error={touched && !valid ? t('validation.passwordRules') : undefined} hint={t('validation.passwordRules')}>
        {(p) => <PasswordField id={p.id || id} value={pw} onChange={setPw} invalid={p['aria-invalid']} describedBy={p['aria-describedby']} />}
      </Field>
      <ServerError error={reset.error} />
      <div className="flex flex-wrap justify-end gap-2 border-t border-border pt-4">
        <Button onClick={onDone}>{t('common.cancel')}</Button>
        <Button type="submit" variant="primary" loading={reset.isPending} icon={<KeyRound className="size-4" aria-hidden />}>
          {t('users.resetPassword')}
        </Button>
      </div>
    </form>
  )
}

/** Strong delete confirmation: the admin must type the user's email. */
export function DeleteUserDialog({ user, onClose }: { user: UserDto | null; onClose: () => void }) {
  const { t } = useTranslation()
  return (
    <Modal open={!!user} onClose={onClose} title={user ? t('users.deleteTitle', { name: user.displayName }) : ''} size="md">
      {user && <DeleteForm key={user.id} user={user} onDone={onClose} />}
    </Modal>
  )
}

function DeleteForm({ user, onDone }: { user: UserDto; onDone: () => void }) {
  const { t } = useTranslation()
  const invalidate = useInvalidateUsers()
  const [typed, setTyped] = useState('')
  const matches = typed.trim().toLowerCase() === user.email.toLowerCase()
  const remove = useMutation({
    mutationFn: () => usersApi.remove(user.id),
    onSuccess: () => {
      toast.success(t('users.deleted'))
      invalidate()
      onDone()
    },
  })
  return (
    <form
      noValidate
      onSubmit={(e) => {
        e.preventDefault()
        if (matches) remove.mutate()
      }}
      className="flex flex-col gap-4"
    >
      <p className="flex items-start gap-2 rounded-xl bg-danger-soft px-3 py-3 text-sm text-danger">
        <AlertTriangle className="mt-0.5 size-4 shrink-0" aria-hidden />
        {t('users.deleteBody')}
      </p>
      <Field
        label={t('users.deleteConfirmLabel', { email: user.email })}
        error={typed && !matches ? t('users.deleteMismatch') : undefined}
      >
        {(p) => (
          <Input
            {...p}
            value={typed}
            onChange={(e) => setTyped(e.target.value)}
            dir="ltr"
            autoComplete="off"
            placeholder={user.email}
            data-testid="delete-confirm-input"
          />
        )}
      </Field>
      <ServerError error={remove.error} />
      <div className="flex flex-wrap justify-end gap-2 border-t border-border pt-4">
        <Button onClick={onDone}>{t('common.cancel')}</Button>
        <Button
          type="submit"
          variant="danger"
          disabled={!matches}
          loading={remove.isPending}
          icon={<Trash2 className="size-4" aria-hidden />}
        >
          {t('users.delete')}
        </Button>
      </div>
    </form>
  )
}

