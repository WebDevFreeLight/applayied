import { act, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { setToken } from '@/api/client'
import type { AudioDto } from '@/api/types'
import { AudioPlayer } from './AudioPlayer'

const ws = vi.hoisted(() => ({
  handlers: {} as Record<string, (arg?: unknown) => void>,
  setTime: vi.fn(),
  play: vi.fn(() => Promise.resolve()),
  playPause: vi.fn(() => Promise.resolve()),
  getDuration: vi.fn(() => 30),
  getCurrentTime: vi.fn(() => 0),
  destroy: vi.fn(),
}))

vi.mock('wavesurfer.js', () => ({
  default: {
    create: vi.fn(() => ({
      on: (event: string, cb: (arg?: unknown) => void) => {
        ws.handlers[event] = cb
      },
      setTime: ws.setTime,
      play: ws.play,
      playPause: ws.playPause,
      getDuration: ws.getDuration,
      getCurrentTime: ws.getCurrentTime,
      destroy: ws.destroy,
    })),
  },
}))

const segments = [
  { start: 0, end: 5, text: 'Hello everyone', speaker: null },
  { start: 5, end: 10, text: 'مرحبا بكم', speaker: 'S1' },
  { start: 10, end: 15, text: 'Goodbye', speaker: null },
]

function clip(overrides: Partial<AudioDto> = {}): AudioDto {
  return {
    id: 'a1',
    ownerType: 'Note',
    ownerId: 'n1',
    source: 'Uploaded',
    filePath: 'audio/2026/11/a1.mp4',
    mimeType: 'video/mp4',
    durationSeconds: 15,
    sizeBytes: 1234,
    transcript: 'Hello everyone مرحبا بكم Goodbye',
    transcriptLanguage: 'en',
    voiceName: null,
    ttsEngine: null,
    text: null,
    isAiGenerated: false,
    mediaKind: 'Video',
    segments,
    createdAt: '2026-11-07T10:00:00Z',
    ...overrides,
  }
}

let play: ReturnType<typeof vi.fn>

beforeEach(() => {
  setToken('tok')
  play = vi.fn(() => Promise.resolve())
  Object.defineProperty(HTMLMediaElement.prototype, 'play', { configurable: true, value: play })
  Object.defineProperty(HTMLMediaElement.prototype, 'pause', { configurable: true, value: vi.fn() })
  ws.setTime.mockClear()
  ws.play.mockClear()
})

afterEach(() => setToken(null))

describe('AudioPlayer — video + transcript', () => {
  it('renders a <video> with a Video badge instead of the waveform', () => {
    render(<AudioPlayer audio={clip()} />)
    const video = screen.getByTestId('clip-video')
    expect(video.tagName).toBe('VIDEO')
    expect(video).toHaveAttribute('src', 'http://localhost:5080/api/files/audio/2026/11/a1.mp4?access_token=tok')
    expect(video).toHaveAttribute('preload', 'metadata')
    expect(video).toHaveAttribute('controls')
    expect(screen.getByText('Video')).toBeInTheDocument()
    expect(screen.queryByRole('slider')).not.toBeInTheDocument()
  })

  it('clicking a segment seeks the video and plays; the active segment is highlighted', () => {
    render(<AudioPlayer audio={clip()} defaultShowTranscript />)
    const video = screen.getByTestId('clip-video') as HTMLVideoElement
    let time = 0
    Object.defineProperty(video, 'currentTime', {
      configurable: true,
      get: () => time,
      set: (v: number) => {
        time = v
      },
    })

    const list = screen.getByRole('list', { name: 'Transcript segments' })
    expect(list.querySelectorAll('li')).toHaveLength(3)
    expect(screen.getByText('0:05')).toBeInTheDocument()

    fireEvent.click(screen.getByRole('button', { name: /Play from 0:05/ }))
    expect(time).toBe(5)
    expect(play).toHaveBeenCalled()

    // Simulate playback at 6s → the 5–10s segment is active.
    act(() => {
      time = 6
      fireEvent.play(video)
      fireEvent.timeUpdate(video)
    })
    expect(screen.getByRole('button', { name: /Play from 0:05/ })).toHaveAttribute('aria-current', 'true')
    expect(screen.getByRole('button', { name: /Play from 0:00/ })).not.toHaveAttribute('aria-current')

    act(() => {
      time = 11
      fireEvent.timeUpdate(video)
    })
    expect(screen.getByRole('button', { name: /Play from 0:10/ })).toHaveAttribute('aria-current', 'true')
  })

  it('offers TXT / SRT / VTT downloads of the transcript', () => {
    render(<AudioPlayer audio={clip()} defaultShowTranscript />)
    for (const f of ['txt', 'srt', 'vtt']) {
      const link = screen.getByRole('link', { name: `Download transcript as ${f.toUpperCase()}` })
      expect(link).toHaveAttribute('href', `http://localhost:5080/api/audio/a1/transcript?format=${f}&access_token=tok`)
      expect(link).toHaveAttribute('download')
    }
    expect(screen.getByRole('button', { name: 'Copy transcript' })).toBeInTheDocument()
  })

  it('transcript is collapsed by default and expands on demand', () => {
    render(<AudioPlayer audio={clip()} />)
    expect(screen.queryByRole('list', { name: 'Transcript segments' })).not.toBeInTheDocument()
    fireEvent.click(screen.getByRole('button', { name: 'Show transcript' }))
    expect(screen.getByRole('list', { name: 'Transcript segments' })).toBeInTheDocument()
  })

  it('for audio clips, clicking a segment seeks the waveform and plays', () => {
    render(
      <AudioPlayer
        audio={clip({ mediaKind: 'Audio', mimeType: 'audio/webm', filePath: 'audio/2026/11/a1.webm' })}
        defaultShowTranscript
      />,
    )
    expect(screen.queryByTestId('clip-video')).not.toBeInTheDocument()
    fireEvent.click(screen.getByRole('button', { name: /Play from 0:10/ }))
    expect(ws.setTime).toHaveBeenCalledWith(10)
    expect(ws.play).toHaveBeenCalled()
  })

  it('shows the Transcribe button for video clips and displays new segments afterwards', async () => {
    const onTranscribe = vi.fn().mockResolvedValue(clip())
    render(<AudioPlayer audio={clip({ transcript: null, segments: [] })} onTranscribe={onTranscribe} />)
    expect(screen.queryByRole('button', { name: 'Show transcript' })).not.toBeInTheDocument()
    await act(async () => {
      fireEvent.click(screen.getByRole('button', { name: 'Transcribe' }))
    })
    expect(onTranscribe).toHaveBeenCalledWith(false)
    expect(screen.getByRole('list', { name: 'Transcript segments' })).toBeInTheDocument()
  })
})
