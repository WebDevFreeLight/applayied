import { act, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { useRef } from 'react'
import { describe, expect, it, vi } from 'vitest'
import type { ImageDto } from '@/api/types'
import { RichTextEditor, type RichTextHandle } from '@/components/editor/RichTextEditor'
import { textToHtml } from '@/lib/utils'
import { ImageGallery } from './ImageGallery'

function image(overrides: Partial<ImageDto> = {}): ImageDto {
  return {
    id: 'img1',
    ownerType: 'Note',
    ownerId: 'n1',
    filePath: 'images/2026/11/img1.jpg',
    thumbnailPath: 'images/2026/11/img1_thumb.jpg',
    mimeType: 'image/jpeg',
    width: 800,
    height: 600,
    sizeBytes: 1000,
    caption: null,
    sortOrder: 0,
    ocrText: null,
    ocrLanguage: null,
    createdAt: '2026-11-07T10:00:00Z',
    ...overrides,
  }
}

/** Mirrors the editor wiring: gallery "Insert into note" → TipTap appendHtml → onChange (autosave). */
function Harness({ images, onBodyChange }: { images: ImageDto[]; onBodyChange: (html: string) => void }) {
  const ref = useRef<RichTextHandle>(null)
  return (
    <>
      <RichTextEditor handleRef={ref} initialHtml="<p>Shopping list</p>" onChange={onBodyChange} label="Content" />
      <ImageGallery
        images={images}
        onInsertText={(text) => ref.current?.appendHtml(textToHtml(text))}
        insertLabel="Insert into note"
      />
    </>
  )
}

describe('ImageGallery — extracted text', () => {
  it('shows a Text badge and inserts the extracted text into the open editor', async () => {
    const onBodyChange = vi.fn()
    render(<Harness images={[image({ ocrText: 'حليب\nMilk 2L', ocrLanguage: 'ar' })]} onBodyChange={onBodyChange} />)

    expect(screen.getByText('Text')).toBeInTheDocument()
    fireEvent.click(screen.getByRole('button', { name: 'Show extracted text' }))
    expect(screen.getByTestId('ocr-text')).toHaveTextContent('حليب')
    expect(screen.getByTestId('ocr-text')).toHaveAttribute('dir', 'auto')
    expect(screen.getByRole('button', { name: 'Download .txt' })).toBeInTheDocument()

    await act(async () => {
      fireEvent.click(screen.getByRole('button', { name: 'Insert into note' }))
    })
    await waitFor(() => expect(onBodyChange).toHaveBeenCalled())
    const html = onBodyChange.mock.calls.at(-1)![0] as string
    expect(html).toContain('Shopping list')
    expect(html).toContain('حليب')
    expect(html).toContain('Milk 2L')
    expect(screen.getByRole('textbox', { name: 'Content' })).toHaveTextContent('Milk 2L')
  })

  it('"Extract text" calls the OCR handler and is disabled without an OCR engine', async () => {
    const onExtractText = vi.fn().mockResolvedValue(undefined)
    const { rerender } = render(<ImageGallery images={[image()]} onExtractText={onExtractText} />)
    await act(async () => {
      fireEvent.click(screen.getByRole('button', { name: 'Extract text' }))
    })
    expect(onExtractText).toHaveBeenCalledWith('img1')

    rerender(<ImageGallery images={[image()]} onExtractText={onExtractText} ocrAvailable={false} />)
    expect(screen.getByRole('button', { name: 'Extract text' })).toBeDisabled()
  })

  it('downloads the text as a .txt Blob', () => {
    const createObjectURL = vi.fn(() => 'blob:txt')
    URL.createObjectURL = createObjectURL
    URL.revokeObjectURL = vi.fn()
    const click = vi.spyOn(HTMLAnchorElement.prototype, 'click').mockImplementation(() => {})
    render(<ImageGallery images={[image({ ocrText: 'Receipt total 42' })]} />)
    fireEvent.click(screen.getByRole('button', { name: 'Show extracted text' }))
    fireEvent.click(screen.getByRole('button', { name: 'Download .txt' }))
    expect(createObjectURL).toHaveBeenCalledWith(expect.any(Blob))
    expect(click).toHaveBeenCalled()
  })
})
