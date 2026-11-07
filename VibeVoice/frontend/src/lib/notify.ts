import { toast } from 'sonner'
import { errorMessage, isForbidden } from '@/api/client'

/** Error toast; 403s collapse into a single "You don't have permission" toast (shared id). */
export function toastError(err: unknown, fallback?: string): void {
  toast.error(errorMessage(err, fallback), isForbidden(err) ? { id: 'forbidden' } : undefined)
}
