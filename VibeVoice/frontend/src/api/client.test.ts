import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { API_URL, ApiError, buildQuery, errorMessage, fileUrl, http, onUnauthorized, setToken, upload } from './client'

function fakeResponse(status: number, body: unknown, contentType = 'application/json') {
  const text = body === undefined ? '' : typeof body === 'string' ? body : JSON.stringify(body)
  return {
    ok: status >= 200 && status < 300,
    status,
    statusText: `Status ${status}`,
    headers: { get: (k: string) => (k.toLowerCase() === 'content-type' ? contentType : null) },
    text: async () => text,
  }
}

const fetchMock = vi.fn()

beforeEach(() => {
  fetchMock.mockReset()
  vi.stubGlobal('fetch', fetchMock)
  setToken(null)
  onUnauthorized(null)
})

afterEach(() => {
  vi.unstubAllGlobals()
  setToken(null)
})

describe('api client', () => {
  it('defaults the base URL to http://localhost:5080', () => {
    expect(API_URL).toBe('http://localhost:5080')
  })

  it('maps ProblemDetails validation errors to a typed ApiError', async () => {
    fetchMock.mockResolvedValue(
      fakeResponse(
        400,
        {
          type: 'https://tools.ietf.org/html/rfc9110#section-15.5.1',
          title: 'One or more validation errors occurred.',
          status: 400,
          errors: { Title: ['Title must be 200 characters or fewer.'], Tags: ['Too many tags.'] },
        },
        'application/problem+json',
      ),
    )
    const err = await http.post('/api/notes', { title: 'x' }).catch((e: unknown) => e)
    expect(err).toBeInstanceOf(ApiError)
    const e = err as ApiError
    expect(e.status).toBe(400)
    expect(e.title).toBe('One or more validation errors occurred.')
    expect(e.isValidation).toBe(true)
    expect(e.errors).toEqual({ Title: ['Title must be 200 characters or fewer.'], Tags: ['Too many tags.'] })
    expect(e.fieldErrors()).toEqual({ title: 'Title must be 200 characters or fewer.', tags: 'Too many tags.' })
    expect(e.summary).toContain('Title must be 200 characters or fewer.')
  })

  it('maps 404 and 503 (voice service down) with detail', async () => {
    fetchMock.mockResolvedValueOnce(fakeResponse(404, { title: 'Not Found', status: 404, detail: 'Note not found' }))
    const notFound = (await http.get('/api/notes/1').catch((e: unknown) => e)) as ApiError
    expect(notFound.status).toBe(404)
    expect(notFound.message).toBe('Note not found')
    expect(errorMessage(notFound)).toBe('Note not found')

    fetchMock.mockResolvedValueOnce(fakeResponse(503, { title: 'Voice service unavailable', status: 503 }))
    const down = (await http.post('/api/voice/tts', { text: 'hi' }).catch((e: unknown) => e)) as ApiError
    expect(down.status).toBe(503)
    expect(down.isVoiceUnavailable).toBe(true)
    expect(down.title).toBe('Voice service unavailable')
  })

  it('maps non-JSON error bodies and network failures', async () => {
    fetchMock.mockResolvedValueOnce(fakeResponse(500, 'Internal kaboom', 'text/plain'))
    const e500 = (await http.get('/api/x').catch((e: unknown) => e)) as ApiError
    expect(e500.status).toBe(500)
    expect(e500.title).toBe('Internal kaboom')

    fetchMock.mockRejectedValueOnce(new TypeError('Failed to fetch'))
    const net = (await http.get('/api/x').catch((e: unknown) => e)) as ApiError
    expect(net).toBeInstanceOf(ApiError)
    expect(net.status).toBe(0)
    expect(net.isNetworkError).toBe(true)
  })

  it('sends the bearer token, JSON bodies and query strings', async () => {
    setToken('jwt-123')
    fetchMock.mockResolvedValue(fakeResponse(200, { ok: true }))
    const res = await http.get<{ ok: boolean }>('/api/notes', { page: 2, search: 'مرحبا', tag: undefined, hasAudio: true })
    expect(res).toEqual({ ok: true })
    const [url, init] = fetchMock.mock.calls[0] as [string, RequestInit]
    expect(url).toBe(`http://localhost:5080/api/notes?page=2&search=${encodeURIComponent('مرحبا')}&hasAudio=true`)
    expect((init.headers as Record<string, string>).Authorization).toBe('Bearer jwt-123')

    await http.put('/api/notes/1', { title: 'T', body: '', tags: [] })
    const [, putInit] = fetchMock.mock.calls[1] as [string, RequestInit]
    expect(putInit.method).toBe('PUT')
    expect((putInit.headers as Record<string, string>)['Content-Type']).toBe('application/json')
    expect(putInit.body).toBe(JSON.stringify({ title: 'T', body: '', tags: [] }))
  })

  it('returns undefined for 204 No Content', async () => {
    fetchMock.mockResolvedValue(fakeResponse(204, undefined))
    await expect(http.del('/api/notes/1')).resolves.toBeUndefined()
  })

  it('calls the unauthorized handler on 401 only for authenticated requests', async () => {
    const handler = vi.fn()
    onUnauthorized(handler)
    fetchMock.mockResolvedValue(fakeResponse(401, { title: 'Unauthorized', status: 401 }))

    await http.post('/api/auth/login', {}).catch(() => undefined)
    expect(handler).not.toHaveBeenCalled()

    setToken('expired')
    await http.get('/api/notes').catch(() => undefined)
    expect(handler).toHaveBeenCalledTimes(1)
  })

  it('builds query strings (arrays joined, empties skipped)', () => {
    expect(buildQuery({ q: 'a b', types: ['notes', 'audio'], x: '', y: null, z: undefined })).toBe(
      '?q=a+b&types=notes%2Caudio',
    )
    expect(buildQuery({})).toBe('')
  })

  it('fileUrl() builds an /api/files URL with ?access_token=', () => {
    expect(fileUrl(null)).toBe('')
    setToken('tok en')
    expect(fileUrl('images/2026/11/abc.jpg')).toBe('http://localhost:5080/api/files/images/2026/11/abc.jpg?access_token=tok%20en')
    expect(fileUrl('\\audio\\x y.webm')).toBe('http://localhost:5080/api/files/audio/x%20y.webm?access_token=tok%20en')
  })
})

