import { http, type QueryParams } from './client'
import type { NoteDto, NoteListItemDto, NoteListQuery, PagedResult, SaveNoteRequest } from './types'

export const notesApi = {
  list: (q: NoteListQuery, signal?: AbortSignal) =>
    http.get<PagedResult<NoteListItemDto>>('/api/notes', q as QueryParams, signal),
  get: (id: string, signal?: AbortSignal) =>
    http.get<NoteDto>(`/api/notes/${id}`, undefined, signal),
  create: (body: SaveNoteRequest) => http.post<NoteDto>('/api/notes', body),
  update: (id: string, body: SaveNoteRequest) => http.put<NoteDto>(`/api/notes/${id}`, body),
  pin: (id: string, value: boolean) => http.patch<NoteDto>(`/api/notes/${id}/pin`, { value }),
  archive: (id: string, value: boolean) =>
    http.patch<NoteDto>(`/api/notes/${id}/archive`, { value }),
  remove: (id: string) => http.del(`/api/notes/${id}`),
  restore: (id: string) => http.post<NoteDto>(`/api/notes/${id}/restore`),
  removePermanent: (id: string) => http.del(`/api/notes/${id}/permanent`),
}
