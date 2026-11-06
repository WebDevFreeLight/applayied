import { Loader2 } from 'lucide-react'
import type { ReactNode } from 'react'
import { useTranslation } from 'react-i18next'
import { Navigate, useLocation } from 'react-router'
import { Button } from '@/components/ui/Button'
import { ErrorState } from '@/components/ui/Display'
import { useAuth } from './context'

function Splash() {
  const { t } = useTranslation()
  return (
    <div className="flex min-h-dvh items-center justify-center" role="status">
      <Loader2 className="size-6 animate-spin text-accent" aria-hidden />
      <span className="sr-only">{t('app.loading')}</span>
    </div>
  )
}

/** Route guard: redirects anonymous users to /login (remembering where they were going). */
export function RequireAuth({ children }: { children: ReactNode }) {
  const { token, user, isLoading, loadError, reload, signOut } = useAuth()
  const location = useLocation()
  const { t } = useTranslation()
  if (!token) return <Navigate to="/login" replace state={{ from: location }} />
  if (isLoading) return <Splash />
  if (!user) {
    return (
      <div className="mx-auto flex min-h-dvh max-w-md flex-col justify-center gap-4 p-6">
        <ErrorState error={loadError} onRetry={reload} />
        <Button variant="ghost" onClick={() => signOut()}>
          {t('header.logout')}
        </Button>
      </div>
    )
  }
  return <>{children}</>
}

export function RequireAdmin({ children }: { children: ReactNode }) {
  const { isAdmin } = useAuth()
  if (!isAdmin) return <Navigate to="/" replace />
  return <>{children}</>
}

/** For /login and /register: signed-in users go to the app. */
export function PublicOnly({ children }: { children: ReactNode }) {
  const { token, user } = useAuth()
  const location = useLocation()
  const from = (location.state as { from?: { pathname: string; search?: string } } | null)?.from
  if (token && user) return <Navigate to={from ? `${from.pathname}${from.search ?? ''}` : '/'} replace />
  return <>{children}</>
}
