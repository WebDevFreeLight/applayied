import { http, type QueryParams } from './client'
import type {
  MemoryDto,
  MemoryListItemDto,
  MemoryListQuery,
  PagedResult,
  SaveMemoryRequest,
} from './types'

export const memoriesApi = {
  list: (q: MemoryListQuery, signal?: AbortSignal) =>
    http.get<PagedResult<MemoryListItemDto>>('/api/memories', q as QueryParams, signal),
  get: (id: string, signal?: AbortSignal) =>
    http.get<MemoryDto>(`/api/memories/${id}`, undefined, signal),
  create: (body: SaveMemoryRequest) => http.post<MemoryDto>('/api/memories', body),
  update: (id: string, body: SaveMemoryRequest) =>
    http.put<MemoryDto>(`/api/memories/${id}`, body),
  pin: (id: string, value: boolean) =>
    http.patch<MemoryDto>(`/api/memories/${id}/pin`, { value }),
  archive: (id: string, value: boolean) =>
    http.patch<MemoryDto>(`/api/memories/${id}/archive`, { value }),
  remove: (id: string) => http.del(`/api/memories/${id}`),
  restore: (id: string) => http.post<MemoryDto>(`/api/memories/${id}/restore`),
  removePermanent: (id: string) => http.del(`/api/memories/${id}/permanent`),
  setCover: (id: string, imageId: string | null) =>
    http.put<MemoryDto>(`/api/memories/${id}/cover`, { imageId }),
}
