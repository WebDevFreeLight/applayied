import { http, upload } from './client'
import type { AudioDto, ImageDto, TranscribeRequest, TtsRequest } from './types'

/** Notes and memories share the same attachment endpoint shape. */
export type OwnerKind = 'notes' | 'memories'

const base = (kind: OwnerKind, id: string) => `/api/${kind}/${id}`

export const attachmentsApi = {
  uploadImages: (
    kind: OwnerKind,
    id: string,
    files: File[],
    onProgress?: (fraction: number) => void,
  ) => {
    const form = new FormData()
    for (const f of files) form.append('files', f, f.name)
    return upload<ImageDto[]>(`${base(kind, id)}/images`, form, { onProgress })
  },
  updateCaption: (kind: OwnerKind, id: string, imageId: string, caption: string | null) =>
    http.patch<ImageDto>(`${base(kind, id)}/images/${imageId}`, { caption }),
  deleteImage: (kind: OwnerKind, id: string, imageId: string) =>
    http.del(`${base(kind, id)}/images/${imageId}`),
  reorderImages: (kind: OwnerKind, id: string, imageIds: string[]) =>
    http.put<ImageDto[]>(`${base(kind, id)}/images/order`, { imageIds }),

  uploadAudio: (
    kind: OwnerKind,
    id: string,
    file: File,
    source: 'Recorded' | 'Uploaded',
    durationSeconds?: number | null,
    onProgress?: (fraction: number) => void,
  ) => {
    const form = new FormData()
    form.append('file', file, file.name)
    form.append('source', source)
    if (durationSeconds != null && Number.isFinite(durationSeconds)) {
      form.append('durationSeconds', String(Math.round(durationSeconds * 100) / 100))
    }
    return upload<AudioDto>(`${base(kind, id)}/audio`, form, { onProgress })
  },
  deleteAudio: (kind: OwnerKind, id: string, audioId: string) =>
    http.del(`${base(kind, id)}/audio/${audioId}`),
  tts: (kind: OwnerKind, id: string, body: TtsRequest) =>
    http.post<AudioDto>(`${base(kind, id)}/tts`, body),
  transcribe: (kind: OwnerKind, id: string, audioId: string, body: TranscribeRequest) =>
    http.post<AudioDto>(`${base(kind, id)}/audio/${audioId}/transcribe`, body),
}

export function ownerKindOf(ownerType: string): OwnerKind | null {
  if (ownerType === 'Note') return 'notes'
  if (ownerType === 'Memory') return 'memories'
  return null
}
