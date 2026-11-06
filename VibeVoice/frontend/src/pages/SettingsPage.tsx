import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation } from '@tanstack/react-query'
import { KeyRound, Palette, Settings, UserRound } from 'lucide-react'
import { useForm, useWatch } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { toast } from 'sonner'
import { z } from 'zod'
import { authApi } from '@/api/auth'
import { ApiError, errorMessage } from '@/api/client'
import { useVoices } from '@/api/hooks'
import type { UserDto } from '@/api/types'
import { useAuth } from '@/auth/context'
import { VoiceSelect } from '@/components/audio/VoiceSelect'
import { Button } from '@/components/ui/Button'
import { Badge, Card, PageHeader } from '@/components/ui/Display'
import { Field, Input, Select } from '@/components/ui/Field'
import { setLanguage } from '@/i18n'
import { useDates } from '@/lib/dates'
import { passwordSchema } from '@/lib/schemas'
import { useTheme } from '@/theme/theme'

export function SettingsPage() {
  const { t } = useTranslation()
  const { user } = useAuth()
  const dates = useDates()
  if (!user) return null
  return (
    <div className="mx-auto max-w-2xl">
      <PageHeader icon={Settings} title={t('settings.title')} subtitle={t('settings.subtitle')} />
      <div className="flex flex-col gap-6">
        <Card className="flex flex-wrap items-center gap-4 p-5">
          <span className="flex size-12 items-center justify-center rounded-full bg-accent text-lg font-semibold text-accent-fg uppercase">
            {(user.displayName || user.email).charAt(0)}
          </span>
          <div className="min-w-0 flex-1">
            <p className="truncate font-semibold text-fg" dir="auto">
              {user.displayName}
            </p>
            <p className="truncate text-sm text-muted">{user.email}</p>
            <p className="text-xs text-subtle">{t('settings.memberSince', { date: dates.format(user.createdAt, 'PP') })}</p>
          </div>
          <Badge tone={user.role === 'Admin' ? 'accent' : 'neutral'}>{t(`users.roles.${user.role}`)}</Badge>
        </Card>
        <ProfileForm key={`${user.id}-${user.preferredLanguage}-${user.theme}-${user.preferredVoice}`} user={user} />
        <PasswordForm />
      </div>
    </div>
  )
}

function ProfileForm({ user }: { user: UserDto }) {
  const { t } = useTranslation()
  const { setUser } = useAuth()
  const { setTheme } = useTheme()
  const voices = useVoices()
  const schema = z.object({
    displayName: z.string().trim().min(1, t('validation.nameRequired')).max(100, t('validation.nameTooLong')),
    preferredLanguage: z.enum(['en', 'ar']),
    theme: z.enum(['light', 'dark', 'system']),
    preferredVoice: z.string(),
  })
  type Values = z.infer<typeof schema>
  const form = useForm<Values>({
    resolver: zodResolver(schema),
    defaultValues: {
      displayName: user.displayName,
      preferredLanguage: user.preferredLanguage,
      theme: user.theme,
      preferredVoice: user.preferredVoice ?? '',
    },
  })
  const save = useMutation({
    mutationFn: (v: Values) =>
      authApi.updateProfile({
        displayName: v.displayName.trim(),
        preferredLanguage: v.preferredLanguage,
        theme: v.theme,
        preferredVoice: v.preferredVoice || null,
      }),
    onSuccess: (u) => {
      setUser(u)
      void setLanguage(u.preferredLanguage)
      setTheme(u.theme)
      toast.success(t('settings.profileSaved'))
    },
    onError: (err) => toast.error(errorMessage(err)),
  })
  const { errors } = form.formState
  const serverErrors = save.error instanceof ApiError ? save.error.fieldErrors() : {}
  const voice = useWatch({ control: form.control, name: 'preferredVoice' })

  return (
    <Card className="p-5">
      <form noValidate onSubmit={form.handleSubmit((v) => save.mutate(v))} className="flex flex-col gap-5">
        <h2 className="flex items-center gap-2 text-base font-semibold text-fg">
          <UserRound className="size-4 text-accent-text" aria-hidden />
          {t('settings.profile')}
        </h2>
        <Field label={t('auth.displayName')} error={errors.displayName?.message ?? serverErrors.displayName}>
          {(p) => <Input {...p} {...form.register('displayName')} autoComplete="name" />}
        </Field>

        <h3 className="mt-2 flex items-center gap-2 text-sm font-semibold text-fg">
          <Palette className="size-4 text-accent-text" aria-hidden />
          {t('settings.appearance')}
        </h3>
        <div className="grid gap-4 sm:grid-cols-2">
          <Field label={t('settings.language')}>
            {(p) => (
              <Select {...p} {...form.register('preferredLanguage')}>
                <option value="en">{t('languages.en')}</option>
                <option value="ar">{t('languages.ar')}</option>
              </Select>
            )}
          </Field>
          <Field label={t('settings.theme')}>
            {(p) => (
              <Select {...p} {...form.register('theme')}>
                <option value="light">{t('settings.themes.light')}</option>
                <option value="dark">{t('settings.themes.dark')}</option>
                <option value="system">{t('settings.themes.system')}</option>
              </Select>
            )}
          </Field>
        </div>
        <Field label={t('settings.defaultVoice')} hint={t('settings.defaultVoiceHint')}>
          {(p) => (
            <VoiceSelect
              {...p}
              voices={voices.data ?? []}
              value={voice}
              onChange={(v) => form.setValue('preferredVoice', v, { shouldDirty: true })}
              emptyLabel={t('settings.noVoice')}
            />
          )}
        </Field>
        <div className="flex justify-end">
          <Button type="submit" variant="primary" loading={save.isPending}>
            {t('settings.saveProfile')}
          </Button>
        </div>
      </form>
    </Card>
  )
}

