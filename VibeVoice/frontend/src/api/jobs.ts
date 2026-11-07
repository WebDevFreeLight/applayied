import { http, type JobDto, type JobKind, type QueryParams } from './client'
import type { PagedResult } from './types'

/** "mine" = the signed-in user's jobs; "all" = every user's (administrators, with technical detail). */
export type JobScope = 'mine' | 'all'

export interface JobListQuery {
  /** Queued | Running | Succeeded | Failed, or "active" (queued + running). */
  status?: string
  kind?: JobKind
  userId?: string
  page?: number
  pageSize?: number
}

export interface JobSummary {
  queued: number
  running: number
  succeeded: number
  failed: number
  total: number
}

const base = (scope: JobScope) => (scope === 'all' ? '/api/admin/jobs' : '/api/jobs')

export const jobsApi = {
  list: (scope: JobScope, q: JobListQuery, signal?: AbortSignal) =>
    http.get<PagedResult<JobDto>>(`${base(scope)}`, q as QueryParams, signal),
  summary: (scope: JobScope, signal?: AbortSignal) => http.get<JobSummary>(`${base(scope)}/summary`, undefined, signal),
  get: (scope: JobScope, id: string, signal?: AbortSignal) => http.get<JobDto>(`${base(scope)}/${id}`, undefined, signal),
  retry: (scope: JobScope, id: string) => http.post<JobDto>(`${base(scope)}/${id}/retry`),
  remove: (scope: JobScope, id: string) => http.del(`${base(scope)}/${id}`),
  /** Deletes the signed-in user's finished (succeeded + failed) jobs. */
  clearFinished: () => http.del<{ deleted: number }>('/api/jobs/finished'),
}

/** The text a finished job produced (OCR text or transcript), whatever the job kind. */
export function jobResultText(job: JobDto): string {
  const r = job.result as Record<string, unknown> | null | undefined
  if (!r) return ''
  const text = r.ocrText ?? r.transcript ?? r.text
  return typeof text === 'string' ? text : ''
}

/** Where the job's picture / clip lives in the app, if it belongs to a note or memory. */
export function jobOwnerLink(job: JobDto): string | null {
  const p = job.payload
  if (!p?.ownerId) return null
  if (p.ownerType === 'Note') return `/notes/${p.ownerId}`
  if (p.ownerType === 'Memory') return `/memories/${p.ownerId}`
  return null
}
