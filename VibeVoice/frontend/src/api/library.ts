import { http, upload, type QueryParams } from './client'
import type {
  AudioListQuery,
  AudioWithOwnerDto,
  ImageDto,
  ImageListQuery,
  ImageWithOwnerDto,
  PagedResult,
} from './types'

export const imagesApi = {
  list: (q: ImageListQuery, signal?: AbortSignal) =>
    http.get<PagedResult<ImageWithOwnerDto>>('/api/images', q as QueryParams, signal),
  upload: (files: File[], onProgress?: (fraction: number) => void) => {
    const form = new FormData()
    for (const f of files) form.append('files', f, f.name)
    return upload<ImageDto[]>('/api/images', form, { onProgress })
  },
  attach: (id: string, ownerType: 'Note' | 'Memory', ownerId: string) =>
    http.post<ImageDto>(`/api/images/${id}/attach`, { ownerType, ownerId }),
  remove: (id: string) => http.del(`/api/images/${id}`),
}

export const audioApi = {
  list: (q: AudioListQuery, signal?: AbortSignal) =>
    http.get<PagedResult<AudioWithOwnerDto>>('/api/audio', q as QueryParams, signal),
}
