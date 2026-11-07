import { useQuery, useQueryClient } from '@tanstack/react-query'
import { useCallback, useEffect, useMemo, useState, type ReactNode } from 'react'
import { useTranslation } from 'react-i18next'
import { toast } from 'sonner'
import { authApi } from '@/api/auth'
import { getToken, onUnauthorized, setToken } from '@/api/client'
import { qk } from '@/api/keys'
import type { AuthResponse, Permission, UserDto } from '@/api/types'
import { setLanguage } from '@/i18n'
import { isPalette, useTheme } from '@/theme/theme'
import { AuthContext, hasAny, permissionsOf, type AuthContextValue } from './context'

const USER_KEY = 'vv.user'

function readCachedUser(): UserDto | null {
  try {
    const raw = localStorage.getItem(USER_KEY)
    return raw ? (JSON.parse(raw) as UserDto) : null
  } catch {
    return null
  }
}

function cacheUser(user: UserDto | null) {
  try {
    if (user) localStorage.setItem(USER_KEY, JSON.stringify(user))
    else localStorage.removeItem(USER_KEY)
  } catch {
    /* ignore */
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const queryClient = useQueryClient()
  const { t } = useTranslation()
  const { setTheme, setPalette } = useTheme()
  const [token, setTokenState] = useState<string | null>(getToken)
  const [cachedUser] = useState<UserDto | null>(() => (getToken() ? readCachedUser() : null))

  const me = useQuery({
    queryKey: qk.me,
    queryFn: authApi.me,
    enabled: !!token,
    staleTime: 60_000,
    retry: false,
    // Pick up role / permission changes made by an admin without a re-login.
    refetchOnWindowFocus: 'always',
  })

  const user = token ? (me.data ?? cachedUser) : null
  const permissions = useMemo(() => permissionsOf(user), [user])

  const signOut = useCallback(
    (reason?: 'expired') => {
      setToken(null)
      cacheUser(null)
      setTokenState(null)
      queryClient.clear()
      if (reason === 'expired') toast.error(t('auth.sessionExpired'), { id: 'session-expired' })
    },
    [queryClient, t],
  )

  const setUser = useCallback(
    (u: UserDto) => {
      queryClient.setQueryData(qk.me, u)
      cacheUser(u)
    },
    [queryClient],
  )

  const signIn = useCallback(
    (res: AuthResponse) => {
      setToken(res.token)
      setTokenState(res.token)
      queryClient.clear()
      setUser(res.user)
    },
    [queryClient, setUser],
  )

  // Any authenticated request that returns 401 logs the user out (guards then redirect).
  useEffect(() => {
    onUnauthorized(() => signOut('expired'))
    return () => onUnauthorized(null)
  }, [signOut])

  useEffect(() => {
    if (me.data) cacheUser(me.data)
  }, [me.data])

  // Apply the user's saved language + theme whenever they change on the server.
  const prefLang = user?.preferredLanguage
  const prefTheme = user?.theme
  const prefPalette = user?.palette
  useEffect(() => {
    if (prefLang) void setLanguage(prefLang)
  }, [prefLang])
  useEffect(() => {
    if (prefTheme) setTheme(prefTheme)
  }, [prefTheme, setTheme])
  useEffect(() => {
    if (isPalette(prefPalette)) setPalette(prefPalette)
  }, [prefPalette, setPalette])

  const value = useMemo<AuthContextValue>(
    () => ({
      user,
      token,
      isLoading: !!token && !user && me.isPending,
      permissions,
      can: (p: Permission | readonly Permission[]) => hasAny(permissions, p),
      loadError: !user ? me.error : null,
      reload: () => void me.refetch(),
      signIn,
      signOut,
      setUser,
    }),
    [user, token, me, permissions, signIn, signOut, setUser],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
