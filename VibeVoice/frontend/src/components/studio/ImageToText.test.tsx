import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { act, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { createMemoryRouter, RouterProvider } from 'react-router'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { imagesApi } from '@/api/library'
import { notesApi } from '@/api/notes'
import type { AudioDto, ImageDto, NoteDto, OcrResultDto, UserDto, VoiceStatus } from '@/api/types'
import { voiceApi } from '@/api/voice'
import { AuthContext, type AuthContextValue } from '@/auth/context'
import { ImageToText } from './ImageToText'

vi.mock('wavesurfer.js', () => ({
  default: {
    create: () => ({ on: () => {}, destroy: () => {}, setTime: () => {}, play: () => Promise.resolve(), playPause: () => Promise.resolve() }),
  },
}))

const user = { id: 'u1', displayName: 'Admin', role: 'Admin', preferredVoice: null } as UserDto
const auth = { user, token: 't', isAdmin: true } as AuthContextValue

const status = (ocrEngine: string | null): VoiceStatus => ({
  available: true,
  ttsEngine: 'vibevoice',
  asrEngine: 'whisper',
  ocrEngine,
  device: 'cpu',
  message: null,
})

const savedImage = { id: 'img9', ownerType: 'Library', ownerId: 'u1' } as ImageDto
const ocrResult: OcrResultDto = {
  text: 'فاتورة رقم 12\nTotal: 42 SAR',
  language: 'ar',
  lines: [
    { text: 'فاتورة رقم 12', confidence: 0.98 },
    { text: 'Total: 42 SAR', confidence: 0.87 },
  ],
  image: savedImage,
}

function renderTab() {
  const qc = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  const router = createMemoryRouter(
    [
      { path: '/studio', element: <ImageToText /> },
      { path: '/notes/:id', element: <h1>Note page</h1> },
    ],
    { initialEntries: ['/studio'] },
  )
  render(
    <QueryClientProvider client={qc}>
      <AuthContext.Provider value={auth}>
        <RouterProvider router={router} />
      </AuthContext.Provider>
    </QueryClientProvider>,
  )
  return router
}

const picture = () => new File(['png'], 'receipt.png', { type: 'image/png' })

beforeEach(() => {
  URL.createObjectURL = vi.fn(() => 'blob:preview')
  URL.revokeObjectURL = vi.fn()
  vi.spyOn(voiceApi, 'voices').mockResolvedValue([
    { id: 'en-Carter', name: 'Carter', language: 'en', gender: 'male', engine: 'vibevoice', isFallback: false },
  ])
})

describe('Studio — Image → text', () => {
  it('picture → text: preview, lines with confidence, full text, then save as note (+ attach image)', async () => {
    vi.spyOn(voiceApi, 'status').mockResolvedValue(status('rapidocr'))
    const ocr = vi.spyOn(voiceApi, 'ocr').mockResolvedValue(ocrResult)
    const create = vi.spyOn(notesApi, 'create').mockResolvedValue({ id: 'n42' } as NoteDto)
    const attach = vi.spyOn(imagesApi, 'attach').mockResolvedValue(savedImage)
    const router = renderTab()
    await waitFor(() => expect(voiceApi.status).toHaveBeenCalled())

    fireEvent.change(screen.getByLabelText('Text language'), { target: { value: 'ar' } })
    const file = picture()
    fireEvent.change(screen.getByTestId('ocr-input'), { target: { files: [file] } })

    expect(await screen.findByRole('img', { name: 'Picture' })).toHaveAttribute('src', 'blob:preview')
    await waitFor(() => expect(ocr).toHaveBeenCalledTimes(1))
    expect(ocr).toHaveBeenCalledWith(file, { language: 'ar', save: true }, expect.any(Function))

    const lines = await screen.findByRole('list', { name: 'Lines' })
    expect(lines).toHaveTextContent('فاتورة رقم 12')
    expect(lines).toHaveTextContent('98%')
    expect(lines).toHaveTextContent('87%')
    expect(screen.getByTestId('ocr-text')).toHaveTextContent('Total: 42 SAR')
    expect(screen.getByRole('button', { name: 'Copy text' })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Download .txt' })).toBeInTheDocument()

    await act(async () => {
      fireEvent.click(screen.getByRole('button', { name: 'Save as note' }))
    })
    await waitFor(() => expect(create).toHaveBeenCalledTimes(1))
    const body = create.mock.calls[0][0]
    expect(body.title).toBe('فاتورة رقم 12')
    expect(body.body).toContain('Total: 42 SAR')
    expect(attach).toHaveBeenCalledWith('img9', 'Note', 'n42')
    await waitFor(() => expect(router.state.location.pathname).toBe('/notes/n42'))
  })

  it('"Keep in Images library" off sends save=false and Save as note skips attaching', async () => {
    vi.spyOn(voiceApi, 'status').mockResolvedValue(status('rapidocr'))
    const ocr = vi.spyOn(voiceApi, 'ocr').mockResolvedValue({ ...ocrResult, image: null })
    vi.spyOn(notesApi, 'create').mockResolvedValue({ id: 'n43' } as NoteDto)
    const attach = vi.spyOn(imagesApi, 'attach')
    renderTab()
    await waitFor(() => expect(voiceApi.status).toHaveBeenCalled())
    fireEvent.click(screen.getByRole('checkbox', { name: 'Keep in Images library' }))
    fireEvent.change(screen.getByTestId('ocr-input'), { target: { files: [picture()] } })
    await waitFor(() => expect(ocr).toHaveBeenCalledWith(expect.any(File), { language: undefined, save: false }, expect.any(Function)))
    await act(async () => {
      fireEvent.click(await screen.findByRole('button', { name: 'Save as note' }))
    })
    await waitFor(() => expect(notesApi.create).toHaveBeenCalled())
    expect(attach).not.toHaveBeenCalled()
  })

  it('reads the extracted text aloud with VibeVoice (picture → text → speech)', async () => {
    vi.spyOn(voiceApi, 'status').mockResolvedValue(status('rapidocr'))
    vi.spyOn(voiceApi, 'ocr').mockResolvedValue(ocrResult)
    const tts = vi.spyOn(voiceApi, 'tts').mockResolvedValue({
      id: 'tts1',
      source: 'Tts',
      isAiGenerated: true,
      mediaKind: 'Audio',
      segments: [],
      filePath: 'tts/x.wav',
      mimeType: 'audio/wav',
      sizeBytes: 10,
      voiceName: 'Carter',
      createdAt: '2026-11-07T10:00:00Z',
    } as unknown as AudioDto)
    renderTab()
    await waitFor(() => expect(voiceApi.status).toHaveBeenCalled())
    fireEvent.change(screen.getByTestId('ocr-input'), { target: { files: [picture()] } })
    fireEvent.click(await screen.findByRole('button', { name: 'Read aloud with VibeVoice' }))
    await act(async () => {
      fireEvent.click(await screen.findByRole('button', { name: 'Generate speech' }))
    })
    await waitFor(() => expect(tts).toHaveBeenCalledWith({ text: ocrResult.text, voice: 'en-Carter', language: undefined }))
    expect(await screen.findByText('AI-generated')).toBeInTheDocument()
  })

  it('rejects non-image files with a visible error', async () => {
    vi.spyOn(voiceApi, 'status').mockResolvedValue(status('rapidocr'))
    const { toast } = await import('sonner')
    const error = vi.spyOn(toast, 'error').mockImplementation(() => 1)
    const ocr = vi.spyOn(voiceApi, 'ocr')
    renderTab()
    fireEvent.change(screen.getByTestId('ocr-input'), { target: { files: [new File(['x'], 'doc.pdf', { type: 'application/pdf' })] } })
    await waitFor(() => expect(error).toHaveBeenCalled())
    expect(ocr).not.toHaveBeenCalled()
  })

  it('is disabled with an explanation when the voice service has no OCR engine', async () => {
    vi.spyOn(voiceApi, 'status').mockResolvedValue(status(null))
    const ocr = vi.spyOn(voiceApi, 'ocr')
    renderTab()
    expect(await screen.findByRole('alert')).toHaveTextContent('Text extraction is unavailable')
    fireEvent.change(screen.getByTestId('ocr-input'), { target: { files: [picture()] } })
    expect(await screen.findByRole('button', { name: 'Extract text' })).toBeDisabled()
    expect(ocr).not.toHaveBeenCalled()
  })
})
