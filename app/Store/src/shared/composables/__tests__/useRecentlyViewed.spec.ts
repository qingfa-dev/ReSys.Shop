import { describe, it, expect, beforeEach, vi } from 'vitest'

const mockItem = {
  productId: 'p1',
  productName: 'Test Product',
  slug: 'test-product',
  thumbnailUrl: null,
  minPrice: 100000,
  viewedAt: Date.now(),
}

describe('useRecentlyViewed', () => {
  beforeEach(() => {
    localStorage.clear()
    vi.resetModules()
  })

  it('starts empty', async () => {
    const { useRecentlyViewed } = await import('@/shared/composables/useRecentlyViewed')
    const { items } = useRecentlyViewed()
    expect(items.value).toEqual([])
  })

  it('adds an item', async () => {
    const { useRecentlyViewed } = await import('@/shared/composables/useRecentlyViewed')
    const { items, add } = useRecentlyViewed()
    add({ ...mockItem })
    expect(items.value).toHaveLength(1)
    expect(items.value[0]?.productId).toBe('p1')
  })

  it('deduplicates by productId', async () => {
    const { useRecentlyViewed } = await import('@/shared/composables/useRecentlyViewed')
    const { items, add } = useRecentlyViewed()
    add({ ...mockItem })
    add({ ...mockItem, productName: 'Updated' })
    expect(items.value).toHaveLength(1)
    expect(items.value[0]?.productName).toBe('Updated')
  })

  it('evicts oldest when maxItems reached', async () => {
    const { useRecentlyViewed } = await import('@/shared/composables/useRecentlyViewed')
    const { items, add } = useRecentlyViewed(3)
    add({ ...mockItem, productId: 'p1', viewedAt: 1000 })
    add({ ...mockItem, productId: 'p2', viewedAt: 2000 })
    add({ ...mockItem, productId: 'p3', viewedAt: 3000 })
    add({ ...mockItem, productId: 'p4', viewedAt: 4000 })
    expect(items.value).toHaveLength(3)
    expect(items.value.map(i => i.productId)).toEqual(['p2', 'p3', 'p4'])
  })

  it('clear removes all items', async () => {
    const { useRecentlyViewed } = await import('@/shared/composables/useRecentlyViewed')
    const { items, add, clear } = useRecentlyViewed()
    add({ ...mockItem })
    clear()
    expect(items.value).toEqual([])
  })
})