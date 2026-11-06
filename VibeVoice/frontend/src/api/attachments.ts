import { http, upload, uploadWithResponse } from './client'
import type { AudioDto, ImageDto, TranscribeRequest, TtsRequest } from './types'

/** Notes and memories share the same attachment endpoint shape. */
export type OwnerKind = 'notes' | 'memories'

export interface UploadMediaOptions {
  source: 'Recorded' | 'Uploaded'
  durationSeconds?: number | null
  /** Transcribe right after upload (form field `transcribe=true`). */
  transcribe?: boolean
  /** e.g. "en" / "ar"; omit for auto-detect. */
  language?: string
  onProgress?: (fraction: number) => void
}

export interface UploadMediaResult {
  audio: AudioDto
  /** Set when auto-transcription failed (the clip itself was saved). */
  transcriptionError: string | null
}

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

  /**
   * Uploads an audio or video clip. With `transcribe` the API transcribes it right away; if that
   * fails the clip is still saved and `transcriptionError` carries the X-Transcription-Error header.
   */
  uploadAudio: async (
    kind: OwnerKind,
    id: string,
    file: File,
    options: UploadMediaOptions,
  ): Promise<UploadMediaResult> => {
    const { source, durationSeconds, transcribe, language, onProgress } = options
    const form = new FormData()
    form.append('file', file, file.name)
    form.append('source', source)
    if (durationSeconds != null && Number.isFinite(durationSeconds)) {
      form.append('durationSeconds', String(Math.round(durationSeconds * 100) / 100))
    }
    if (transcribe) form.append('transcribe', 'true')
    if (language) form.append('language', language)
    const res = await uploadWithResponse<AudioDto>(`${base(kind, id)}/audio`, form, { onProgress })
    return { audio: res.data, transcriptionError: res.header('X-Transcription-Error') }
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
