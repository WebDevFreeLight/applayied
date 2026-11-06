import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation } from '@tanstack/react-query'
import { Eye, EyeOff, Languages } from 'lucide-react'
import { useState, type ComponentProps, type ReactNode } from 'react'
import { useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router'
import { z } from 'zod'
import { authApi } from '@/api/auth'
import { ApiError, errorMessage } from '@/api/client'
import type { Language } from '@/api/types'
import { useAuth } from '@/auth/context'
import { Button } from '@/components/ui/Button'
import { Field, Input, Select } from '@/components/ui/Field'
import { normalizeLanguage, setLanguage } from '@/i18n'
import { passwordSchema } from '@/lib/schemas'

function AuthShell({ title, subtitle, children, footer }: { title: string; subtitle: string; children: ReactNode; footer: ReactNode }) {
  const { t, i18n } = useTranslation()
  const lang = normalizeLanguage(i18n.language)
  return (
    <div className="relative flex min-h-dvh items-center justify-center overflow-hidden bg-bg px-4 py-10">
      <div aria-hidden className="pointer-events-none absolute -top-40 start-1/2 size-[36rem] -translate-x-1/2 rounded-full bg-accent/15 blur-3xl rtl:translate-x-1/2" />
      <button
        type="button"
        onClick={() => void setLanguage(lang === 'ar' ? 'en' : 'ar')}
        className="absolute top-4 end-4 inline-flex h-9 items-center gap-1.5 rounded-xl px-3 text-sm font-medium text-muted hover:bg-surface-2 hover:text-fg"
        aria-label={t('header.language')}
      >
        <Languages className="size-4" aria-hidden />
        {lang === 'ar' ? t('header.switchToEnglish') : t('header.switchToArabic')}
      </button>
      <div className="relative w-full max-w-sm">
        <div className="mb-8 flex flex-col items-center text-center">
          <span className="mb-4 flex size-12 items-center justify-center rounded-2xl bg-gradient-to-br from-[#8b7cff] to-[#4f46e5] shadow-lg">
            <svg viewBox="0 0 24 24" className="size-6 fill-white" aria-hidden>
              <rect x="3" y="9" width="2.4" height="6" rx="1.2" />
              <rect x="7.2" y="5" width="2.4" height="14" rx="1.2" />
              <rect x="11.4" y="7" width="2.4" height="10" rx="1.2" />
              <rect x="15.6" y="3" width="2.4" height="18" rx="1.2" />
              <rect x="19.6" y="9.5" width="2" height="5" rx="1" />
            </svg>
          </span>
          <h1 className="text-2xl font-semibold tracking-tight text-fg">{title}</h1>
          <p className="mt-1 text-sm text-muted">{subtitle}</p>
        </div>
        <div className="rounded-2xl border border-border bg-surface p-6 shadow-card">{children}</div>
        <p className="mt-6 text-center text-sm text-muted">{footer}</p>
      </div>
    </div>
  )
}

function PasswordInput(props: ComponentProps<typeof Input>) {
  const { t } = useTranslation()
  const [show, setShow] = useState(false)
  return (
    <div className="relative">
      <Input {...props} dir="ltr" type={show ? 'text' : 'password'} className="pe-10" />
      <button
        type="button"
        onClick={() => setShow((s) => !s)}
        aria-label={show ? t('auth.hidePassword') : t('auth.showPassword')}
        className="absolute end-1 top-1/2 flex size-8 -translate-y-1/2 items-center justify-center rounded-lg text-subtle hover:text-fg"
      >
        {show ? <EyeOff className="size-4" aria-hidden /> : <Eye className="size-4" aria-hidden />}
      </button>
    </div>
  )
}

export function LoginPage() {
  const { t } = useTranslation()
  const { signIn } = useAuth()
  const schema = z.object({
    email: z.string().trim().email(t('validation.emailInvalid')),
    password: z.string().min(1, t('common.required')),
  })
  type Values = z.infer<typeof schema>
  const form = useForm<Values>({ resolver: zodResolver(schema), defaultValues: { email: '', password: '' } })
  const login = useMutation({
    mutationFn: authApi.login,
    onSuccess: signIn,
  })
  const { errors } = form.formState
  const serverError = login.error
    ? login.error instanceof ApiError && login.error.status === 401
      ? t('auth.badCredentials')
      : errorMessage(login.error)
    : null

  return (
    <AuthShell
      title={t('auth.loginTitle')}
      subtitle={t('auth.loginSubtitle')}
      footer={
        <>
          {t('auth.noAccount')}{' '}
          <Link to="/register" className="font-medium text-accent-text hover:underline">
            {t('auth.registerLink')}
          </Link>
        </>
      }
    >
      <form noValidate onSubmit={form.handleSubmit((v) => login.mutate(v))} className="flex flex-col gap-4">
        <Field label={t('auth.email')} error={errors.email?.message}>
          {(p) => <Input {...p} {...form.register('email')} type="email" autoComplete="email" dir="ltr" />}
        </Field>
        <Field label={t('auth.password')} error={errors.password?.message}>
          {(p) => <PasswordInput {...p} {...form.register('password')} autoComplete="current-password" />}
        </Field>
        {serverError && (
          <p role="alert" className="rounded-xl bg-danger-soft px-3 py-2 text-sm text-danger">
            {serverError}
          </p>
        )}
        <Button type="submit" variant="primary" size="lg" loading={login.isPending} className="w-full">
          {login.isPending ? t('auth.signingIn') : t('auth.signIn')}
        </Button>
        {import.meta.env.DEV && <p className="text-center text-xs text-subtle" dir="ltr">{t('auth.devHint')}</p>}
      </form>
    </AuthShell>
  )
}

export function RegisterPage() {
  const { t, i18n } = useTranslation()
  const { signIn } = useAuth()
  const schema = z
    .object({
      displayName: z.string().trim().min(1, t('validation.nameRequired')).max(100, t('validation.nameTooLong')),
      email: z.string().trim().email(t('validation.emailInvalid')),
      password: passwordSchema(t('validation.passwordRules')),
      confirm: z.string(),
      preferredLanguage: z.enum(['en', 'ar']),
    })
    .refine((v) => v.password === v.confirm, { path: ['confirm'], message: t('validation.passwordMismatch') })
  type Values = z.infer<typeof schema>
  const form = useForm<Values>({
    resolver: zodResolver(schema),
    defaultValues: {
      displayName: '',
      email: '',
      password: '',
      confirm: '',
      preferredLanguage: normalizeLanguage(i18n.language),
    },
  })
  const register = useMutation({ mutationFn: authApi.register, onSuccess: signIn })
  const { errors } = form.formState
  const fieldErrors = register.error instanceof ApiError ? register.error.fieldErrors() : {}

  return (
    <AuthShell
      title={t('auth.registerTitle')}
      subtitle={t('auth.registerSubtitle')}
      footer={
        <>
          {t('auth.haveAccount')}{' '}
          <Link to="/login" className="font-medium text-accent-text hover:underline">
            {t('auth.loginLink')}
          </Link>
        </>
      }
    >
      <form
        noValidate
        onSubmit={form.handleSubmit(({ confirm: _confirm, ...v }) => register.mutate(v))}
        className="flex flex-col gap-4"
      >
        <Field label={t('auth.displayName')} error={errors.displayName?.message ?? fieldErrors.displayName}>
          {(p) => <Input {...p} {...form.register('displayName')} autoComplete="name" />}
        </Field>
        <Field label={t('auth.email')} error={errors.email?.message ?? fieldErrors.email}>
          {(p) => <Input {...p} {...form.register('email')} type="email" autoComplete="email" dir="ltr" />}
        </Field>
        <Field
          label={t('auth.password')}
          error={errors.password?.message ?? fieldErrors.password}
          hint={t('validation.passwordRules')}
        >
          {(p) => <PasswordInput {...p} {...form.register('password')} autoComplete="new-password" />}
        </Field>
        <Field label={t('auth.confirmPassword')} error={errors.confirm?.message}>
          {(p) => <PasswordInput {...p} {...form.register('confirm')} autoComplete="new-password" />}
        </Field>
        <Field label={t('auth.preferredLanguage')}>
          {(p) => (
            <Select
              {...p}
              {...form.register('preferredLanguage', {
                onChange: (e) => void setLanguage(e.target.value as Language),
              })}
            >
              <option value="en">{t('languages.en')}</option>
              <option value="ar">{t('languages.ar')}</option>
            </Select>
          )}
        </Field>
        {register.error && !Object.keys(fieldErrors).length && (
          <p role="alert" className="rounded-xl bg-danger-soft px-3 py-2 text-sm text-danger">
            {errorMessage(register.error)}
          </p>
        )}
        <Button type="submit" variant="primary" size="lg" loading={register.isPending} className="w-full">
          {register.isPending ? t('auth.signingUp') : t('auth.signUp')}
        </Button>
      </form>
    </AuthShell>
  )
}
