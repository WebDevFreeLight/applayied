import { act, fireEvent, screen, waitFor, within } from '@testing-library/react'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { permissionsApi, rolesApi } from '@/api/auth'
import { ApiError } from '@/api/client'
import { PERMISSIONS } from '@/api/types'
import { PERMISSION_CATALOG } from '@/lib/admin'
import { ADMIN_ID, makeRole, makeUser, renderWithAuth, USER_ROLE } from '@/test/adminFixtures'
import { RolesPage } from './RolesPage'

const roles = [
  makeRole({ id: ADMIN_ID, name: 'Admin', isSystem: true, userCount: 1, permissions: [...PERMISSIONS], description: 'Full access' }),
  makeRole({ id: USER_ROLE, name: 'User', isSystem: true, userCount: 0, permissions: ['notes', 'memories', 'voice.tts'] }),
  makeRole({ id: 'r-editors', name: 'Editors', userCount: 2, permissions: ['notes', 'memories'] }),
  makeRole({ id: 'r-empty', name: 'Guests', userCount: 0, permissions: [] }),
]

beforeEach(() => {
  vi.spyOn(rolesApi, 'list').mockResolvedValue(roles)
  vi.spyOn(permissionsApi, 'list').mockResolvedValue(PERMISSION_CATALOG)
})

async function renderPage(user = makeUser()) {
  const r = renderWithAuth(RolesPage, user, '/roles')
  await screen.findByTestId('role-Editors')
  return r
}

describe('Roles page', () => {
  it('shows each role with built-in badge, user count, permission chips and a users link', async () => {
    await renderPage()
    const admin = screen.getByTestId('role-Admin')
    expect(within(admin).getByText('Built-in')).toBeInTheDocument()
    expect(within(admin).getByText('Manage roles')).toBeInTheDocument()
    const editors = screen.getByTestId('role-Editors')
    expect(within(editors).getByRole('link', { name: /2 users · View users with this role/ })).toHaveAttribute(
      'href',
      '/users?roleId=r-editors',
    )
    expect(within(editors).getByText('Notes')).toBeInTheDocument()
  })

  it('disables delete for built-in roles and roles still in use, with the reason', async () => {
    await renderPage()
    const del = (name: string) => within(screen.getByTestId(`role-${name}`)).getByRole('button', { name: 'Delete' })
    expect(del('Admin')).toBeDisabled()
    expect(del('User')).toBeDisabled()
    expect(within(screen.getByTestId('role-User')).getByText("Built-in roles can't be deleted.")).toBeInTheDocument()
    expect(del('Editors')).toBeDisabled()
    expect(within(screen.getByTestId('role-Editors')).getByText("Can't delete: 2 users still have this role.")).toBeInTheDocument()
    expect(del('Guests')).toBeEnabled()
  })

  it('deleting an unused role calls the API; a 409 message is shown', async () => {
    const { toast } = await import('sonner')
    const error = vi.spyOn(toast, 'error').mockImplementation(() => 1)
    const remove = vi
      .spyOn(rolesApi, 'remove')
      .mockRejectedValue(new ApiError(409, { title: 'Conflict', detail: 'Users still have this role.' }))
    await renderPage()
    fireEvent.click(within(screen.getByTestId('role-Guests')).getByRole('button', { name: 'Delete' }))
    const dialog = await screen.findByRole('dialog', { name: 'Delete role Guests?' })
    await act(async () => {
      fireEvent.click(within(dialog).getByRole('button', { name: 'Delete' }))
    })
    await waitFor(() => expect(remove).toHaveBeenCalledWith('r-empty'))
    await waitFor(() => expect(error).toHaveBeenCalledWith('Users still have this role.'))
  })

  it('the Admin role opens read-only with every permission checked', async () => {
    await renderPage()
    fireEvent.click(within(screen.getByTestId('role-Admin')).getByRole('button', { name: 'Open' }))
    const dialog = await screen.findByRole('dialog', { name: 'Role: Admin' })
    expect(within(dialog).getByText(/always has every permission/)).toBeInTheDocument()
    const boxes = within(dialog).getAllByRole('checkbox')
    expect(boxes).toHaveLength(10)
    for (const b of boxes) {
      expect(b).toBeChecked()
      expect(b).toBeDisabled()
    }
    expect(within(dialog).queryByRole('button', { name: 'Save' })).not.toBeInTheDocument()
  })

  it('the built-in User role can be edited but not renamed', async () => {
    await renderPage()
    fireEvent.click(within(screen.getByTestId('role-User')).getByRole('button', { name: 'Edit' }))
    const dialog = await screen.findByRole('dialog', { name: 'Edit role: User' })
    expect(within(dialog).getByLabelText('Role name')).toBeDisabled()
    expect(within(dialog).getByText(/can't be renamed or deleted/)).toBeInTheDocument()
  })

  it('create dialog groups permissions with explanations and "select all"', async () => {
    const create = vi.spyOn(rolesApi, 'create').mockResolvedValue(makeRole({ id: 'new', name: 'Narrators' }))
    await renderPage()
    fireEvent.click(screen.getByRole('button', { name: 'New role' }))
    const dialog = await screen.findByRole('dialog', { name: 'Create role' })
    for (const group of ['Content', 'Media', 'AI', 'Administration']) {
      expect(within(dialog).getByRole('group', { name: group })).toBeInTheDocument()
    }
    expect(within(dialog).getByText('Extract Arabic and English text from pictures.')).toBeInTheDocument()

    // Name is required.
    await act(async () => {
      fireEvent.click(within(dialog).getByRole('button', { name: 'New role' }))
    })
    expect(within(dialog).getByText('Enter a role name')).toBeInTheDocument()
    expect(create).not.toHaveBeenCalled()

    fireEvent.change(within(dialog).getByLabelText('Role name'), { target: { value: 'Narrators' } })
    const ai = within(dialog).getByRole('group', { name: 'AI' })
    fireEvent.click(within(ai).getByRole('checkbox', { name: 'Select all' }))
    fireEvent.click(within(dialog).getByRole('checkbox', { name: /^Notes/ }))
    await act(async () => {
      fireEvent.click(within(dialog).getByRole('button', { name: 'New role' }))
    })
    await waitFor(() => expect(create).toHaveBeenCalledTimes(1))
    const body = create.mock.calls[0][0]
    expect(body.name).toBe('Narrators')
    expect([...body.permissions].sort()).toEqual(['notes', 'voice.ocr', 'voice.transcribe', 'voice.tts'])
  })

  it('without roles.manage the page is read-only', async () => {
    await renderPage(makeUser({ permissions: ['users.view'] }))
    expect(screen.queryByRole('button', { name: 'New role' })).not.toBeInTheDocument()
    expect(screen.queryByRole('button', { name: 'Delete' })).not.toBeInTheDocument()
  })
})
