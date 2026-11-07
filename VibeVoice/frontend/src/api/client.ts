import type { ProblemDetails } from './types'

const TOKEN_KEY = 'vv.token'

/**
 * API origin. With no VITE_API_URL, the Vite dev server proxies same-origin `/api` calls to the API
 * (see vite.config.ts), so the app also works from a phone on the LAN (`npm run dev:lan`). Production
 * builds and tests keep an explicit absolute URL.
 */
export const API_URL: string = (
  (import.meta.env.VITE_API_URL as string | undefined) ||
  // Any dev-server mode (development, lan) proxies /api; tests keep an absolute URL.
  (import.meta.env.DEV && import.meta.env.MODE !== 'test' ? '' : 'http://localhost:5080')
).replace(/\/+$/, '')

/** Typed error raised for every non-2xx response (mapped from RFC 7807 ProblemDetails). */
export class ApiError extends Error {
  readonly status: number
  readonly title: string
  readonly detail: string | undefined
  readonly type: string | undefined
  readonly errors: Record<string, string[]> | undefined
  readonly problem: ProblemDetails | undefined

  constructor(status: number, problem?: ProblemDetails, fallbackMessage?: string) {
    const title = problem?.title || fallbackMessage || `Request failed (${status})`
    super(problem?.detail || title)
    this.name = 'ApiError'
    this.status = status
    this.title = title
    this.detail = problem?.detail
    this.type = problem?.type
    this.errors = problem?.errors
    this.problem = problem
  }

  get isNetworkError(): boolean {
    return this.status === 0
  }

  get isValidation(): boolean {
    return this.status === 400 && !!this.errors
  }

  get isVoiceUnavailable(): boolean {
    return this.status === 503
  }

  /** First validation message per field, with case-insensitive keys (ASP.NET uses PascalCase). */
  fieldErrors(): Record<string, string> {
    const out: Record<string, string> = {}
    for (const [key, msgs] of Object.entries(this.errors ?? {})) {
      if (!msgs?.length) continue
      const k = key.length ? key[0].toLowerCase() + key.slice(1) : key
      out[k] = msgs[0]
    }
    return out
  }

  /** Human readable summary: detail, or title + validation messages. */
  get summary(): string {
    const msgs = Object.values(this.errors ?? {}).flat()
    if (msgs.length) return msgs.join(' ')
    return this.detail || this.title
  }
}

// ---------- token & 401 handling ----------

let token: string | null = readStoredToken()
let unauthorizedHandler: (() => void) | null = null

function readStoredToken(): string | null {
  try {
    return localStorage.getItem(TOKEN_KEY)
  } catch {
    return null
  }
}

export function getToken(): string | null {
  return token
}

export function setToken(value: string | null): void {
  token = value
  try {
    if (value) localStorage.setItem(TOKEN_KEY, value)
    else localStorage.removeItem(TOKEN_KEY)
  } catch {
    /* storage unavailable */
  }
}

/** Registers the callback invoked when an authenticated request returns 401. */
export function onUnauthorized(handler: (() => void) | null): void {
  unauthorizedHandler = handler
}

// ---------- helpers ----------

export type QueryValue = string | number | boolean | null | undefined | readonly string[]
export type QueryParams = Record<string, QueryValue>

export function buildQuery(params?: QueryParams): string {
  if (!params) return ''
  const sp = new URLSearchParams()
  for (const [key, value] of Object.entries(params)) {
    if (value === undefined || value === null || value === '') continue
    if (Array.isArray(value)) {
      if (value.length) sp.set(key, value.join(','))
    } else {
      sp.set(key, String(value))
    }
  }
  const s = sp.toString()
  return s ? `?${s}` : ''
}

export function apiUrl(path: string, query?: QueryParams): string {
  const p = path.startsWith('/') ? path : `/${path}`
  return `${API_URL}${p}${buildQuery(query)}`
}

/**
 * Builds a URL for a stored file (`filePath` / `thumbnailPath`), usable in `<img>` / `<audio>`.
 * The JWT is passed as `?access_token=` since tags cannot send headers.
 */
export function fileUrl(path: string | null | undefined): string {
  if (!path) return ''
  const clean = path.replace(/\\/g, '/').replace(/^\/+/, '')
  const encoded = clean.split('/').map(encodeURIComponent).join('/')
  const t = getToken()
  return `${API_URL}/api/files/${encoded}${t ? `?access_token=${encodeURIComponent(t)}` : ''}`
}

