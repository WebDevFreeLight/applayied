import { http, uploadJob } from './client'
import type {
  AudioDto,
  OcrResultDto,
  PagedResult,
  StudioTtsRequest,
  TranscriptionResult,
  VoiceDto,
  VoiceStatus,
} from './types'

export const voiceApi = {
  voices: () => http.get<VoiceDto[]>('/api/voices'),
  status: () => http.get<VoiceStatus>('/api/voice/status'),
  tts: (body: StudioTtsRequest) => http.post<AudioDto>('/api/voice/tts', body),
  transcribe: (
    file: File,
    opts: { language?: string; save?: boolean } = {},
    onProgress?: (fraction: number) => void,
  ) => {
    const form = new FormData()
    form.append('file', file, file.name)
    if (opts.language) form.append('language', opts.language)
    form.append('save', String(opts.save ?? true))
    // Background job on the API: the upload finishes fast, then we wait for the transcript.
    return uploadJob<TranscriptionResult>('/api/voice/transcribe', form, { onProgress })
  },
  /** Picture → text. With `save` (default true) the picture is kept in the Images library. */
  ocr: (
    file: File,
    opts: { language?: string; save?: boolean } = {},
    onProgress?: (fraction: number) => void,
  ) => {
    const form = new FormData()
    form.append('file', file, file.name)
    if (opts.language) form.append('language', opts.language)
    form.append('save', String(opts.save ?? true))
    // Background job on the API: the upload finishes fast, then we wait for the text.
    return uploadJob<OcrResultDto>('/api/voice/ocr', form, { onProgress })
  },
  history: (page = 1, pageSize = 20) =>
    http.get<PagedResult<AudioDto>>('/api/voice/history', { page, pageSize }),
  deleteHistory: (id: string) => http.del(`/api/voice/history/${id}`),
}
