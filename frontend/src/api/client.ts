import axios, { AxiosError } from 'axios'
import i18n from '../i18n'

// Single axios instance. Base path defaults to the Vite dev proxy (/api -> backend).
const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE || '/api',
  timeout: 20000,
})

// Attach the JWT and the request language on every call. The backend reads Accept-Language
// to return Arabic/English error messages, so the UI language and API language stay in sync.
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('pmo.token')
  if (token) config.headers.Authorization = `Bearer ${token}`
  config.headers['Accept-Language'] = i18n.language || 'en'
  return config
})

/** The backend envelope: { code, msg, data }. 200 = success. */
export interface Envelope<T> {
  code: number
  msg: string
  data: T
}

/** Unwrap the envelope, throwing a localized Error on a non-200 code. */
export async function unwrap<T>(p: Promise<{ data: Envelope<T> }>): Promise<T> {
  try {
    const res = await p
    const body = res.data
    if (body && typeof body === 'object' && 'code' in body) {
      if (body.code !== 200) throw new Error(body.msg || 'Request failed')
      return body.data
    }
    return body as unknown as T
  } catch (err) {
    const ax = err as AxiosError<{ msg?: string }>
    // 401/403 bodies carry a localized `msg` from the server (RBAC denial).
    const serverMsg = ax.response?.data?.msg
    if (ax.response?.status === 401) {
      localStorage.removeItem('pmo.token')
    }
    throw new Error(serverMsg || (err as Error).message || 'Request failed')
  }
}

export default api
