import { describe, it, expect, vi } from 'vitest'
import { clickOutside } from '../clickOutside'

describe('clickOutside', () => {
  it('adds click listener on mounted', () => {
    const addSpy = vi.spyOn(document, 'addEventListener')
    const el = document.createElement('div')
    const handler = vi.fn()
    clickOutside.mounted(el, { value: handler, arg: '' } as any)
    expect(addSpy).toHaveBeenCalledWith('click', expect.any(Function))
    addSpy.mockRestore()
  })

  it('removes listener on unmounted', () => {
    const removeSpy = vi.spyOn(document, 'removeEventListener')
    const el = document.createElement('div')
    const handler = vi.fn()
    clickOutside.mounted(el, { value: handler, arg: '' } as any)
    clickOutside.unmounted(el)
    expect(removeSpy).toHaveBeenCalledWith('click', expect.any(Function))
    removeSpy.mockRestore()
  })
})
