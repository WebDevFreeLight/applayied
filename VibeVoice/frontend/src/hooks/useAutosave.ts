import { useCallback, useEffect, useLayoutEffect, useRef, useState } from 'react'

export type AutosaveStatus = 'saved' | 'saving' | 'unsaved' | 'error'

export interface AutosaveOptions<T> {
  /** Debounce delay in ms (default 1500). */
  delay?: number
  /** When false, changes are tracked but not saved automatically (flush() still works). */
  enabled?: boolean
  /** Equality check between values (default: JSON comparison). */
  isEqual?: (a: T, b: T) => boolean
  onError?: (error: unknown) => void
}

export interface AutosaveResult {
  status: AutosaveStatus
  error: unknown
  lastSavedAt: Date | null
  /** Saves immediately (cancelling the debounce); resolves once the latest value is saved. */
  flush: () => Promise<void>
  /** True whenever the latest value isn't persisted yet. */
  isDirty: boolean
}

function jsonEqual<T>(a: T, b: T): boolean {
  return a === b || JSON.stringify(a) === JSON.stringify(b)
}

/**
 * Debounced autosave.
 * - The initial value is treated as already saved (nothing is saved on mount).
 * - Saves never overlap: changes made while a save is in flight are queued and the
 *   latest value is saved as soon as the in-flight save finishes.
 */
export function useAutosave<T>(
  value: T,
  saveFn: (value: T) => Promise<unknown>,
  options: AutosaveOptions<T> = {},
): AutosaveResult {
  const { delay = 1500, enabled = true, isEqual = jsonEqual, onError } = options

  const [savedValue, setSavedValue] = useState<T>(value)
  const [phase, setPhase] = useState<'idle' | 'saving' | 'error'>('idle')
  const [error, setError] = useState<unknown>(null)
  const [lastSavedAt, setLastSavedAt] = useState<Date | null>(null)

  const savedRef = useRef<T>(value)
  const latestRef = useRef<T>(value)
  const enabledRef = useRef(enabled)
  const saveFnRef = useRef(saveFn)
  const isEqualRef = useRef(isEqual)
  const onErrorRef = useRef(onError)
  const timerRef = useRef<ReturnType<typeof setTimeout> | null>(null)
  const inFlightRef = useRef<Promise<void> | null>(null)
  const queuedRef = useRef(false)

  useLayoutEffect(() => {
    saveFnRef.current = saveFn
    isEqualRef.current = isEqual
    onErrorRef.current = onError
  })

  const clearTimer = useCallback(() => {
    if (timerRef.current !== null) {
      clearTimeout(timerRef.current)
      timerRef.current = null
    }
  }, [])

  const run = useCallback((): Promise<void> => {
    clearTimer()
    if (inFlightRef.current) {
      // Never overlap: the in-flight job will pick up the latest value when it finishes.
      queuedRef.current = true
      return inFlightRef.current
    }
    if (isEqualRef.current(latestRef.current, savedRef.current)) return Promise.resolve()

    const job = (async () => {
      setPhase('saving')
      try {
        do {
          queuedRef.current = false
          const snapshot = latestRef.current
          if (isEqualRef.current(snapshot, savedRef.current)) break
          await saveFnRef.current(snapshot)
          savedRef.current = snapshot
          setSavedValue(snapshot)
        } while (queuedRef.current)
        setError(null)
        setLastSavedAt(new Date())
        setPhase('idle')
      } catch (err) {
        queuedRef.current = false
        setError(err)
        setPhase('error')
        onErrorRef.current?.(err)
        throw err
      } finally {
        inFlightRef.current = null
      }
    })()
    inFlightRef.current = job.catch(() => undefined)
    return job
  }, [clearTimer])

  // Schedule a debounced save whenever the value actually changes.
  useEffect(() => {
    const changed = !isEqualRef.current(value, latestRef.current)
    const enabledChanged = enabledRef.current !== enabled
    latestRef.current = value
    enabledRef.current = enabled
    if (!changed && !enabledChanged) return
    if (!enabled || isEqualRef.current(value, savedRef.current)) {
      clearTimer()
      return
    }
    clearTimer()
    timerRef.current = setTimeout(() => {
      timerRef.current = null
      run().catch(() => undefined)
    }, delay)
  }, [value, delay, enabled, run, clearTimer])

  useEffect(() => clearTimer, [clearTimer])

  const flush = useCallback(async () => {
    clearTimer()
    if (inFlightRef.current) {
      queuedRef.current = true
      await inFlightRef.current
    }
    if (!isEqualRef.current(latestRef.current, savedRef.current)) await run()
  }, [run, clearTimer])

  const dirty = !isEqual(value, savedValue)
  const status: AutosaveStatus =
    phase === 'saving' ? 'saving' : !dirty ? 'saved' : phase === 'error' ? 'error' : 'unsaved'

  return { status, error, lastSavedAt, flush, isDirty: dirty || phase === 'saving' }
}
