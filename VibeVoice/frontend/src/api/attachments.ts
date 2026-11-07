import { http, PREFER_ASYNC, runJob, uploadWithResponse, waitForJobIds } from './client'
import type { AudioDto, ImageDto, OcrRequest, TranscribeRequest, TtsRequest } from './types'

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

export interface UploadImagesOptions {
  /** Extract text from each picture after upload (form field `ocr=true`). */
  ocr?: boolean
  /** "ar" / "en"; omit for auto (Arabic + English). */
  language?: string
  onProgress?: (fraction: number) => void
}

export interface UploadImagesResult {
  images: ImageDto[]
  /** From the X-Ocr-Error header when text extraction failed (the images were still saved). */
  ocrError: string | null
}

/** Multipart body shared by note/memory/library image uploads. */
export function imagesForm(files: File[], options: UploadImagesOptions = {}): FormData {
  const form = new FormData()
  for (const f of files) form.append('files', f, f.name)
  if (options.ocr) {
    form.append('ocr', 'true')
    if (options.language) form.append('language', options.language)
  }
  return form
}

const base = (kind: OwnerKind, id: string) => `/api/${kind}/${id}`

export const attachmentsApi = {
  /** Uploads images; with `ocr` the API extracts text per image (failures → X-Ocr-Error header). */
  uploadImages: async (
    kind: OwnerKind,
    id: string,
    files: File[],
    options: UploadImagesOptions = {},
  ): Promise<UploadImagesResult> => {
    const res = await uploadWithResponse<ImageDto[]>(`${base(kind, id)}/images`, imagesForm(files, options), {
      onProgress: options.onProgress,
      headers: options.ocr ? PREFER_ASYNC : undefined,
    })
    return withOcrJobs(res.data ?? [], res.header('X-Job-Ids'), res.header('X-Ocr-Error'))
  },
  /** Runs as a background job on the API; resolves once the text is stored. */
  ocrImage: (kind: OwnerKind, id: string, imageId: string, body: OcrRequest = {}) =>
    runJob<ImageDto>(`${base(kind, id)}/images/${imageId}/ocr`, body),
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
    const res = await uploadWithResponse<AudioDto>(`${base(kind, id)}/audio`, form, {
      onProgress,
      headers: transcribe ? PREFER_ASYNC : undefined,
    })
    // The clip is saved; with a queued transcription job, wait for the transcript in the background.
    const jobs = await waitForJobIds<AudioDto>(res.header('X-Job-Ids'))
    return {
      audio: jobs.results[0] ?? res.data,
      transcriptionError: jobs.error ?? res.header('X-Transcription-Error'),
    }
  },
  deleteAudio: (kind: OwnerKind, id: string, audioId: string) =>
    http.del(`${base(kind, id)}/audio/${audioId}`),
  tts: (kind: OwnerKind, id: string, body: TtsRequest) =>
    http.post<AudioDto>(`${base(kind, id)}/tts`, body),
  /** Runs as a background job on the API; resolves once the transcript is stored. */
  transcribe: (kind: OwnerKind, id: string, audioId: string, body: TranscribeRequest) =>
    runJob<AudioDto>(`${base(kind, id)}/audio/${audioId}/transcribe`, body),
}

/**
 * Image uploads with OCR save the pictures first and read them in background jobs (X-Job-Ids):
 * waits for those jobs and swaps in the images with their text.
 */
export async function withOcrJobs(
  images: ImageDto[],
  jobIds: string | null,
  ocrError: string | null,
): Promise<UploadImagesResult> {
  const jobs = await waitForJobIds<ImageDto>(jobIds)
  const done = new Map(jobs.results.map((img) => [img.id, img]))
  return { images: images.map((img) => done.get(img.id) ?? img), ocrError: jobs.error ?? ocrError }
}

export function ownerKindOf(ownerType: string): OwnerKind | null {
  if (ownerType === 'Note') return 'notes'
  if (ownerType === 'Memory') return 'memories'
  return null
}
