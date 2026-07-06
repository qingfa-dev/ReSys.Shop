import { describe, it, expect, vi } from 'vitest'
import { ref, nextTick } from 'vue'
import { useDebouncedRef } from '../useDebouncedRef'

describe('useDebouncedRef', () => {
  it('emits after the delay', async () => {
    vi.useFakeTimers()
    const source = ref('a')
    const debounced = useDebouncedRef(source, 200)
    source.value = 'b'
    await nextTick()
    expect(debounced.value).toBe('a')
    await vi.advanceTimersByTimeAsync(200)
    expect(debounced.value).toBe('b')
    vi.useRealTimers()
  })
})

