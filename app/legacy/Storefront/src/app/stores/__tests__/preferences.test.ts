import { describe, it, expect } from 'vitest'

describe('stores exports', () => {
  it('should export usePreferencesStore', async () => {
    const { usePreferencesStore } = await import('../preferences')
    expect(usePreferencesStore).toBeDefined()
  })

  it('should export useUIStore', async () => {
    const { useUIStore } = await import('../ui')
    expect(useUIStore).toBeDefined()
  })
})
