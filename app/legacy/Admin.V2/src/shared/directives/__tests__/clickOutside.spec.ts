import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import type { DirectiveBinding } from 'vue'
import { clickOutside } from '../clickOutside'

const dir = clickOutside as any

describe('clickOutside', () => {
  let el: HTMLElement
  let handler: ReturnType<typeof vi.fn>

  beforeEach(() => {
    el = document.createElement('div')
    handler = vi.fn()
    document.body.appendChild(el)
  })

  afterEach(() => {
    if (el.parentNode) el.parentNode.removeChild(el)
    vi.restoreAllMocks()
  })

  it('adds click listener on mounted', () => {
    const addSpy = vi.spyOn(document, 'addEventListener')
    dir.mounted(el, { value: handler, arg: '' } as DirectiveBinding)
    expect(addSpy).toHaveBeenCalledWith('click', expect.any(Function))
  })

  it('removes listener on unmounted', () => {
    const removeSpy = vi.spyOn(document, 'removeEventListener')
    dir.mounted(el, { value: handler, arg: '' } as DirectiveBinding)
    dir.unmounted(el)
    expect(removeSpy).toHaveBeenCalledWith('click', expect.any(Function))
  })

  it('does not call handler when clicking inside the element', () => {
    dir.mounted(el, { value: handler, arg: '' } as DirectiveBinding)
    el.dispatchEvent(new MouseEvent('click', { bubbles: true }))
    expect(handler).not.toHaveBeenCalled()
  })

  it('calls handler when clicking outside the element', () => {
    dir.mounted(el, { value: handler, arg: '' } as DirectiveBinding)
    document.body.dispatchEvent(new MouseEvent('click', { bubbles: true }))
    expect(handler).toHaveBeenCalledTimes(1)
  })

  it('does not throw when target is null', () => {
    dir.mounted(el, { value: handler, arg: '' } as DirectiveBinding)
    const event = new MouseEvent('click', { bubbles: true })
    Object.defineProperty(event, 'target', { value: null })
    expect(() => document.dispatchEvent(event)).not.toThrow()
  })

  it('skips handler when clicking ignored element via exceptSelectors', () => {
    const exceptHandler = vi.fn()
    dir.mounted(el, { value: exceptHandler, arg: '.ignore' } as DirectiveBinding)
    const ignoredEl = document.createElement('div')
    ignoredEl.className = 'ignore'
    document.body.appendChild(ignoredEl)
    vi.spyOn(ignoredEl, 'closest').mockReturnValue(ignoredEl)
    ignoredEl.dispatchEvent(new MouseEvent('click', { bubbles: true }))
    expect(exceptHandler).not.toHaveBeenCalled()
    document.body.removeChild(ignoredEl)
  })

  it('mounted with no arg works', () => {
    dir.mounted(el, { value: handler } as DirectiveBinding)
    document.body.dispatchEvent(new MouseEvent('click', { bubbles: true }))
    expect(handler).toHaveBeenCalledTimes(1)
  })
})
