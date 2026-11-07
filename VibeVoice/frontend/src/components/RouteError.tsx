import { isRouteErrorResponse, useRouteError } from 'react-router'
import { NotFoundPage } from '@/pages/NotFoundPage'
import { ErrorFallback } from './ui/ErrorBoundary'

export function RouteError() {
  const error = useRouteError()
  if (isRouteErrorResponse(error) && error.status === 404) return <NotFoundPage />
  const err = error instanceof Error ? error : new Error(String((error as { statusText?: string })?.statusText ?? error))
  return (
    <div className="p-6">
      <ErrorFallback error={err} onReset={() => window.location.reload()} />
    </div>
  )
}
