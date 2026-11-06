import { createContext, useContext } from 'react'
import type { AuthResponse, UserDto } from '@/api/types'

export interface AuthContextValue {
  user: UserDto | null
  token: string | null
  /** True while the stored token is being validated on startup. */
  isLoading: boolean
  isAdmin: boolean
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