class FakeXHR {
  static last: FakeXHR
  method = ''
  url = ''
  headers: Record<string, string> = {}
  status = 0
  statusText = ''
  responseText = ''
  body: unknown
  upload: { onprogress: ((e: { lengthComputable: boolean; loaded: number; total: number }) => void) | null } = {
    onprogress: null,
  }
  onload: (() => void) | null = null
  onerror: (() => void) | null = null
  onabort: (() => void) | null = null
  constructor() {
    FakeXHR.last = this
  }
  open(method: string, url: string) {
    this.method = method
    this.url = url
  }
  setRequestHeader(k: string, v: string) {
    this.headers[k] = v
  }
  send(body: unknown) {
    this.body = body
  }
  abort() {
    this.onabort?.()
  }
  respond(status: number, body: unknown) {
    this.status = status
    this.responseText = JSON.stringify(body)
    this.onload?.()
  }
}

describe('upload (XHR multipart with progress)', () => {
  it('reports progress and resolves with JSON', async () => {
    vi.stubGlobal('XMLHttpRequest', FakeXHR)
    setToken('t1')
    const progress: number[] = []
    const form = new FormData()
    form.append('files', new File(['x'], 'a.png', { type: 'image/png' }))
    const p = upload<{ id: string }[]>('/api/images', form, { onProgress: (f) => progress.push(f) })
    const xhr = FakeXHR.last
    expect(xhr.method).toBe('POST')
    expect(xhr.url).toBe('http://localhost:5080/api/images')
    expect(xhr.headers.Authorization).toBe('Bearer t1')
    expect(xhr.body).toBe(form)
    xhr.upload.onprogress?.({ lengthComputable: true, loaded: 50, total: 100 })
    xhr.respond(200, [{ id: 'img1' }])
    await expect(p).resolves.toEqual([{ id: 'img1' }])
    expect(progress).toEqual([0.5, 1])
  })

  it('rejects with an ApiError built from ProblemDetails', async () => {
    vi.stubGlobal('XMLHttpRequest', FakeXHR)
    const p = upload('/api/notes/1/audio', new FormData())
    FakeXHR.last.respond(400, { title: 'Invalid file', status: 400, errors: { file: ['Unsupported audio format.'] } })
    const err = (await p.catch((e: unknown) => e)) as ApiError
    expect(err).toBeInstanceOf(ApiError)
    expect(err.status).toBe(400)
    expect(err.fieldErrors()).toEqual({ file: 'Unsupported audio format.' })
  })
})
