import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { useDebounce } from '../useDebounce'

describe('useDebounce', () => {
  beforeEach(() => { vi.useFakeTimers() })
  afterEach(() => { vi.useRealTimers() })

  it('calls the function after the delay', () => {
    const fn = vi.fn()
    const { debounced } = useDebounce(fn, 300)
    debounced()
    expect(fn).not.toHaveBeenCalled()
    vi.advanceTimersByTime(300)
    expect(fn).toHaveBeenCalledTimes(1)
  })

  it('cancels previous pending call on rapid invocations', () => {
    const fn = vi.fn()
    const { debounced } = useDebounce(fn, 300)
    debounced()
    debounced()
    vi.advanceTimersByTime(300)
    expect(fn).toHaveBeenCalledTimes(1)
  })

  it('cancel() prevents pending call', () => {
    const fn = vi.fn()
    const { debounced, cancel } = useDebounce(fn, 300)
    debounced()
    cancel()
    vi.advanceTimersByTime(300)
    expect(fn).not.toHaveBeenCalled()
  })

  it('uses default 300ms delay', () => {
    const fn = vi.fn()
    const { debounced } = useDebounce(fn)
    debounced()
    vi.advanceTimersByTime(299)
    expect(fn).not.toHaveBeenCalled()
    vi.advanceTimersByTime(1)
    expect(fn).toHaveBeenCalled()
  })

  it('passes arguments to the wrapped function', () => {
    const fn = vi.fn()
    const { debounced } = useDebounce(fn, 100)
    debounced('a', 1)
    vi.advanceTimersByTime(100)
    expect(fn).toHaveBeenCalledWith('a', 1)
  })
})
