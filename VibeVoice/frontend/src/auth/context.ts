import { createContext, useContext } from 'react'
import { PERMISSIONS, type AuthResponse, type Permission, type UserDto } from '@/api/types'

export interface AuthContextValue {
  user: UserDto | null
  token: string | null
  /** True while the stored token is being validated on startup. */
  isLoading: boolean
  /** Effective permissions of the signed-in user (from /api/auth/me). */
  permissions: readonly Permission[]
  /** True when the user has ANY of the given permissions. */
  can: (permission: Permission | readonly Permission[]) => boolean
  /** Error while loading the current user (when there's no cached user to fall back on). */
  loadError: unknown
  reload: () => void
  signIn: (res: AuthResponse) => void
  signOut: (reason?: 'expired') => void
  setUser: (user: UserDto) => void
}

export const AuthContext = createContext<AuthContextValue | null>(null)

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used inside <AuthProvider>')
  return ctx
}

const ADMIN_ONLY: readonly Permission[] = ['users.view', 'users.manage', 'roles.manage']

/**
 * Permissions of a user. Older API builds (pre-v5) didn't send `permissions` — fall back to the
 * built-in role semantics so the UI keeps working until the API is updated.
 */
export function permissionsOf(user: UserDto | null | undefined): Permission[] {
  if (!user) return []
  if (Array.isArray(user.permissions)) return user.permissions
  return user.role === 'Admin' ? [...PERMISSIONS] : PERMISSIONS.filter((p) => !ADMIN_ONLY.includes(p))
}

export function hasAny(granted: readonly Permission[], wanted: Permission | readonly Permission[]): boolean {
  const list: readonly Permission[] = typeof wanted === 'string' ? [wanted] : wanted
  return list.length === 0 || list.some((p) => granted.includes(p))
}

/**
 * `useCan('notes')` / `useCan(['voice.tts', 'voice.ocr'])` → true when the user has any of them.
 * Outside an AuthProvider (isolated component tests) everything is allowed; the API still enforces.
 */
export function useCan(permission: Permission | readonly Permission[]): boolean {
  const ctx = useContext(AuthContext)
  if (!ctx) return true
  return ctx.can(permission)
}
