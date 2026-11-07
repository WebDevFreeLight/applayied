import { toast } from 'sonner'
import { errorMessage, isForbidden } from '@/api/client'

/** Error toast; 403s collapse into a single "You don't have permission" toast (shared id). */
export function toastError(err: unknown, fallback?: string): void {
  if (isForbidden(err)) toast.error(errorMessage(err, fallback), { id: 'forbidden' })
  else toast.error(errorMessage(err, fallback))
}
