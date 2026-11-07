import type { ComponentType } from 'react'
import { createBrowserRouter, type RouteObject } from 'react-router'
import { PublicOnly, RequireAdmin, RequireAuth } from './auth/guards'
import { AppLayout } from './components/layout/AppLayout'
import { RouteError } from './components/RouteError'
import { LoginPage, RegisterPage } from './pages/AuthPages'
import { NotFoundPage } from './pages/NotFoundPage'

/** Code-split page modules (each exports a named page component). */
const lazyPage =
  <K extends string>(load: () => Promise<Record<K, ComponentType>>, name: K): RouteObject['lazy'] =>
  async () => ({ Component: (await load())[name] })

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
      { path: 'memories', lazy: lazyPage(() => import('./pages/MemoriesPage'), 'MemoriesPage') },
      { path: 'memories/:id', lazy: lazyPage(() => import('./pages/MemoryEditorPage'), 'MemoryEditorPage') },
      { path: 'notes', lazy: lazyPage(() => import('./pages/NotesPage'), 'NotesPage') },
      { path: 'notes/:id', lazy: lazyPage(() => import('./pages/NoteEditorPage'), 'NoteEditorPage') },
      { path: 'audio', lazy: lazyPage(() => import('./pages/AudioPage'), 'AudioPage') },
      { path: 'studio', lazy: lazyPage(() => import('./pages/StudioPage'), 'StudioPage') },
      { path: 'images', lazy: lazyPage(() => import('./pages/ImagesPage'), 'ImagesPage') },
      { path: 'search', lazy: lazyPage(() => import('./pages/SearchPage'), 'SearchPage') },
      { path: 'trash', lazy: lazyPage(() => import('./pages/TrashPage'), 'TrashPage') },
      { path: 'settings', lazy: lazyPage(() => import('./pages/SettingsPage'), 'SettingsPage') },
      {
        path: 'users',
        lazy: async () => {
          const { UsersPage } = await import('./pages/UsersPage')
          return {
            Component: () => (
              <RequireAdmin>
                <UsersPage />
              </RequireAdmin>
            ),
          }
        },
      },
      { path: '*', element: <NotFoundPage /> },
    ],
  },
]

export const router = createBrowserRouter(routes)
