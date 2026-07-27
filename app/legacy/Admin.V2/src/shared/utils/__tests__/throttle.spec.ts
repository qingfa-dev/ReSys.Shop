import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { throttle } from '../throttle'

describe('throttle', () => {
  beforeEach(() => { vi.useFakeTimers() })
  afterEach(() => { vi.useRealTimers() })

  it('calls function immediately on first call', () => {
    const fn = vi.fn()
    const throttled = throttle(fn, 300)
    throttled()
    expect(fn).toHaveBeenCalledTimes(1)
  })

  it('ignores subsequent calls within the limit window', () => {
    const fn = vi.fn()
    const throttled = throttle(fn, 300)
    throttled()
    throttled()
    throttled()
    expect(fn).toHaveBeenCalledTimes(1)
  })

  it('allows call after limit window expires', () => {
    const fn = vi.fn()
    const throttled = throttle(fn, 300)
    throttled()
    vi.advanceTimersByTime(300)
    throttled()
    expect(fn).toHaveBeenCalledTimes(2)
  })
})
