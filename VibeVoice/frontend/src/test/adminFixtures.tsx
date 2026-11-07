import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render } from '@testing-library/react'
import type { ComponentType } from 'react'
import { createMemoryRouter, RouterProvider } from 'react-router'
import { PERMISSIONS, type Permission, type RoleDto, type UserDto } from '@/api/types'
import { AuthContext, hasAny, type AuthContextValue } from '@/auth/context'

export const ADMIN_ID = '00000000-0000-0000-0000-00000000a001'
export const USER_ROLE = '00000000-0000-0000-0000-00000000a002'

export function makeUser(over: Partial<UserDto> = {}): UserDto {
  return {
    id: 'me',
    email: 'admin@vibevoice.local',
    displayName: 'Demo Admin',
    role: 'Admin',
    roleId: ADMIN_ID,
    permissions: [...PERMISSIONS],
    preferredLanguage: 'en',
    preferredVoice: null,
    theme: 'light',
    palette: 'sunset',
    isActive: true,
    createdAt: '2026-01-01T00:00:00Z',
    lastLoginAt: '2026-11-07T10:00:00Z',
    ...over,
  }
}

export function makeRole(over: Partial<RoleDto> = {}): RoleDto {
  return {
    id: 'r1',
    name: 'Role',
    description: null,
    permissions: [],
    isSystem: false,
    userCount: 0,
    createdAt: '2026-01-01T00:00:00Z',
    updatedAt: '2026-01-01T00:00:00Z',
    ...over,
  }
}

export function authValue(user: UserDto, extra: Partial<AuthContextValue> = {}): AuthContextValue {
  const permissions: Permission[] = user.permissions
  return {
    user,
    token: 't',
    isLoading: false,
    permissions,
    can: (p) => hasAny(permissions, p),
    loadError: null,
    reload: () => {},
    signIn: () => {},
    signOut: () => {},
    setUser: () => {},
    ...extra,
  }
}

/** Renders a page inside QueryClient + memory router + a fake auth context. */
export function renderWithAuth(Page: ComponentType, user: UserDto, path = '/', routePath = '*') {
  const qc = new QueryClient({ defaultOptions: { queries: { retry: false }, mutations: { retry: false } } })
  const router = createMemoryRouter([{ path: routePath, element: <Page /> }], { initialEntries: [path] })
  const utils = render(
    <QueryClientProvider client={qc}>
      <AuthContext.Provider value={authValue(user)}>
        <RouterProvider router={router} />
      </AuthContext.Provider>
    </QueryClientProvider>,
  )
  return { ...utils, router, qc }
}
