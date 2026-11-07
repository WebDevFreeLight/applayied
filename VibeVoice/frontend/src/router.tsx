import type { ComponentType } from 'react'
import { createBrowserRouter, type RouteObject } from 'react-router'
import type { Permission } from './api/types'
import { PublicOnly, RequireAuth, RequirePermission } from './auth/guards'
import { AppLayout } from './components/layout/AppLayout'
import { RouteError } from './components/RouteError'
import { LoginPage, RegisterPage } from './pages/AuthPages'
import { NotFoundPage } from './pages/NotFoundPage'

/** Code-split page modules (each exports a named page component). */
const lazyPage =
  <K extends string>(
    load: () => Promise<Record<K, ComponentType>>,
    name: K,
    permission?: readonly Permission[],
  ): RouteObject['lazy'] =>
  async () => {
    const Page: ComponentType = (await load())[name]
    if (!permission) return { Component: Page }
    // Guarded route → friendly "no access" page when the role lacks the permission.
    return {
      Component: () => (
        <RequirePermission any={permission}>
          <Page />
        </RequirePermission>
      ),
    }
  }

export const routes: RouteObject[] = [
  {
    path: '/login',
    element: (
      <PublicOnly>
        <LoginPage />
      </PublicOnly>
    ),
  },
  {
    path: '/register',
    element: (
      <PublicOnly>
        <RegisterPage />
      </PublicOnly>
    ),
  },
  {
    path: '/',
    element: (
      <RequireAuth>
        <AppLayout />
      </RequireAuth>
    ),
    errorElement: <RouteError />,
    children: [
      { index: true, lazy: lazyPage(() => import('./pages/DashboardPage'), 'DashboardPage') },
      { path: 'memories', lazy: lazyPage(() => import('./pages/MemoriesPage'), 'MemoriesPage', ['memories']) },
      { path: 'memories/:id', lazy: lazyPage(() => import('./pages/MemoryEditorPage'), 'MemoryEditorPage', ['memories']) },
      { path: 'notes', lazy: lazyPage(() => import('./pages/NotesPage'), 'NotesPage', ['notes']) },
      { path: 'notes/:id', lazy: lazyPage(() => import('./pages/NoteEditorPage'), 'NoteEditorPage', ['notes']) },
      { path: 'audio', lazy: lazyPage(() => import('./pages/AudioPage'), 'AudioPage') },
      {
        path: 'studio',
        lazy: lazyPage(() => import('./pages/StudioPage'), 'StudioPage', ['voice.tts', 'voice.transcribe', 'voice.ocr']),
      },
      { path: 'images', lazy: lazyPage(() => import('./pages/ImagesPage'), 'ImagesPage') },
      { path: 'search', lazy: lazyPage(() => import('./pages/SearchPage'), 'SearchPage') },
      { path: 'trash', lazy: lazyPage(() => import('./pages/TrashPage'), 'TrashPage', ['notes', 'memories']) },
      { path: 'settings', lazy: lazyPage(() => import('./pages/SettingsPage'), 'SettingsPage') },
      { path: 'admin', lazy: lazyPage(() => import('./pages/admin/OverviewPage'), 'OverviewPage', ['users.view']) },
      { path: 'users', lazy: lazyPage(() => import('./pages/admin/UsersPage'), 'UsersPage', ['users.view']) },
      { path: 'roles', lazy: lazyPage(() => import('./pages/admin/RolesPage'), 'RolesPage', ['users.view']) },
      { path: '*', element: <NotFoundPage /> },
    ],
  },
]

export const router = createBrowserRouter(routes)
