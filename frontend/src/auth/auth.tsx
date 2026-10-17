import { createContext, useContext, useMemo, useState, type ReactNode } from 'react'
import * as pmo from '../api/pmo'
import type { LoginResult } from '../api/types'

// Role keys mirror the backend seed (sys_role.role_key carried in the JWT as RoleKeys).
export const ROLE = { admin: 'pmo_admin', pm: 'pmo_pm', contributor: 'pmo_contributor' } as const

interface Session { token: string; userName: string; nickName: string; roles: string[] }

interface AuthCtx {
  session: Session | null
  isAuthed: boolean
  signIn: (u: string, p: string, code?: string, uuid?: string) => Promise<void>
  signOut: () => void
  hasRole: (min: 'admin' | 'pm' | 'contributor') => boolean
}

const Ctx = createContext<AuthCtx>(null as unknown as AuthCtx)

function load(): Session | null {
  const token = localStorage.getItem('pmo.token')
  const raw = localStorage.getItem('pmo.session')
  if (!token || !raw) return null
  try { return { token, ...JSON.parse(raw) } } catch { return null }
}

// Numeric rank so the UI can mirror the server's role floor (admin 3 ≥ pm 2 ≥ contributor 1).
function rank(roles: string[]): number {
  if (roles.includes(ROLE.admin) || roles.includes('admin')) return 3
  if (roles.includes(ROLE.pm)) return 2
  if (roles.includes(ROLE.contributor)) return 1
  return 0
}
const MIN_RANK = { admin: 3, pm: 2, contributor: 1 } as const

export function AuthProvider({ children }: { children: ReactNode }) {
  const [session, setSession] = useState<Session | null>(load)

  const value = useMemo<AuthCtx>(() => ({
    session,
    isAuthed: !!session,
    async signIn(username, password, code, uuid) {
      const res: LoginResult = await pmo.login(username, password, code, uuid)
      const s: Session = {
        token: res.token,
        userName: res.user.userName,
        nickName: res.user.nickName || res.user.userName,
        roles: res.roles || [],
      }
      localStorage.setItem('pmo.token', s.token)
      localStorage.setItem('pmo.session', JSON.stringify({ userName: s.userName, nickName: s.nickName, roles: s.roles }))
      setSession(s)
    },
    signOut() {
      localStorage.removeItem('pmo.token')
      localStorage.removeItem('pmo.session')
      setSession(null)
    },
    hasRole(min) {
      return rank(session?.roles || []) >= MIN_RANK[min]
    },
  }), [session])

  return <Ctx.Provider value={value}>{children}</Ctx.Provider>
}

export const useAuth = () => useContext(Ctx)
