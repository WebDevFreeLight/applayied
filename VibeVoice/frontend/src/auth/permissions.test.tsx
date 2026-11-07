import { act, render, renderHook, screen, waitFor } from '@testing-library/react'
import type { ReactNode } from 'react'
import { MemoryRouter } from 'react-router'
import { describe, expect, it, vi } from 'vitest'
import { ApiError, errorMessage } from '@/api/client'
import { PERMISSIONS, type UserDto } from '@/api/types'
import { setLanguage } from '@/i18n'
import { createQueryClient } from '@/queryClient'
import { authValue, makeUser } from '@/test/adminFixtures'
import { AuthContext, permissionsOf, useCan } from './context'
import { RequirePermission } from './guards'

function withUser(user: UserDto) {
  return ({ children }: { children: ReactNode }) => (
    <AuthContext.Provider value={authValue(user)}>{children}</AuthContext.Provider>
  )
}

describe('permissions', () => {
  it('useCan checks the signed-in user permissions (any of)', () => {
    const user = makeUser({ permissions: ['notes', 'voice.tts'] })
    const { result } = renderHook(
      () => ({
        notes: useCan('notes'),
        memories: useCan('memories'),
        anyVoice: useCan(['voice.ocr', 'voice.tts']),
        admin: useCan('users.view'),
      }),
      { wrapper: withUser(user) },
    )
    expect(result.current).toEqual({ notes: true, memories: false, anyVoice: true, admin: false })
  })

  it('falls back to built-in role semantics when an old API omits permissions', () => {
    const legacyAdmin = { ...makeUser(), permissions: undefined } as unknown as UserDto
    const legacyUser = { ...makeUser({ role: 'User' }), permissions: undefined } as unknown as UserDto
    expect(permissionsOf(legacyAdmin)).toEqual([...PERMISSIONS])
    expect(permissionsOf(legacyUser)).not.toContain('users.view')
    expect(permissionsOf(legacyUser)).toContain('notes')
    expect(permissionsOf(null)).toEqual([])
  })

  it('route guard shows the friendly "no access" page', () => {
    const { rerender } = render(
      <MemoryRouter>
      <AuthContext.Provider value={authValue(makeUser({ permissions: ['memories'] }))}>
        <RequirePermission any={['notes']}>
          <h1>Secret notes</h1>
        </RequirePermission>
      </AuthContext.Provider>
      </MemoryRouter>,
    )
    expect(screen.queryByText('Secret notes')).not.toBeInTheDocument()
    expect(screen.getByRole('heading', { name: "You don't have access" })).toBeInTheDocument()
    expect(document.querySelector('svg[aria-hidden="true"]')).not.toBeNull()

    rerender(
      <MemoryRouter>
      <AuthContext.Provider value={authValue(makeUser({ permissions: ['notes'] }))}>
        <RequirePermission any={['notes']}>
          <h1>Secret notes</h1>
        </RequirePermission>
      </AuthContext.Provider>
      </MemoryRouter>,
    )
    expect(screen.getByRole('heading', { name: 'Secret notes' })).toBeInTheDocument()
  })

  it('a 403 from the API shows one "You don\'t have permission" toast (translated)', async () => {
    const { toast } = await import('sonner')
    const error = vi.spyOn(toast, 'error').mockImplementation(() => 1)
    const qc = createQueryClient()
    const forbidden = new ApiError(403, { title: 'Forbidden', status: 403 })
    await qc.fetchQuery({ queryKey: ['x'], queryFn: () => Promise.reject(forbidden) }).catch(() => undefined)
    await waitFor(() => expect(error).toHaveBeenCalledWith("You don't have permission to do that.", { id: 'forbidden' }))

    // Mutations too; other errors don't trigger the global toast.
    error.mockClear()
    await qc.getMutationCache().build(qc, { mutationFn: () => Promise.reject(forbidden) }).execute(undefined).catch(() => undefined)
    expect(error).toHaveBeenCalledWith("You don't have permission to do that.", { id: 'forbidden' })
    error.mockClear()
    await qc.fetchQuery({ queryKey: ['y'], queryFn: () => Promise.reject(new ApiError(404, {})) }).catch(() => undefined)
    expect(error).not.toHaveBeenCalled()

    await act(async () => {
      await setLanguage('ar')
    })
    expect(errorMessage(forbidden)).toBe('ليست لديك صلاحية للقيام بذلك.')
    await act(async () => {
      await setLanguage('en')
    })
  })
})
