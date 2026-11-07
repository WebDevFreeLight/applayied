import { act, fireEvent, screen, waitFor, within } from '@testing-library/react'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { rolesApi, usersApi } from '@/api/auth'
import { ApiError } from '@/api/client'
import type { AdminUserDto } from '@/api/types'
import { ADMIN_ID, makeRole, makeUser, renderWithAuth, USER_ROLE } from '@/test/adminFixtures'
import { UsersPage } from './UsersPage'

const me = makeUser()
const other = makeUser({
  id: 'u2',
  email: 'sara@example.com',
  displayName: 'Sara',
  role: 'User',
  roleId: USER_ROLE,
  permissions: ['notes', 'memories'],
  lastLoginAt: null,
})
const row = (user = me, over: Partial<AdminUserDto> = {}): AdminUserDto => ({
  user,
  notes: 3,
  memories: 2,
  mediaFiles: 5,
  storageBytes: 2_500_000,
  ...over,
})

beforeEach(() => {
  vi.spyOn(usersApi, 'list').mockResolvedValue({ items: [row(me), row(other)], page: 1, pageSize: 20, total: 2 })
  vi.spyOn(rolesApi, 'list').mockResolvedValue([
    makeRole({ id: ADMIN_ID, name: 'Admin', isSystem: true, userCount: 1 }),
    makeRole({ id: USER_ROLE, name: 'User', isSystem: true, userCount: 1 }),
  ])
})

async function renderPage(user = me, path = '/users') {
  const r = renderWithAuth(UsersPage, user, path)
  await screen.findAllByText('Sara')
  return r
}

