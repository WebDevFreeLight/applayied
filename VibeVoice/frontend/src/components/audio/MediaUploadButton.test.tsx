import { act, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { toast } from 'sonner'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { attachmentsApi } from '@/api/attachments'
import { FakeXHR } from '@/test/fakeXhr'
import { MediaUploadButton } from './MediaUploadButton'

function renderButton(onUploaded = vi.fn()) {
  render(
    <MediaUploadButton
      upload={(file, opts, onProgress) =>
        attachmentsApi.uploadAudio('notes', 'n1', file, {
          source: 'Uploaded',
          transcribe: opts.transcribe,
          language: opts.language,
          onProgress,
        })
      }
      onUploaded={onUploaded}
    />,
  )
  return onUploaded
}

function pick(file: File) {
  fireEvent.change(screen.getByTestId('media-input'), { target: { files: [file] } })
}

const video = () => new File(['\x00\x00\x00\x18ftypmp42'], 'trip.mp4', { type: 'video/mp4' })

let success: ReturnType<typeof vi.spyOn>
let warning: ReturnType<typeof vi.spyOn>
let error: ReturnType<typeof vi.spyOn>

beforeEach(() => {
  FakeXHR.instances = []
  vi.stubGlobal('XMLHttpRequest', FakeXHR)
  success = vi.spyOn(toast, 'success').mockImplementation(() => 1)
  warning = vi.spyOn(toast, 'warning').mockImplementation(() => 1)
  error = vi.spyOn(toast, 'error').mockImplementation(() => 1)
})

afterEach(() => {
  vi.unstubAllGlobals()
})

describe('MediaUploadButton', () => {
  it('is labelled "Upload audio / video" and offers auto-transcribe (on by default)', () => {
    renderButton()
    fireEvent.click(screen.getByRole('button', { name: 'Upload audio / video' }))
    expect(screen.getByRole('checkbox', { name: 'Auto-transcribe' })).toBeChecked()
    expect(screen.getByLabelText('Spoken language')).toHaveValue('auto')
    expect(screen.getByTestId('media-input')).toHaveAttribute('accept', expect.stringContaining('video/*'))
  })

  it('accepts a video file and sends transcribe=true + language', async () => {
    const onUploaded = renderButton()
    fireEvent.click(screen.getByRole('button', { name: 'Upload audio / video' }))
    fireEvent.change(screen.getByLabelText('Spoken language'), { target: { value: 'ar' } })
    const file = video()
    pick(file)

    await waitFor(() => expect(FakeXHR.instances).toHaveLength(1))
    const xhr = FakeXHR.last
    expect(xhr.url).toBe('http://localhost:5080/api/notes/n1/audio')
    const form = xhr.body as FormData
    expect(form.get('file')).toBeInstanceOf(File)
    expect((form.get('file') as File).name).toBe('trip.mp4')
    expect(form.get('source')).toBe('Uploaded')
    expect(form.get('transcribe')).toBe('true')
    expect(form.get('language')).toBe('ar')

    // Upload progress is visible while sending.
    act(() => xhr.upload.onprogress?.({ lengthComputable: true, loaded: 40, total: 100 }))
    expect(screen.getByText('Uploading… 40%')).toBeInTheDocument()

    await act(async () => xhr.respond(200, { id: 'a1', mediaKind: 'Video', segments: [] }))
    await waitFor(() => expect(onUploaded).toHaveBeenCalled())
    expect(success).toHaveBeenCalledWith('Uploaded and transcribed')
    expect(warning).not.toHaveBeenCalled()
  })

  it('omits the transcribe fields when auto-transcribe is turned off', async () => {
    renderButton()
    fireEvent.click(screen.getByRole('button', { name: 'Upload audio / video' }))
    fireEvent.click(screen.getByRole('checkbox', { name: 'Auto-transcribe' }))
    pick(new File(['ID3'], 'memo.mp3', { type: 'audio/mpeg' }))
    await waitFor(() => expect(FakeXHR.instances).toHaveLength(1))
    const form = FakeXHR.last.body as FormData
    expect(form.get('transcribe')).toBeNull()
    expect(form.get('language')).toBeNull()
    await act(async () => FakeXHR.last.respond(200, { id: 'a2' }))
  })

  it('shows a warning toast when X-Transcription-Error is returned (clip still saved)', async () => {
    const onUploaded = renderButton()
    pick(video())
    await waitFor(() => expect(FakeXHR.instances).toHaveLength(1))
    await act(async () =>
      FakeXHR.last.respond(200, { id: 'a3' }, { 'X-Transcription-Error': 'ASR engine is disabled' }),
    )
    await waitFor(() => expect(warning).toHaveBeenCalledTimes(1))
    expect(String(warning.mock.calls[0][0])).toContain('ASR engine is disabled')
    expect(onUploaded).toHaveBeenCalled()
    expect(success).not.toHaveBeenCalled()
  })

  it('rejects an oversize video with a visible reason and no request', async () => {
    renderButton()
    const big = video()
    Object.defineProperty(big, 'size', { value: 600 * 1024 * 1024 })
    pick(big)
    await waitFor(() => expect(error).toHaveBeenCalledTimes(1))
    expect(String(error.mock.calls[0][0])).toMatch(/trip\.mp4 is too large — video files can be up to 500 MB/)
    expect(FakeXHR.instances).toHaveLength(0)
  })

  it('rejects an oversize audio file (50 MB limit) and unsupported files with a toast', async () => {
    renderButton()
    const bigAudio = new File(['x'], 'long.wav', { type: 'audio/wav' })
    Object.defineProperty(bigAudio, 'size', { value: 60 * 1024 * 1024 })
    pick(bigAudio)
    await waitFor(() => expect(error).toHaveBeenCalledTimes(1))
    expect(String(error.mock.calls[0][0])).toContain('50 MB')

    pick(new File(['hello'], 'notes.txt', { type: 'text/plain' }))
    await waitFor(() => expect(error).toHaveBeenCalledTimes(2))
    expect(String(error.mock.calls[1][0])).toContain("notes.txt isn't a supported audio or video file")
    expect(FakeXHR.instances).toHaveLength(0)
  })

  it('accepts video by extension when the browser gives no MIME type', async () => {
    renderButton()
    pick(new File(['x'], 'clip.MOV', { type: '' }))
    await waitFor(() => expect(FakeXHR.instances).toHaveLength(1))
    await act(async () => FakeXHR.last.respond(200, { id: 'a4' }))
    expect(error).not.toHaveBeenCalled()
  })

  it('shows an error toast when the upload fails', async () => {
    renderButton()
    pick(video())
    await waitFor(() => expect(FakeXHR.instances).toHaveLength(1))
    await act(async () => FakeXHR.last.respond(400, { title: 'Invalid file', status: 400, detail: 'Unsupported video codec.' }))
    await waitFor(() => expect(error).toHaveBeenCalledWith('Unsupported video codec.'))
  })
})
