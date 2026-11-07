import { fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { attachmentsApi } from '@/api/attachments'
import type { AudioDto } from '@/api/types'
import { AudioRecorder } from './AudioRecorder'
import { pickRecorderMimeType } from './recording'

class MockMediaRecorder {
  static instances: MockMediaRecorder[] = []
  static isTypeSupported = vi.fn((type: string) => type === 'audio/webm;codecs=opus')
  state: 'inactive' | 'recording' | 'paused' = 'inactive'
  mimeType: string
  stream: MediaStream
  ondataavailable: ((e: { data: Blob }) => void) | null = null
  onstop: (() => void) | null = null
  onerror: (() => void) | null = null
  constructor(stream: MediaStream, options?: { mimeType?: string }) {
    this.stream = stream
    this.mimeType = options?.mimeType ?? 'audio/webm'
    MockMediaRecorder.instances.push(this)
  }
  start = vi.fn(() => {
    this.state = 'recording'
  })
  pause = vi.fn(() => {
    this.state = 'paused'
  })
  resume = vi.fn(() => {
    this.state = 'recording'
  })
  stop = vi.fn(() => {
    this.state = 'inactive'
    this.ondataavailable?.({ data: new Blob(['voice-bytes'], { type: 'audio/webm' }) })
    this.onstop?.()
  })
}

const trackStop = vi.fn()
const getUserMedia = vi.fn()

beforeEach(() => {
  MockMediaRecorder.instances = []
  trackStop.mockReset()
  getUserMedia.mockReset()
  getUserMedia.mockResolvedValue({ getTracks: () => [{ stop: trackStop }] })
  vi.stubGlobal('MediaRecorder', MockMediaRecorder)
  Object.defineProperty(navigator, 'mediaDevices', {
    configurable: true,
    value: { getUserMedia },
  })
  URL.createObjectURL = vi.fn(() => 'blob:preview')
  URL.revokeObjectURL = vi.fn()
})

afterEach(() => {
  vi.unstubAllGlobals()
})

const fakeAudio = { id: 'a1' } as AudioDto
const fakeResult = { audio: fakeAudio, transcriptionError: null }

describe('AudioRecorder', () => {
  it('picks the preferred supported mime type', () => {
    expect(pickRecorderMimeType()).toBe('audio/webm;codecs=opus')
    MockMediaRecorder.isTypeSupported.mockImplementation((t: string) => t === 'audio/mp4')
    expect(pickRecorderMimeType()).toBe('audio/mp4')
    MockMediaRecorder.isTypeSupported.mockImplementation((t: string) => t === 'audio/webm;codecs=opus')
  })

  it('records, previews and saves (upload with source=Recorded + durationSeconds)', async () => {
    const uploadAudio = vi.spyOn(attachmentsApi, 'uploadAudio').mockResolvedValue(fakeResult)
    const onSaved = vi.fn()
    render(
      <AudioRecorder
        onSave={(file, durationSeconds, onProgress) =>
          attachmentsApi.uploadAudio('notes', 'note-1', file, { source: 'Recorded', durationSeconds, onProgress })
        }
        onSaved={onSaved}
      />,
    )

    fireEvent.click(screen.getByRole('button', { name: 'Start recording' }))
    const stopBtn = await screen.findByRole('button', { name: 'Stop' })
    expect(getUserMedia).toHaveBeenCalledWith({ audio: true })
    expect(MockMediaRecorder.instances).toHaveLength(1)
    expect(MockMediaRecorder.instances[0].mimeType).toBe('audio/webm;codecs=opus')
    expect(MockMediaRecorder.instances[0].start).toHaveBeenCalled()
    expect(screen.getByText('Recording')).toBeInTheDocument()
    expect(screen.getByRole('meter', { name: 'Input level' })).toBeInTheDocument()

    // Pause / resume.
    fireEvent.click(screen.getByRole('button', { name: 'Pause' }))
    expect(await screen.findByText('Paused')).toBeInTheDocument()
    fireEvent.click(screen.getByRole('button', { name: 'Resume' }))
    expect(await screen.findByText('Recording')).toBeInTheDocument()

    fireEvent.click(stopBtn)
    const preview = await screen.findByTestId('recorder-preview')
    expect(preview).toHaveAttribute('src', 'blob:preview')
    expect(trackStop).toHaveBeenCalled()

    fireEvent.click(screen.getByRole('button', { name: 'Save recording' }))
    await waitFor(() => expect(uploadAudio).toHaveBeenCalledTimes(1))
    const [kind, id, file, opts] = uploadAudio.mock.calls[0]
    expect(kind).toBe('notes')
    expect(id).toBe('note-1')
    expect(file).toBeInstanceOf(File)
    expect((file as File).type).toBe('audio/webm')
    expect((file as File).name).toMatch(/^recording-.*\.webm$/)
    expect(opts.source).toBe('Recorded')
    expect(typeof opts.durationSeconds).toBe('number')
    expect(typeof opts.onProgress).toBe('function')
    await waitFor(() => expect(onSaved).toHaveBeenCalled())
    // Back to idle after a successful save.
    expect(await screen.findByRole('button', { name: 'Start recording' })).toBeInTheDocument()
  })

  it('discard drops the recording without uploading', async () => {
    const onSave = vi.fn().mockResolvedValue(undefined)
    render(<AudioRecorder onSave={onSave} />)
    fireEvent.click(screen.getByRole('button', { name: 'Start recording' }))
    fireEvent.click(await screen.findByRole('button', { name: 'Stop' }))
    await screen.findByTestId('recorder-preview')
    fireEvent.click(screen.getByRole('button', { name: 'Discard' }))
    expect(await screen.findByRole('button', { name: 'Start recording' })).toBeInTheDocument()
    expect(onSave).not.toHaveBeenCalled()
    expect(URL.revokeObjectURL).toHaveBeenCalledWith('blob:preview')
  })

  it('shows a clear error when microphone permission is denied', async () => {
    getUserMedia.mockRejectedValue(new DOMException('Permission denied', 'NotAllowedError'))
    const onSave = vi.fn()
    render(<AudioRecorder onSave={onSave} />)
    fireEvent.click(screen.getByRole('button', { name: 'Start recording' }))
    const alert = await screen.findByRole('alert')
    expect(alert).toHaveTextContent(/Microphone access was denied/)
    expect(screen.getByRole('button', { name: 'Try again' })).toBeInTheDocument()
    expect(MockMediaRecorder.instances).toHaveLength(0)
    expect(onSave).not.toHaveBeenCalled()
  })

  it('shows an unsupported message when MediaRecorder is missing', async () => {
    vi.stubGlobal('MediaRecorder', undefined)
    render(<AudioRecorder onSave={vi.fn()} />)
    fireEvent.click(screen.getByRole('button', { name: 'Start recording' }))
    expect(await screen.findByRole('alert')).toHaveTextContent(/isn't supported in this browser/)
    expect(getUserMedia).not.toHaveBeenCalled()
  })

  it('explains that an http:// LAN page needs https instead of blaming the browser', async () => {
    vi.stubGlobal('isSecureContext', false)
    render(<AudioRecorder onSave={vi.fn()} />)
    fireEvent.click(screen.getByRole('button', { name: 'Start recording' }))
    const alert = await screen.findByRole('alert')
    expect(alert).toHaveTextContent(/only works on a secure page/)
    expect(alert).not.toHaveTextContent(/isn't supported/)
    expect(screen.queryByRole('button', { name: 'Try again' })).not.toBeInTheDocument()
    expect(getUserMedia).not.toHaveBeenCalled()
  })
})
