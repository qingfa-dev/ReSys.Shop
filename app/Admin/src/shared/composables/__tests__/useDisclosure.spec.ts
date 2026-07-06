import { describe, it, expect } from 'vitest'
import { useDisclosure } from '../useDisclosure'

describe('useDisclosure', () => {
  it('toggles open state', () => {
    const { isOpen, open, close, toggle } = useDisclosure()
    expect(isOpen.value).toBe(false)
    open()
    expect(isOpen.value).toBe(true)
    toggle()
    expect(isOpen.value).toBe(false)
    close()
    expect(isOpen.value).toBe(false)
  })
})
