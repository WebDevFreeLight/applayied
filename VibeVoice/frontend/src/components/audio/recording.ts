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

/**
 * Browsers expose the microphone only to secure pages (https:// or http://localhost). Opened over the LAN
 * as http://192.168.x.x, navigator.mediaDevices is undefined, which is not the browser's fault.
 */
export function isInsecureContext(): boolean {
  return typeof window !== 'undefined' && window.isSecureContext === false
}

export type RecorderErrorKind = 'denied' | 'unsupported' | 'insecure' | 'noDevice' | 'failed'

export function classifyMediaError(err: unknown): RecorderErrorKind {
  const name = err instanceof DOMException || err instanceof Error ? err.name : ''
  if (name === 'NotAllowedError' || name === 'SecurityError' || name === 'PermissionDeniedError') return 'denied'
  if (name === 'NotFoundError' || name === 'DevicesNotFoundError' || name === 'OverconstrainedError') return 'noDevice'
  if (name === 'NotSupportedError' || name === 'TypeError') return 'unsupported'
  return 'failed'
}
