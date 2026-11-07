import { MutationCache, QueryCache, QueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import { ApiError, errorMessage, isForbidden, setForbiddenMessage } from './api/client'
import i18n from './i18n'

setForbiddenMessage(() => i18n.t('errors.forbidden'))

/** A 403 anywhere → one de-duplicated "You don't have permission" toast. */
export function notifyForbidden(err: unknown): void {
  if (isForbidden(err)) toast.error(errorMessage(err), { id: 'forbidden' })
}

export function createQueryClient(): QueryClient {
  return new QueryClient({
    queryCache: new QueryCache({ onError: notifyForbidden }),
    mutationCache: new MutationCache({ onError: notifyForbidden }),
    defaultOptions: {
      queries: {
        staleTime: 30_000,
        refetchOnWindowFocus: false,
        // Don't retry client errors (401/403/404/400); retry network / 5xx once.
        retry: (count, err) => !(err instanceof ApiError && err.status >= 400 && err.status < 500) && count < 1,
      },
    },
  })
}

export const queryClient = createQueryClient()
