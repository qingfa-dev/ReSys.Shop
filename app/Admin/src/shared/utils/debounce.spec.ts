import { describe, it, expect, vi, afterEach } from 'vitest'
import { debounce } from './debounce'

describe('debounce', () => {
  afterEach(() => {
    vi.restoreAllMocks()
  })

  it('calls the function after the wait period', async () => {
    vi.useFakeTimers()
    const fn = vi.fn<(...args: unknown[]) => unknown>()
    const debounced = debounce(fn, 50)
    debounced()
    expect(fn).not.toHaveBeenCalled()
    vi.advanceTimersByTime(60)
    expect(fn).toHaveBeenCalledOnce()
    vi.useRealTimers()
  })

  it('debounces multiple rapid calls', async () => {
    vi.useFakeTimers()
    const fn = vi.fn<(...args: unknown[]) => unknown>()
    const debounced = debounce(fn, 100)
    debounced()
    debounced()
    debounced()
    expect(fn).not.toHaveBeenCalled()
    vi.advanceTimersByTime(50)
    debounced()
    vi.advanceTimersByTime(100)
    expect(fn).toHaveBeenCalledOnce()
    vi.useRealTimers()
  })

  it('passes arguments to the original function', async () => {
    vi.useFakeTimers()
    const fn = vi.fn<(...args: unknown[]) => unknown>()
    const debounced = debounce(fn, 50)
    debounced('a', 1)
    vi.advanceTimersByTime(60)
    expect(fn).toHaveBeenCalledWith('a', 1)
    vi.useRealTimers()
  })
})
