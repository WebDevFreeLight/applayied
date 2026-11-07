import { useId } from 'react'

/** SVG gradient ids must be valid URL fragments — strip React's special characters from useId(). */
export function useSvgId(prefix: string): string {
  return `${prefix}-${useId().replace(/[^a-zA-Z0-9_-]/g, '')}`
}
