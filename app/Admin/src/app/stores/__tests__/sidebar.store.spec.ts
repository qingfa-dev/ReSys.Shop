import { describe, it, expect, beforeEach } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import { useSidebarStore } from '../sidebar.store'

describe('sidebar.store', () => {
  beforeEach(() => setActivePinia(createPinia()))

  it('toggles collapsed', () => {
    const s = useSidebarStore()
    expect(s.collapsed).toBe(false)
    s.toggle()
    expect(s.collapsed).toBe(true)
  })
})