function PasswordForm() {
  const { t } = useTranslation()
  const schema = z
    .object({
      currentPassword: z.string().min(1, t('common.required')),
      newPassword: passwordSchema(t('validation.passwordRules')),
      confirm: z.string(),
    })
    .refine((v) => v.newPassword === v.confirm, { path: ['confirm'], message: t('validation.passwordMismatch') })
  type Values = z.infer<typeof schema>
  const form = useForm<Values>({
    resolver: zodResolver(schema),
    defaultValues: { currentPassword: '', newPassword: '', confirm: '' },
  })
  const change = useMutation({
    mutationFn: (v: Values) => authApi.changePassword({ currentPassword: v.currentPassword, newPassword: v.newPassword }),
    onSuccess: () => {
      toast.success(t('settings.passwordChanged'))
      form.reset()
    },
    onError: (err) => toast.error(errorMessage(err)),
  })
  const { errors } = form.formState
  const serverErrors = change.error instanceof ApiError ? change.error.fieldErrors() : {}

  return (
    <Card className="p-5">
      <form noValidate onSubmit={form.handleSubmit((v) => change.mutate(v))} className="flex flex-col gap-4">
        <h2 className="flex items-center gap-2 text-base font-semibold text-fg">
          <KeyRound className="size-4 text-accent-text" aria-hidden />
          {t('settings.password')}
        </h2>
        <Field label={t('settings.currentPassword')} error={errors.currentPassword?.message ?? serverErrors.currentPassword}>
          {(p) => <Input {...p} {...form.register('currentPassword')} type="password" autoComplete="current-password" dir="ltr" />}
        </Field>
        <div className="grid gap-4 sm:grid-cols-2">
          <Field label={t('settings.newPassword')} error={errors.newPassword?.message ?? serverErrors.newPassword} hint={t('validation.passwordRules')}>
            {(p) => <Input {...p} {...form.register('newPassword')} type="password" autoComplete="new-password" dir="ltr" />}
          </Field>
          <Field label={t('settings.confirmNewPassword')} error={errors.confirm?.message}>
            {(p) => <Input {...p} {...form.register('confirm')} type="password" autoComplete="new-password" dir="ltr" />}
          </Field>
        </div>
        <div className="flex justify-end">
          <Button type="submit" loading={change.isPending}>
            {t('settings.changePassword')}
          </Button>
        </div>
      </form>
    </Card>
  )
}
