import '@testing-library/jest-dom/vitest'
import { cleanup } from '@testing-library/react'
import { afterEach } from 'vitest'
import '../i18n'

afterEach(() => {
  cleanup()
})

// jsdom lacks layout APIs that ProseMirror (TipTap) calls when scrolling the selection into view.
const emptyRects = () => ({ length: 0, item: () => null, [Symbol.iterator]: [][Symbol.iterator] }) as unknown as DOMRectList
const zeroRect = () => ({ x: 0, y: 0, top: 0, left: 0, right: 0, bottom: 0, width: 0, height: 0, toJSON: () => ({}) }) as DOMRect
if (typeof Range !== 'undefined') {
  Range.prototype.getClientRects ??= emptyRects
  Range.prototype.getBoundingClientRect ??= zeroRect
}
Element.prototype.getClientRects ??= emptyRects
Element.prototype.scrollIntoView ??= () => {}
document.elementFromPoint ??= () => null