async function parseProblem(res: Response): Promise<ProblemDetails | undefined> {
  const text = await res.text().catch(() => '')
  if (!text) return undefined
  try {
    const json: unknown = JSON.parse(text)
    if (json && typeof json === 'object') return json as ProblemDetails
  } catch {
    /* not JSON */
  }
  return { title: text.slice(0, 300) }
}

function handleUnauthorized(status: number, hadToken: boolean) {
  if (status === 401 && hadToken) unauthorizedHandler?.()
}

export interface RequestOptions {
  method?: 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE'
  body?: unknown
  query?: QueryParams
  signal?: AbortSignal
  headers?: Record<string, string>
}

/** Typed fetch wrapper: base URL, bearer token, JSON/FormData bodies, ProblemDetails → ApiError. */
export async function request<T>(path: string, options: RequestOptions = {}): Promise<T> {
  return (await send<T>(path, options)).data
}

/** Same as {@link request} but also returns the HTTP status (e.g. 202 for a queued background job). */
async function send<T>(path: string, options: RequestOptions = {}): Promise<{ status: number; data: T }> {
  const { method = 'GET', body, query, signal, headers = {} } = options
  const h: Record<string, string> = { Accept: 'application/json', ...headers }
  const t = getToken()
  if (t) h.Authorization = `Bearer ${t}`

  let payload: BodyInit | undefined
  if (body instanceof FormData || body instanceof Blob) {
    payload = body
  } else if (body !== undefined) {
    h['Content-Type'] = 'application/json'
    payload = JSON.stringify(body)
  }

  let res: Response
  try {
    res = await fetch(apiUrl(path, query), { method, headers: h, body: payload, signal })
  } catch (err) {
    if (err instanceof DOMException && err.name === 'AbortError') throw err
    throw new ApiError(0, undefined, 'Network error — is the API running?')
  }

  if (!res.ok) {
    handleUnauthorized(res.status, !!t)
    throw new ApiError(res.status, await parseProblem(res), res.statusText || undefined)
  }
  if (res.status === 204 || res.status === 205) return { status: res.status, data: undefined as T }
  const text = await res.text()
  if (!text) return { status: res.status, data: undefined as T }
  const ct = res.headers.get('content-type') ?? ''
  return { status: res.status, data: (ct.includes('json') ? JSON.parse(text) : text) as T }
}

// ---------- Background jobs (OCR / transcription) ----------

export type JobStatus = 'Queued' | 'Running' | 'Succeeded' | 'Failed'
export type JobKind = 'ImageOcr' | 'AudioTranscribe' | 'StudioOcr' | 'StudioTranscribe'

/** What the job was asked to do (only the fields its kind uses are set). */
export interface JobPayload {
  targetId?: string | null
  ownerType?: string | null
  ownerId?: string | null
  appendToBody?: boolean
  language?: string | null
  save?: boolean
  source?: string | null
  fileName?: string | null
  contentType?: string | null
}

/** GET /api/jobs/{id}: an OCR / transcription request the API runs in the background. */
export interface JobDto<T = unknown> {
  id: string
  kind: JobKind | string
  status: JobStatus
  createdAt: string
  startedAt?: string | null
  finishedAt?: string | null
  attempts: number
  /** HTTP status the synchronous call would have failed with (e.g. 503 voice service down). */
  errorStatus?: number | null
  errorTitle?: string | null
  error?: string | null
  /** Exactly what the synchronous endpoint returns, once Succeeded. */
  result?: T | null
  payload?: JobPayload | null
  durationSeconds?: number | null
  canRetry?: boolean
  canDelete?: boolean
  /** Administrators' view only. */
  userId?: string | null
  userEmail?: string | null
  userName?: string | null
  /** Administrators' view only: exception chain / upstream error. */
  errorDetail?: string | null
}

/** RFC 7240: asks the API to queue the work and answer 202 + a job instead of waiting. */
export const PREFER_ASYNC: Record<string, string> = { Prefer: 'respond-async' }

