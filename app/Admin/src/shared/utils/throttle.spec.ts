import { describe, it, expect, vi, afterEach } from 'vitest'
import { throttle } from './throttle'

describe('throttle', () => {
  afterEach(() => {
    vi.restoreAllMocks()
  })

  it('calls the function immediately on first invocation', () => {
    const fn = vi.fn()
    const throttled = throttle(fn, 100)
    throttled()
    expect(fn).toHaveBeenCalledOnce()
  })

  it('blocks subsequent calls within the limit window', () => {
    vi.useFakeTimers()
    const fn = vi.fn()
    const throttled = throttle(fn, 100)
    throttled()
    throttled()
    throttled()
    expect(fn).toHaveBeenCalledOnce()
    vi.useRealTimers()
  })

  it('allows call after the limit window expires', () => {
    vi.useFakeTimers()
    const fn = vi.fn()
    const throttled = throttle(fn, 100)
    throttled()
    vi.advanceTimersByTime(100)
    throttled()
    expect(fn).toHaveBeenCalledTimes(2)
    vi.useRealTimers()
  })

  it('passes arguments to the original function', () => {
    const fn = vi.fn()
    const throttled = throttle(fn, 100)
    throttled('a', 1)
    expect(fn).toHaveBeenCalledWith('a', 1)
  })
})
