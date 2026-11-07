import { useCallback, useEffect, useLayoutEffect, useRef } from 'react'
import { useBlocker, type Blocker } from 'react-router'

/**
 * Dirty-state guard: blocks in-app navigation (the caller renders a confirm dialog from the
 * returned blocker) and asks the browser to confirm reload / tab close.
 * `bypass()` lets the next navigation through (e.g. after deleting the item).
 */
export function useUnsavedGuard(isDirty: boolean): { blocker: Blocker; bypass: () => void } {
  const dirtyRef = useRef(isDirty)
  const bypassRef = useRef(false)
  useLayoutEffect(() => {
    dirtyRef.current = isDirty
  }, [isDirty])

  const blocker = useBlocker(
    ({ currentLocation, nextLocation }) =>
      dirtyRef.current && !bypassRef.current && currentLocation.pathname !== nextLocation.pathname,
  )

  useEffect(() => {
    if (!isDirty) return
    const onBeforeUnload = (e: BeforeUnloadEvent) => {
      e.preventDefault()
      // Legacy browsers need returnValue to be set.
      e.returnValue = ''
    }
    window.addEventListener('beforeunload', onBeforeUnload)
    return () => window.removeEventListener('beforeunload', onBeforeUnload)
  }, [isDirty])

  const bypass = useCallback(() => {
    bypassRef.current = true
  }, [])

  return { blocker, bypass }
}