export interface WaitForJobOptions {
  signal?: AbortSignal
  /** Give up after this long (default 25 min: a free host may first have to wake the voice service). */
  timeoutMs?: number
  /** First poll delay in ms; grows to 4 s. */
  intervalMs?: number
}

function sleep(ms: number, signal?: AbortSignal): Promise<void> {
  return new Promise((resolve, reject) => {
    if (signal?.aborted) return reject(new DOMException('Aborted', 'AbortError'))
    const timer = setTimeout(resolve, ms)
    signal?.addEventListener(
      'abort',
      () => {
        clearTimeout(timer)
        reject(new DOMException('Aborted', 'AbortError'))
      },
      { once: true },
    )
  })
}

/**
 * Polls a background job until it finishes. Resolves with its result, or throws the same ApiError the
 * synchronous endpoint would have thrown. Short outages while polling (the API waking up, a flaky
 * phone connection) are retried; the job itself keeps running on the server either way.
 */
export async function waitForJob<T>(job: JobDto<T> | string, options: WaitForJobOptions = {}): Promise<T> {
  const { signal, timeoutMs = 25 * 60_000, intervalMs = 700 } = options
  const deadline = Date.now() + timeoutMs
  let current: JobDto<T> | null = typeof job === 'string' ? null : job
  const id = typeof job === 'string' ? job : job.id
  let delay = intervalMs
  let failures = 0

  while (!current || current.status === 'Queued' || current.status === 'Running') {
    if (Date.now() > deadline) {
      throw new ApiError(504, { title: 'Timed out', detail: 'The job is still running; check again later.' })
    }
    if (current) {
      await sleep(delay, signal)
      delay = Math.min(Math.round(delay * 1.5), 4000)
    }
    try {
      current = await request<JobDto<T>>(`/api/jobs/${id}`, { signal })
      failures = 0
    } catch (err) {
      const transient = err instanceof ApiError && (err.status === 0 || err.status >= 500)
      if (!transient || ++failures > 15) throw err
      if (!current) await sleep(delay, signal)
    }
  }

  if (current.status === 'Failed') {
    const status = current.errorStatus ?? 500
    throw new ApiError(status, {
      title: current.errorTitle ?? 'Error',
      detail: current.error ?? undefined,
      status,
    })
  }
  return current.result as T
}

/** POSTs JSON as a background job and waits for it; an older API that answers directly works too. */
export async function runJob<T>(path: string, body?: unknown, options: WaitForJobOptions = {}): Promise<T> {
  const res = await send<T | JobDto<T>>(path, { method: 'POST', body, headers: PREFER_ASYNC, signal: options.signal })
  return res.status === 202 ? waitForJob<T>(res.data as JobDto<T>, options) : (res.data as T)
}

/** Uploads a file as a background job (with upload progress) and waits for its result. */
export async function uploadJob<T>(path: string, form: FormData, options: UploadOptions = {}): Promise<T> {
  const res = await uploadWithResponse<T | JobDto<T>>(path, form, {
    ...options,
    headers: { ...PREFER_ASYNC, ...options.headers },
  })
  return res.status === 202 ? waitForJob<T>(res.data as JobDto<T>, { signal: options.signal }) : (res.data as T)
}

/**
 * Upload endpoints that save first and queue OCR / transcription jobs report them in X-Job-Ids.
 * Waits for all of them; returns each finished result and the first error message, if any.
 */
export async function waitForJobIds<T>(
  header: string | null,
  options: WaitForJobOptions = {},
): Promise<{ results: T[]; error: string | null }> {
  const ids = (header ?? '').split(',').map((x) => x.trim()).filter(Boolean)
  const results: T[] = []
  let error: string | null = null
  for (const id of ids) {
    try {
      results.push(await waitForJob<T>(id, options))
    } catch (err) {
      if (err instanceof DOMException && err.name === 'AbortError') throw err
      error ??= err instanceof Error ? err.message : String(err)
    }
  }
  return { results, error }
}

export const http = {
  get: <T>(path: string, query?: QueryParams, signal?: AbortSignal) =>
    request<T>(path, { query, signal }),
  post: <T>(path: string, body?: unknown) => request<T>(path, { method: 'POST', body }),
  put: <T>(path: string, body?: unknown) => request<T>(path, { method: 'PUT', body }),
  patch: <T>(path: string, body?: unknown) => request<T>(path, { method: 'PATCH', body }),
  del: <T = void>(path: string) => request<T>(path, { method: 'DELETE' }),
}

