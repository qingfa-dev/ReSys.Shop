import { describe, it, expect, vi, beforeEach } from 'vitest'
import { emit, on, off, reset } from '../useStoreEvents'

describe('useStoreEvents', () => {
  beforeEach(() => {
    reset()
  })

  it('resolves only after async handlers complete', async () => {
    const order: string[] = []
    on('test:async', async () => {
      await Promise.resolve()
      order.push('handler')
    })

    await emit({ type: 'test:async' })
    order.push('emit-resolved')

    expect(order).toEqual(['handler', 'emit-resolved'])
  })

  it('awaits all async handlers even when one is slow', async () => {
    const order: string[] = []
    const gate = new Promise<void>(r => setTimeout(r, 5))
    on('test:multi', async () => {
      await gate
      order.push('slow')
    })
    on('test:multi', () => {
      order.push('sync')
    })

    await emit({ type: 'test:multi' })
    order.push('emit-resolved')

    expect(order).toEqual(['sync', 'slow', 'emit-resolved'])
  })

  it('still invokes synchronous handlers', () => {
    const called: string[] = []
    on('test:sync', () => {
      called.push('sync')
    })
    void emit({ type: 'test:sync' })
    expect(called).toEqual(['sync'])
  })

  it('removes handlers via off()', () => {
    const handler = vi.fn<() => void>()
    on('test:off', handler)
    off('test:off', handler)
    void emit({ type: 'test:off' })
    expect(handler).not.toHaveBeenCalled()
  })
})
