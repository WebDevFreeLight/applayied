import { act, renderHook } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { useAutosave } from './useAutosave'

type Props = { value: string }

function setup(save: (v: string) => Promise<unknown>, onError?: (e: unknown) => void) {
  return renderHook(({ value }: Props) => useAutosave(value, save, { delay: 1500, onError }), {
    initialProps: { value: 'initial' },
  })
}

/** Advances fake timers and lets pending promises settle inside act(). */
async function advance(ms: number) {
  await act(async () => {
    await vi.advanceTimersByTimeAsync(ms)
  })
}

function deferred() {
  let resolve!: () => void
  let reject!: (e: unknown) => void
  const promise = new Promise<void>((res, rej) => {
    resolve = res
    reject = rej
  })
  return { promise, resolve, reject }
}

describe('useAutosave', () => {
  beforeEach(() => {
    vi.useFakeTimers()
  })
  afterEach(() => {
    vi.useRealTimers()
  })

  it('does not save the initial value', async () => {
    const save = vi.fn().mockResolvedValue(undefined)
    const { result } = setup(save)
    expect(result.current.status).toBe('saved')
    expect(result.current.isDirty).toBe(false)
    await advance(5000)
    expect(save).not.toHaveBeenCalled()
  })

  it('debounces changes and saves only the latest value', async () => {
    const save = vi.fn().mockResolvedValue(undefined)
    const { result, rerender } = setup(save)

    rerender({ value: 'a' })
    expect(result.current.status).toBe('unsaved')
    await advance(1000)
    rerender({ value: 'ab' })
    await advance(1000)
    rerender({ value: 'abc' })
    await advance(1400)
    expect(save).not.toHaveBeenCalled()

    await advance(200)
    expect(save).toHaveBeenCalledTimes(1)
    expect(save).toHaveBeenCalledWith('abc')
    expect(result.current.status).toBe('saved')
    expect(result.current.lastSavedAt).toBeInstanceOf(Date)
  })

  it('goes back to "saved" without saving when the value returns to the saved one', async () => {
    const save = vi.fn().mockResolvedValue(undefined)
    const { result, rerender } = setup(save)
    rerender({ value: 'changed' })
    expect(result.current.status).toBe('unsaved')
    rerender({ value: 'initial' })
    expect(result.current.status).toBe('saved')
    await advance(3000)
    expect(save).not.toHaveBeenCalled()
  })

  it('never runs overlapping saves and queues the latest value', async () => {
    let inFlight = 0
    let maxInFlight = 0
    const first = deferred()
    const save = vi.fn(async (_v: string) => {
      inFlight++
      maxInFlight = Math.max(maxInFlight, inFlight)
      try {
        if (save.mock.calls.length === 1) await first.promise
      } finally {
        inFlight--
      }
    })
    const { result, rerender } = setup(save)

    rerender({ value: 'v1' })
    await advance(1500)
    expect(save).toHaveBeenCalledTimes(1)
    expect(result.current.status).toBe('saving')

    // Edits while the first save is still running.
    rerender({ value: 'v2' })
    await advance(1500)
    rerender({ value: 'v3' })
    await advance(1500)
    expect(save).toHaveBeenCalledTimes(1)

    await act(async () => {
      first.resolve()
      await vi.advanceTimersByTimeAsync(0)
    })
    await advance(0)

    expect(save).toHaveBeenCalledTimes(2)
    expect(save).toHaveBeenNthCalledWith(1, 'v1')
    expect(save).toHaveBeenNthCalledWith(2, 'v3')
    expect(maxInFlight).toBe(1)
    expect(result.current.status).toBe('saved')
  })

  it('flush() saves immediately without waiting for the debounce', async () => {
    const save = vi.fn().mockResolvedValue(undefined)
    const { result, rerender } = setup(save)
    rerender({ value: 'now' })
    await act(async () => {
      await result.current.flush()
    })
    expect(save).toHaveBeenCalledTimes(1)
    expect(save).toHaveBeenCalledWith('now')
    expect(result.current.status).toBe('saved')

    // The cancelled debounce must not trigger a second save.
    await advance(3000)
    expect(save).toHaveBeenCalledTimes(1)
  })

  it('flush() is a no-op when nothing changed', async () => {
    const save = vi.fn().mockResolvedValue(undefined)
    const { result } = setup(save)
    await act(async () => {
      await result.current.flush()
    })
    expect(save).not.toHaveBeenCalled()
  })

  it('reports errors, keeps the change dirty and recovers on the next save', async () => {
    const boom = new Error('boom')
    const onError = vi.fn()
    const save = vi.fn().mockRejectedValueOnce(boom).mockResolvedValue(undefined)
    const { result, rerender } = setup(save, onError)

    rerender({ value: 'bad' })
    await advance(1500)
    expect(save).toHaveBeenCalledTimes(1)
    expect(result.current.status).toBe('error')
    expect(result.current.error).toBe(boom)
    expect(result.current.isDirty).toBe(true)
    expect(onError).toHaveBeenCalledWith(boom)

    rerender({ value: 'good' })
    await advance(1500)
    expect(save).toHaveBeenCalledTimes(2)
    expect(save).toHaveBeenLastCalledWith('good')
    expect(result.current.status).toBe('saved')
    expect(result.current.error).toBeNull()
  })

  it('flush() rejects when the save fails', async () => {
    const save = vi.fn().mockRejectedValue(new Error('nope'))
    const { result, rerender } = setup(save)
    rerender({ value: 'x' })
    await act(async () => {
      await expect(result.current.flush()).rejects.toThrow('nope')
    })
    expect(result.current.status).toBe('error')
  })
})