describe('Users page', () => {
  it('lists users with role, status, last login, counts and storage', async () => {
    await renderPage()
    expect(screen.getByText('2 users')).toBeInTheDocument()
    expect(screen.getAllByText('sara@example.com').length).toBeGreaterThan(0)
    expect(screen.getAllByText('Never').length).toBeGreaterThan(0)
    expect(screen.getAllByText('3 notes · 2 memories · 5 media').length).toBeGreaterThan(0)
    expect(screen.getAllByText('2.4 MB').length).toBeGreaterThan(0)
  })

  it('passes search / role filters to the API (roleId from the URL)', async () => {
    await renderPage(me, `/users?roleId=${USER_ROLE}`)
    expect(usersApi.list).toHaveBeenCalledWith(expect.objectContaining({ roleId: USER_ROLE, page: 1 }), expect.anything())
  })

  it('disables self-actions (role, deactivate, delete) with explanations', async () => {
    await renderPage()
    for (const b of screen.getAllByRole('button', { name: 'Deactivate — Demo Admin' })) expect(b).toBeDisabled()
    for (const b of screen.getAllByRole('button', { name: 'Delete user — Demo Admin' })) expect(b).toBeDisabled()
    for (const s of screen.getAllByRole('combobox', { name: 'Role — Demo Admin' })) expect(s).toBeDisabled()
    expect(screen.getAllByTitle("You can't delete your own account").length).toBeGreaterThan(0)
    // Other users stay manageable.
    for (const b of screen.getAllByRole('button', { name: 'Delete user — Sara' })) expect(b).toBeEnabled()
  })

  it('hides management actions without users.manage', async () => {
    await renderPage(makeUser({ permissions: ['users.view'] }))
    expect(screen.queryByRole('button', { name: 'New user' })).not.toBeInTheDocument()
    expect(screen.queryByRole('button', { name: /Delete user/ })).not.toBeInTheDocument()
    expect(screen.queryByRole('combobox', { name: /Role — / })).not.toBeInTheDocument()
  })

  it('create dialog validates, then creates the user', async () => {
    const create = vi.spyOn(usersApi, 'create').mockResolvedValue(other)
    await renderPage()
    fireEvent.click(screen.getByRole('button', { name: 'New user' }))
    const dialog = await screen.findByRole('dialog', { name: 'Create user' })

    await act(async () => {
      fireEvent.click(within(dialog).getByRole('button', { name: 'New user' }))
    })
    expect(await within(dialog).findByText('Enter a valid email address')).toBeInTheDocument()
    expect(within(dialog).getByText('Enter your name')).toBeInTheDocument()
    expect(within(dialog).getByText('At least 8 characters with a letter and a digit')).toBeInTheDocument()
    expect(create).not.toHaveBeenCalled()

    fireEvent.change(within(dialog).getByLabelText('Display name'), { target: { value: 'Omar' } })
    fireEvent.change(within(dialog).getByLabelText('Email'), { target: { value: 'omar@example.com' } })
    fireEvent.click(within(dialog).getByRole('button', { name: 'Generate' }))
    const pw = (within(dialog).getByLabelText('Password') as HTMLInputElement).value
    expect(pw).toMatch(/^(?=.*\p{L})(?=.*\d).{14}$/u)
    expect(within(dialog).getByText(/Strength:/)).toBeInTheDocument()
    fireEvent.change(within(dialog).getByLabelText('Role'), { target: { value: USER_ROLE } })

    await act(async () => {
      fireEvent.click(within(dialog).getByRole('button', { name: 'New user' }))
    })
    await waitFor(() => expect(create).toHaveBeenCalledTimes(1))
    expect(create).toHaveBeenCalledWith({
      email: 'omar@example.com',
      displayName: 'Omar',
      password: pw,
      roleId: USER_ROLE,
      preferredLanguage: 'en',
      isActive: true,
    })
  })

  it('shows server 400 messages in the dialog', async () => {
    vi.spyOn(usersApi, 'create').mockRejectedValue(new ApiError(400, { title: 'Bad Request', detail: 'That email is already registered.' }))
    await renderPage()
    fireEvent.click(screen.getByRole('button', { name: 'New user' }))
    const dialog = await screen.findByRole('dialog', { name: 'Create user' })
    fireEvent.change(within(dialog).getByLabelText('Display name'), { target: { value: 'Dup' } })
    fireEvent.change(within(dialog).getByLabelText('Email'), { target: { value: 'sara@example.com' } })
    fireEvent.change(within(dialog).getByLabelText('Password'), { target: { value: 'abcdefg1' } })
    await act(async () => {
      fireEvent.click(within(dialog).getByRole('button', { name: 'New user' }))
    })
    expect(await within(dialog).findByRole('alert')).toHaveTextContent('That email is already registered.')
  })

  it('delete requires typing the exact email and explains everything is deleted', async () => {
    const remove = vi.spyOn(usersApi, 'remove').mockResolvedValue(undefined)
    await renderPage()
    fireEvent.click(screen.getAllByRole('button', { name: 'Delete user — Sara' })[0])
    const dialog = await screen.findByRole('dialog', { name: 'Delete Sara?' })
    expect(dialog).toHaveTextContent(/ALL of its notes, memories, audio, video and images/)
    const confirm = within(dialog).getByRole('button', { name: 'Delete user' })
    expect(confirm).toBeDisabled()

    fireEvent.change(within(dialog).getByTestId('delete-confirm-input'), { target: { value: 'sara@example' } })
    expect(confirm).toBeDisabled()
    expect(within(dialog).getByText("The email doesn't match")).toBeInTheDocument()

    fireEvent.change(within(dialog).getByTestId('delete-confirm-input'), { target: { value: 'SARA@example.com' } })
    expect(confirm).toBeEnabled()
    await act(async () => {
      fireEvent.click(confirm)
    })
    await waitFor(() => expect(remove).toHaveBeenCalledWith('u2'))
  })

  it('reset password generates a valid password and calls the API', async () => {
    const reset = vi.spyOn(usersApi, 'resetPassword').mockResolvedValue(undefined)
    await renderPage()
    fireEvent.click(screen.getAllByRole('button', { name: 'Reset password — Sara' })[0])
    const dialog = await screen.findByRole('dialog', { name: 'Reset password for Sara' })
    fireEvent.click(within(dialog).getByRole('button', { name: 'Generate' }))
    const pw = (within(dialog).getByLabelText('New password') as HTMLInputElement).value
    await act(async () => {
      fireEvent.click(within(dialog).getByRole('button', { name: 'Reset password' }))
    })
    await waitFor(() => expect(reset).toHaveBeenCalledWith('u2', pw))
  })

  it('toggling active and changing role call the API', async () => {
    const setActive = vi.spyOn(usersApi, 'setActive').mockResolvedValue({ ...other, isActive: false })
    const setRole = vi.spyOn(usersApi, 'setRole').mockResolvedValue({ ...other, roleId: ADMIN_ID, role: 'Admin' })
    await renderPage()
    await act(async () => {
      fireEvent.click(screen.getAllByRole('button', { name: 'Deactivate — Sara' })[0])
    })
    await waitFor(() => expect(setActive).toHaveBeenCalledWith('u2', false))
    await act(async () => {
      fireEvent.change(screen.getAllByRole('combobox', { name: 'Role — Sara' })[0], { target: { value: ADMIN_ID } })
    })
    await waitFor(() => expect(setRole).toHaveBeenCalledWith('u2', ADMIN_ID))
  })
})
