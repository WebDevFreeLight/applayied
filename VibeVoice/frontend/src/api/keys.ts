import type {
  AudioListQuery,
  ImageListQuery,
  MemoryListQuery,
  NoteListQuery,
  SearchQuery,
} from './types'

/** Query-key factory: one place for every TanStack Query key. */
export const qk = {
  me: ['me'] as const,
  users: ['users'] as const,
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
  search: {
    all: ['search'] as const,
    query: (q: SearchQuery) => ['search', q] as const,
  },
}
