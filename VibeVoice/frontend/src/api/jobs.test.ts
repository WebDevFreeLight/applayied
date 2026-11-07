import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { ApiError, runJob, setToken, uploadJob, waitForJob, waitForJobIds, type JobDto } from './client'

function fakeResponse(status: number, body: unknown) {
  const text = body === undefined ? '' : JSON.stringify(body)
  return {
    ok: status >= 200 && status < 300,
    status,
    statusText: `Status ${status}`,
    headers: { get: (k: string) => (k.toLowerCase() === 'content-type' ? 'application/json' : null) },
    text: async () => text,
  }
}

const job = (status: JobDto['status'], extra: Partial<JobDto> = {}): JobDto => ({
  id: 'j1',
  kind: 'ImageOcr',
  status,
  createdAt: '2026-11-07T10:00:00Z',
  attempts: 1,
  ...extra,
})

const fetchMock = vi.fn()

beforeEach(() => {
  fetchMock.mockReset()
  vi.stubGlobal('fetch', fetchMock)
  setToken('tok')
})

afterEach(() => {
  vi.unstubAllGlobals()
  setToken(null)
})

const fast = { intervalMs: 1 }

describe('background jobs', () => {
  it('runJob sends Prefer: respond-async and polls the job until it succeeds', async () => {
    fetchMock
      .mockResolvedValueOnce(fakeResponse(202, job('Queued')))
      .mockResolvedValueOnce(fakeResponse(200, job('Running')))
      .mockResolvedValueOnce(fakeResponse(200, job('Succeeded', { result: { id: 'img', ocrText: 'مرحبا' } })))

    const result = await runJob<{ id: string; ocrText: string }>('/api/images/img/ocr', { language: 'ar' }, fast)

    expect(result.ocrText).toBe('مرحبا')
    const [url, init] = fetchMock.mock.calls[0]
    expect(url).toContain('/api/images/img/ocr')
    expect(init.method).toBe('POST')
    expect(init.headers.Prefer).toBe('respond-async')
    expect(fetchMock.mock.calls[1][0]).toContain('/api/jobs/j1')
  })

  it('runJob returns the body directly when an older API answers synchronously', async () => {
    fetchMock.mockResolvedValueOnce(fakeResponse(200, { id: 'img', ocrText: 'hi' }))
    expect(await runJob('/api/images/img/ocr', {}, fast)).toEqual({ id: 'img', ocrText: 'hi' })
    expect(fetchMock).toHaveBeenCalledTimes(1)
  })

  it('a failed job throws the ApiError the synchronous call would have', async () => {
    fetchMock.mockResolvedValueOnce(
      fakeResponse(200, job('Failed', { errorStatus: 503, errorTitle: 'Voice service unavailable', error: 'down' })),
    )
    const err = await waitForJob('j1', fast).catch((e: unknown) => e)
    expect(err).toBeInstanceOf(ApiError)
    expect((err as ApiError).status).toBe(503)
    expect((err as ApiError).title).toBe('Voice service unavailable')
    expect((err as ApiError).message).toBe('down')
  })

  it('keeps polling through short outages (e.g. the API waking up)', async () => {
    fetchMock
      .mockRejectedValueOnce(new TypeError('Failed to fetch'))
      .mockResolvedValueOnce(fakeResponse(502, { title: 'Bad gateway' }))
      .mockResolvedValueOnce(fakeResponse(200, job('Succeeded', { result: 'ok' })))
    expect(await waitForJob('j1', fast)).toBe('ok')
  })

  it('does not retry client errors such as 404', async () => {
    fetchMock.mockResolvedValueOnce(fakeResponse(404, { title: 'Not found' }))
    const err = await waitForJob('missing', fast).catch((e: unknown) => e)
    expect((err as ApiError).status).toBe(404)
    expect(fetchMock).toHaveBeenCalledTimes(1)
  })

  it('waitForJobIds collects results and the first error', async () => {
    fetchMock
      .mockResolvedValueOnce(fakeResponse(200, job('Succeeded', { id: 'a', result: 'A' })))
      .mockResolvedValueOnce(fakeResponse(200, job('Failed', { id: 'b', errorStatus: 422, error: 'unreadable' })))
    const out = await waitForJobIds<string>('a, b', fast)
    expect(out).toEqual({ results: ['A'], error: 'unreadable' })
    expect(await waitForJobIds(null)).toEqual({ results: [], error: null })
  })
})

class FakeXHR {
  static last: FakeXHR
  headers: Record<string, string> = {}
  status = 0
  statusText = ''
  responseText = ''
  upload = { onprogress: null }
  onload: (() => void) | null = null
  onerror: (() => void) | null = null
  onabort: (() => void) | null = null
  constructor() {
    FakeXHR.last = this
  }
  open() {}
  setRequestHeader(k: string, v: string) {
    this.headers[k] = v
  }
  send() {}
  getResponseHeader() {
    return null
  }
  respond(status: number, body: unknown) {
    this.status = status
    this.responseText = JSON.stringify(body)
    this.onload?.()
  }
}

describe('uploadJob', () => {
  it('uploads with Prefer: respond-async, then waits for the job result', async () => {
    vi.stubGlobal('XMLHttpRequest', FakeXHR)
    fetchMock.mockResolvedValueOnce(fakeResponse(200, job('Succeeded', { result: { text: 'نص' } })))

    const pending = uploadJob<{ text: string }>('/api/voice/ocr', new FormData())
    expect(FakeXHR.last.headers.Prefer).toBe('respond-async')
    FakeXHR.last.respond(202, job('Queued'))

    expect(await pending).toEqual({ text: 'نص' })
  })
})
