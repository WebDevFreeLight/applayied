import { act, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { ImageUploader } from './ImageUploader'

const png = () => new File(['png-bytes'], 'photo.png', { type: 'image/png' })
const jpg = () => new File(['jpg-bytes'], 'beach.jpg', { type: 'image/jpeg' })

function drop(files: File[]) {
  fireEvent.drop(screen.getByTestId('image-dropzone'), { dataTransfer: { files, types: ['Files'] } })
}

describe('ImageUploader', () => {
  it('uploads every dropped image (one request per file)', async () => {
    const upload = vi.fn().mockResolvedValue([])
    const onUploaded = vi.fn()
    render(<ImageUploader upload={upload} onUploaded={onUploaded} enablePaste={false} />)
    const a = png()
    const b = jpg()
    drop([a, b])
    await waitFor(() => expect(upload).toHaveBeenCalledTimes(2))
    expect(upload.mock.calls[0][0]).toBe(a)
    expect(upload.mock.calls[1][0]).toBe(b)
    expect(typeof upload.mock.calls[0][1]).toBe('function')
    await waitFor(() => expect(onUploaded).toHaveBeenCalledWith(2))
  })

  it('rejects files of the wrong type without uploading them', async () => {
    const upload = vi.fn().mockResolvedValue([])
    render(<ImageUploader upload={upload} enablePaste={false} />)
    drop([new File(['hello'], 'notes.txt', { type: 'text/plain' }), new File(['%PDF'], 'doc.pdf', { type: 'application/pdf' })])
    const alert = await screen.findByRole('alert')
    expect(alert).toHaveTextContent('notes.txt')
    expect(alert).toHaveTextContent('doc.pdf')
    expect(upload).not.toHaveBeenCalled()
  })

  it('uploads the valid files of a mixed drop and reports the rejected ones', async () => {
    const upload = vi.fn().mockResolvedValue([])
    render(<ImageUploader upload={upload} enablePaste={false} />)
    const good = png()
    drop([good, new File(['x'], 'clip.mp3', { type: 'audio/mpeg' })])
    expect(await screen.findByRole('alert')).toHaveTextContent('clip.mp3')
    await waitFor(() => expect(upload).toHaveBeenCalledTimes(1))
    expect(upload.mock.calls[0][0]).toBe(good)
  })

  it('rejects images larger than 10 MB', async () => {
    const upload = vi.fn()
    render(<ImageUploader upload={upload} enablePaste={false} />)
    const big = new File(['x'], 'huge.png', { type: 'image/png' })
    Object.defineProperty(big, 'size', { value: 11 * 1024 * 1024 })
    drop([big])
    expect(await screen.findByRole('alert')).toHaveTextContent('huge.png')
    expect(upload).not.toHaveBeenCalled()
  })

  it('shows per-file progress while uploading', async () => {
    let report: (f: number) => void = () => {}
    let finish: () => void = () => {}
    const upload = vi.fn(
      (_file: File, onProgress: (f: number) => void) =>
        new Promise<void>((resolve) => {
          report = onProgress
          finish = resolve
        }),
    )
    render(<ImageUploader upload={upload} enablePaste={false} />)
    drop([png()])
    await waitFor(() => expect(upload).toHaveBeenCalled())
    act(() => report(0.5))
    expect(screen.getByRole('progressbar', { name: 'photo.png' })).toHaveAttribute('aria-valuenow', '50')
    await act(async () => finish())
  })

  it('uploads images picked with the file input', async () => {
    const upload = vi.fn().mockResolvedValue([])
    render(<ImageUploader upload={upload} enablePaste={false} />)
    const file = jpg()
    fireEvent.change(screen.getByTestId('image-input'), { target: { files: [file] } })
    await waitFor(() => expect(upload).toHaveBeenCalledWith(file, expect.any(Function)))
  })

  it('uploads images pasted from the clipboard', async () => {
    const upload = vi.fn().mockResolvedValue([])
    render(<ImageUploader upload={upload} />)
    const file = png()
    const event = new Event('paste', { bubbles: true, cancelable: true })
    Object.defineProperty(event, 'clipboardData', { value: { files: [file] } })
    act(() => {
      window.dispatchEvent(event)
    })
    await waitFor(() => expect(upload).toHaveBeenCalledWith(file, expect.any(Function)))
    expect(event.defaultPrevented).toBe(true)
  })
})
