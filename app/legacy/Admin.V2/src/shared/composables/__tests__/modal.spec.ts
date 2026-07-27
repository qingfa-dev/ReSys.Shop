import { describe, it, expect } from 'vitest'
import { useModalService } from '../useModal'

describe('useModalService', () => {
  it('initial state: closed, no data', () => {
    const { isOpen, modalData } = useModalService()
    expect(isOpen.value).toBe(false)
    expect(modalData.value).toBeNull()
  })

  it('open() sets isOpen to true', () => {
    const { isOpen, open } = useModalService()
    open()
    expect(isOpen.value).toBe(true)
  })

  it('open() sets modalData when provided', () => {
    const { modalData, open } = useModalService()
    open({ id: 1 })
    expect(modalData.value).toEqual({ id: 1 })
  })

  it('close() sets isOpen to false', () => {
    const { isOpen, open, close } = useModalService()
    open()
    close()
    expect(isOpen.value).toBe(false)
  })

  it('toggle() flips isOpen', () => {
    const { isOpen, toggle } = useModalService()
    expect(isOpen.value).toBe(false)
    toggle()
    expect(isOpen.value).toBe(true)
    toggle()
    expect(isOpen.value).toBe(false)
  })
})
