import { act, fireEvent, screen, waitFor, within } from '@testing-library/react'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import type { JobDto } from '@/api/client'
import { jobsApi } from '@/api/jobs'
import { makeUser, renderWithAuth } from '@/test/adminFixtures'
import { JobsPage } from './JobsPage'

const done: JobDto = {
  id: 'j-ok',
  kind: 'ImageOcr',
  status: 'Succeeded',
  createdAt: '2026-11-07T10:00:00Z',
  startedAt: '2026-11-07T10:00:01Z',
  finishedAt: '2026-11-07T10:00:03Z',
  durationSeconds: 1.6,
  attempts: 1,
  result: { id: 'img1', ocrText: 'قائمة المشتريات\nShopping list' },
  payload: { targetId: 'img1', ownerType: 'Note', ownerId: 'n1', language: 'ar' },
  canRetry: false,
  canDelete: true,
}

const failed: JobDto = {
  id: 'j-bad',
  kind: 'StudioTranscribe',
  status: 'Failed',
  createdAt: '2026-11-07T09:00:00Z',
  attempts: 4,
  errorStatus: 503,
  errorTitle: 'Voice service unavailable',
  error: 'The voice service is not responding.',
  errorDetail: 'VoiceServiceUnavailableException: down --> HttpRequestException: Connection refused',
  userEmail: 'sara@example.com',
  userName: 'Sara',
  payload: { fileName: 'memo.webm', contentType: 'audio/webm', save: true },
  canRetry: true,
  canDelete: true,
}

beforeEach(() => {
  vi.restoreAllMocks()
  vi.spyOn(jobsApi, 'summary').mockResolvedValue({ queued: 0, running: 0, succeeded: 1, failed: 1, total: 2 })
  vi.spyOn(jobsApi, 'list').mockResolvedValue({ items: [done, failed], page: 1, pageSize: 20, total: 2 })
  vi.spyOn(jobsApi, 'get').mockImplementation(async (_scope, id) => (id === failed.id ? failed : done))
})

/** Waits until the job rows (not just the filters) are on screen. */
const rows = () => screen.findAllByRole('button', { name: 'Details' })

describe('Jobs page', () => {
  it('lists jobs with status, timing, errors and a result preview', async () => {
    renderWithAuth(JobsPage, makeUser(), '/jobs')
    await rows()
    expect(screen.getAllByText('Picture → text').length).toBeGreaterThan(1) // row + type filter option
    expect(screen.getAllByText('Studio · transcription').length).toBeGreaterThan(1)
    expect(screen.getAllByText('Done').length).toBeGreaterThan(0)
    expect(screen.getByText('took 1.6 s')).toBeInTheDocument()
    expect(screen.getByText('4 attempts')).toBeInTheDocument()
    expect(screen.getByText(/Voice service unavailable \(503\)/)).toBeInTheDocument()
    expect(screen.getByText('The voice service is not responding.')).toBeInTheDocument()
    expect(screen.getByText(/Shopping list/)).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Open' })).toHaveAttribute('href', '/notes/n1')
    // Only failed jobs can be retried.
    expect(screen.getAllByRole('button', { name: 'Retry' })).toHaveLength(1)
  })

  it('filters by status and type through the API', async () => {
    renderWithAuth(JobsPage, makeUser(), '/jobs')
    await rows()
    fireEvent.click(screen.getByRole('radio', { name: 'Failed' }))
    await waitFor(() => expect(jobsApi.list).toHaveBeenLastCalledWith('mine', expect.objectContaining({ status: 'Failed', page: 1 }), expect.anything()))
    fireEvent.change(screen.getByRole('combobox', { name: 'Filter by type' }), { target: { value: 'AudioTranscribe' } })
    await waitFor(() =>
      expect(jobsApi.list).toHaveBeenLastCalledWith('mine', expect.objectContaining({ kind: 'AudioTranscribe' }), expect.anything()),
    )
  })

  it('retries a failed job', async () => {
    const retry = vi.spyOn(jobsApi, 'retry').mockResolvedValue({ ...failed, status: 'Queued', canRetry: false })
    renderWithAuth(JobsPage, makeUser(), '/jobs')
    await rows()
    await act(async () => fireEvent.click(screen.getByRole('button', { name: 'Retry' })))
    expect(retry).toHaveBeenCalledWith('mine', 'j-bad')
  })

  it('shows every detail of a job, including the technical error for admins', async () => {
    renderWithAuth(JobsPage, makeUser(), '/jobs')
    await rows()
    fireEvent.click(screen.getAllByRole('button', { name: 'Details' })[1])
    const dialog = await screen.findByRole('dialog')
    expect(within(dialog).getByText('j-bad')).toBeInTheDocument()
    expect(within(dialog).getByText('memo.webm (audio/webm)')).toBeInTheDocument()
    expect(within(dialog).getByText('Sara <sara@example.com>')).toBeInTheDocument()
    expect(within(dialog).getByText(/HTTP 503/)).toBeInTheDocument()
    expect(within(dialog).getByText(/Connection refused/)).toBeInTheDocument()
  })

  it('lets administrators switch to every user’s jobs; members do not see the switch', async () => {
    const first = renderWithAuth(JobsPage, makeUser(), '/jobs')
    await rows()
    fireEvent.click(screen.getByRole('radio', { name: 'All users' }))
    await waitFor(() => expect(jobsApi.list).toHaveBeenLastCalledWith('all', expect.anything(), expect.anything()))
    expect(screen.getByText('sara@example.com')).toBeInTheDocument()
    first.unmount()

    renderWithAuth(JobsPage, makeUser({ role: 'User', permissions: ['notes', 'voice.ocr', 'voice.transcribe'] }), '/jobs')
    await rows()
    expect(screen.queryByRole('radio', { name: 'All users' })).not.toBeInTheDocument()
  })
})
