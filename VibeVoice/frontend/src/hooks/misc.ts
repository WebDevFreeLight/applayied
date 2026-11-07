import { useCallback, useEffect, useLayoutEffect, useRef, useState } from 'react'

/** Returns `value` after it stopped changing for `delay` ms. */
export function useDebouncedValue<T>(value: T, delay = 300): T {
  const [debounced, setDebounced] = useState(value)
  useEffect(() => {
    const t = setTimeout(() => setDebounced(value), delay)
    return () => clearTimeout(t)
  }, [value, delay])
  return debounced
}

/** Keeps a ref pointing at the latest callback (safe to call from listeners). */
export function useLatest<T>(value: T) {
  const ref = useRef(value)
  useLayoutEffect(() => {
    ref.current = value
  })
  return ref
}

export interface HotkeyOptions {
  /** Require Ctrl (Windows/Linux) or ⌘ (macOS). */
  mod?: boolean
  shift?: boolean
  enabled?: boolean
  /** Also fire when focus is inside inputs / editors (default true for mod shortcuts). */
  allowInInputs?: boolean
}

/** Global keyboard shortcut, e.g. useHotkey('s', save, { mod: true }). */
export function useHotkey(key: string, handler: (e: KeyboardEvent) => void, options: HotkeyOptions = {}) {
  const { mod = false, shift = false, enabled = true, allowInInputs = mod } = options
  const handlerRef = useLatest(handler)
  useEffect(() => {
    if (!enabled) return
    const onKey = (e: KeyboardEvent) => {
      if (e.key.toLowerCase() !== key.toLowerCase()) return
      if (mod !== (e.ctrlKey || e.metaKey)) return
      if (shift !== e.shiftKey) return
      if (!allowInInputs) {
        const el = e.target as HTMLElement | null
        if (el && (el.isContentEditable || /^(INPUT|TEXTAREA|SELECT)$/.test(el.tagName))) return
      }
      e.preventDefault()
      handlerRef.current(e)
    }
    window.addEventListener('keydown', onKey)
    return () => window.removeEventListener('keydown', onKey)
  }, [key, mod, shift, enabled, allowInInputs, handlerRef])
}

/** Persisted state in localStorage (per-browser UI preferences only). */
export function useLocalStorageState<T>(key: string, initial: T): [T, (v: T) => void] {
  const [value, setValue] = useState<T>(() => {
    try {
      const raw = localStorage.getItem(key)
      return raw != null ? (JSON.parse(raw) as T) : initial
    } catch {
      return initial
    }
  })
  const set = useCallback(
    (v: T) => {
      setValue(v)
      try {
        localStorage.setItem(key, JSON.stringify(v))
      } catch {
        /* ignore */
      }
    },
    [key],
  )
  return [value, set]
}

export function useMediaQuery(query: string): boolean {
  const get = () =>
    typeof window !== 'undefined' && typeof window.matchMedia === 'function'
      ? window.matchMedia(query).matches
      : false
  const [matches, setMatches] = useState(get)
  useEffect(() => {
    if (typeof window.matchMedia !== 'function') return
    const mql = window.matchMedia(query)
    const onChange = () => setMatches(mql.matches)
    mql.addEventListener('change', onChange)
    return () => mql.removeEventListener('change', onChange)
  }, [query])
  return matches
}

/** Copies text to the clipboard; resolves to true on success. */
export async function copyText(text: string): Promise<boolean> {
  try {
    await navigator.clipboard.writeText(text)
    return true
  } catch {
    return false
  }
}
