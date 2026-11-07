import type {
  AudioListQuery,
  ImageListQuery,
  MemoryListQuery,
  NoteListQuery,
  SearchQuery,
  UserListQuery,
} from './types'

/** Query-key factory: one place for every TanStack Query key. */
export const qk = {
  me: ['me'] as const,
  users: {
    all: ['users'] as const,
    list: (q: UserListQuery) => ['users', 'list', q] as const,
  },
  roles: ['roles'] as const,
  permissions: ['permissions'] as const,
  adminOverview: ['admin', 'overview'] as const,
  dashboard: ['dashboard'] as const,
  health: ['health'] as const,
  tags: ['tags'] as const,
  people: ['people'] as const,
  notes: {
    all: ['notes'] as const,
    lists: () => ['notes', 'list'] as const,
    list: (q: NoteListQuery) => ['notes', 'list', q] as const,
    details: () => ['notes', 'detail'] as const,
    detail: (id: string) => ['notes', 'detail', id] as const,
  },
  memories: {
    all: ['memories'] as const,
    lists: () => ['memories', 'list'] as const,
    list: (q: MemoryListQuery) => ['memories', 'list', q] as const,
    details: () => ['memories', 'detail'] as const,
    detail: (id: string) => ['memories', 'detail', id] as const,
  },
  images: {
    all: ['images'] as const,
    list: (q: ImageListQuery) => ['images', q] as const,
  },
  audio: {
    all: ['audio'] as const,
    list: (q: AudioListQuery) => ['audio', q] as const,
  },
  voice: {
    all: ['voice'] as const,
    status: () => ['voice', 'status'] as const,
    voices: () => ['voice', 'voices'] as const,
    histories: () => ['voice', 'history'] as const,
    history: (page: number) => ['voice', 'history', page] as const,
  },
  jobs: {
    all: ['jobs'] as const,
    list: (scope: 'mine' | 'all', q: object) => ['jobs', 'list', scope, q] as const,
    summary: (scope: 'mine' | 'all') => ['jobs', 'summary', scope] as const,
    detail: (scope: 'mine' | 'all', id: string) => ['jobs', 'detail', scope, id] as const,
  },
  search: {
    all: ['search'] as const,
    query: (q: SearchQuery) => ['search', q] as const,
  },
}
