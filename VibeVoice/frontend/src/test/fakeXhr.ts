/** Minimal XMLHttpRequest stand-in for upload tests. */
export class FakeXHR {
  static instances: FakeXHR[] = []
  static get last(): FakeXHR {
    return FakeXHR.instances[FakeXHR.instances.length - 1]
  }
  method = ''
  url = ''
  headers: Record<string, string> = {}
  status = 0
  statusText = ''
  responseText = ''
  responseHeaders: Record<string, string> = {}
  body: unknown
  upload: { onprogress: ((e: { lengthComputable: boolean; loaded: number; total: number }) => void) | null } = {
    onprogress: null,
  }
  onload: (() => void) | null = null
  onerror: (() => void) | null = null
  onabort: (() => void) | null = null
  constructor() {
    FakeXHR.instances.push(this)
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
  getResponseHeader(name: string) {
    const key = Object.keys(this.responseHeaders).find((k) => k.toLowerCase() === name.toLowerCase())
    return key ? this.responseHeaders[key] : null
  }
  respond(status: number, body: unknown, headers: Record<string, string> = {}) {
    this.responseHeaders = headers
    this.status = status
    this.responseText = JSON.stringify(body)
    this.onload?.()
  }
}