export interface UploadOptions {
  /** Called with a 0..1 fraction while the request body is sent. */
  onProgress?: (fraction: number) => void
  signal?: AbortSignal
  method?: 'POST' | 'PUT'
  /** Extra request headers (e.g. Prefer: respond-async). */
  headers?: Record<string, string>
}

export interface UploadResponse<T> {
  data: T
  status: number
  /** Reads a response header (only headers exposed via CORS are visible cross-origin). */
  header: (name: string) => string | null
}

/** Multipart upload via XHR so we get upload progress events; also exposes response headers. */
export function uploadWithResponse<T>(
  path: string,
  form: FormData,
  options: UploadOptions = {},
): Promise<UploadResponse<T>> {
  const { onProgress, signal, method = 'POST', headers = {} } = options
  return new Promise<UploadResponse<T>>((resolve, reject) => {
    const xhr = new XMLHttpRequest()
    xhr.open(method, apiUrl(path))
    xhr.setRequestHeader('Accept', 'application/json')
    const t = getToken()
    if (t) xhr.setRequestHeader('Authorization', `Bearer ${t}`)
    for (const [name, value] of Object.entries(headers)) xhr.setRequestHeader(name, value)

    if (onProgress) {
      xhr.upload.onprogress = (e) => {
        if (e.lengthComputable && e.total > 0) onProgress(Math.min(1, e.loaded / e.total))
      }
    }

    const header = (name: string): string | null => {
      try {
        const v = xhr.getResponseHeader(name)
        if (v == null || v === '') return null
        try {
          return decodeURIComponent(v)
        } catch {
          return v
        }
      } catch {
        return null
      }
    }

    xhr.onload = () => {
      const text = xhr.responseText
      if (xhr.status >= 200 && xhr.status < 300) {
        onProgress?.(1)
        let data: T
        if (!text) data = undefined as T
        else {
          try {
            data = JSON.parse(text) as T
          } catch {
            data = text as T
          }
        }
        resolve({ data, status: xhr.status, header })
        return
      }
      handleUnauthorized(xhr.status, !!t)
      let problem: ProblemDetails | undefined
      try {
        problem = text ? (JSON.parse(text) as ProblemDetails) : undefined
      } catch {
        problem = { title: text.slice(0, 300) }
      }
      reject(new ApiError(xhr.status, problem, xhr.statusText || undefined))
    }
    xhr.onerror = () => reject(new ApiError(0, undefined, 'Network error — is the API running?'))
    xhr.onabort = () => reject(new DOMException('Upload aborted', 'AbortError'))

    if (signal) {
      if (signal.aborted) {
        reject(new DOMException('Upload aborted', 'AbortError'))
        return
      }
      signal.addEventListener('abort', () => xhr.abort(), { once: true })
    }
    xhr.send(form)
  })
}

/** Multipart upload via XHR with progress; resolves with the parsed body. */
export function upload<T>(path: string, form: FormData, options: UploadOptions = {}): Promise<T> {
  return uploadWithResponse<T>(path, form, options).then((r) => r.data)
}

/** Download URL for a clip's transcript (`txt`, `srt` or `vtt`), usable in <a download>. */
export function transcriptUrl(audioId: string, format: 'txt' | 'srt' | 'vtt'): string {
  const t = getToken()
  const q = new URLSearchParams({ format })
  if (t) q.set('access_token', t)
  return `${API_URL}/api/audio/${encodeURIComponent(audioId)}/transcript?${q.toString()}`
}

let forbiddenMessage = (): string => "You don't have permission to do that."

/** Lets the i18n layer provide the (translated) 403 message. */
export function setForbiddenMessage(fn: () => string): void {
  forbiddenMessage = fn
}

export function isForbidden(err: unknown): boolean {
  return err instanceof ApiError && err.status === 403
}

/** Extracts a user-facing message from any thrown value. 403 → "You don't have permission". */
export function errorMessage(err: unknown, fallback = 'Something went wrong'): string {
  if (isForbidden(err)) return forbiddenMessage()
  if (err instanceof ApiError) return err.summary
  if (err instanceof Error && err.message) return err.message
  return fallback
}
