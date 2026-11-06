/** MIME types tried in order of preference for MediaRecorder. */
export const RECORDER_MIME_CANDIDATES = ['audio/webm;codecs=opus', 'audio/webm', 'audio/ogg;codecs=opus', 'audio/ogg', 'audio/mp4'] as const

export function isRecordingSupported(): boolean {
  return (
    typeof window !== 'undefined' &&
    typeof window.MediaRecorder !== 'undefined' &&
    !!navigator.mediaDevices?.getUserMedia
  )
}

/** Picks the first supported recording mime type ('' lets the browser decide). */
export function pickRecorderMimeType(): string {
  const MR = typeof window !== 'undefined' ? window.MediaRecorder : undefined
  if (!MR || typeof MR.isTypeSupported !== 'function') return ''
  return RECORDER_MIME_CANDIDATES.find((m) => MR.isTypeSupported(m)) ?? ''
}

export type RecorderErrorKind = 'denied' | 'unsupported' | 'noDevice' | 'failed'

export function classifyMediaError(err: unknown): RecorderErrorKind {
  const name = err instanceof DOMException || err instanceof Error ? err.name : ''
  if (name === 'NotAllowedError' || name === 'SecurityError' || name === 'PermissionDeniedError') return 'denied'
  if (name === 'NotFoundError' || name === 'DevicesNotFoundError' || name === 'OverconstrainedError') return 'noDevice'
  if (name === 'NotSupportedError' || name === 'TypeError') return 'unsupported'
  return 'failed'
}
