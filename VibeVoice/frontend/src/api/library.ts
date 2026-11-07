import { imagesForm, type UploadImagesOptions, type UploadImagesResult } from './attachments'
import { http, uploadWithResponse, type QueryParams } from './client'
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
  upload: async (files: File[], options: UploadImagesOptions = {}): Promise<UploadImagesResult> => {
    const res = await uploadWithResponse<ImageDto[]>('/api/images', imagesForm(files, options), {
      onProgress: options.onProgress,
    })
    return { images: res.data ?? [], ocrError: res.header('X-Ocr-Error') }
  },
  /** Extracts text from any of the user's images (incl. library). */
  ocr: (id: string, body: { language?: string } = {}) => http.post<ImageDto>(`/api/images/${id}/ocr`, body),
  attach: (id: string, ownerType: 'Note' | 'Memory', ownerId: string) =>
    http.post<ImageDto>(`/api/images/${id}/attach`, { ownerType, ownerId }),
  remove: (id: string) => http.del(`/api/images/${id}`),
}

export const audioApi = {
  list: (q: AudioListQuery, signal?: AbortSignal) =>
    http.get<PagedResult<AudioWithOwnerDto>>('/api/audio', q as QueryParams, signal),
}
