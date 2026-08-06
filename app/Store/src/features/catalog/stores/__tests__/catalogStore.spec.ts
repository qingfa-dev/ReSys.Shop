import { describe, it, expect, vi, beforeEach } from 'vitest'
import { createTestingPinia } from '@pinia/testing'
import { setActivePinia } from 'pinia'
import { useCatalogStore } from '../catalogStore'

describe('catalogStore', () => {
  beforeEach(() => {
    setActivePinia(createTestingPinia({ stubActions: false, createSpy: vi.fn }))
  })

  it('toggleOptionValue adds and removes a value', () => {
    const store = useCatalogStore()

    store.toggleOptionValue('o-1')
    expect(store.selectedOptionValueIds).toEqual(['o-1'])

    store.toggleOptionValue('o-1')
    expect(store.selectedOptionValueIds).toEqual([])
  })

  it('toggleOptionValue accumulates multiple values', () => {
    const store = useCatalogStore()

    store.toggleOptionValue('o-1')
    store.toggleOptionValue('o-2')
    store.toggleOptionValue('o-1')

    expect(store.selectedOptionValueIds).toEqual(['o-2'])
  })

  it('toggleTaxon adds and removes a taxon', () => {
    const store = useCatalogStore()

    store.toggleTaxon('t-1')
    expect(store.selectedTaxonIds).toEqual(['t-1'])

    store.toggleTaxon('t-1')
    expect(store.selectedTaxonIds).toEqual([])
  })

  it('toggleTaxon accumulates multiple taxons', () => {
    const store = useCatalogStore()

    store.toggleTaxon('t-1')
    store.toggleTaxon('t-2')
    store.toggleTaxon('t-1')

    expect(store.selectedTaxonIds).toEqual(['t-2'])
  })

  it('setPriceRange updates both bounds', () => {
    const store = useCatalogStore()

    store.setPriceRange(10000, 500000)
    expect(store.minPrice).toBe(10000)
    expect(store.maxPrice).toBe(500000)

    store.setPriceRange(null, null)
    expect(store.minPrice).toBeNull()
    expect(store.maxPrice).toBeNull()
  })

  it('setSearch updates the search query', () => {
    const store = useCatalogStore()

    store.setSearch('hex bolt')
    expect(store.searchQuery).toBe('hex bolt')
  })

  it('clearFilters resets every filter dimension', () => {
    const store = useCatalogStore()
    store.setSearch('hex bolt')
    store.toggleTaxon('t-1')
    store.toggleOptionValue('o-1')
    store.setPriceRange(10000, 500000)

    store.clearFilters()

    expect(store.searchQuery).toBe('')
    expect(store.selectedTaxonIds).toEqual([])
    expect(store.selectedOptionValueIds).toEqual([])
    expect(store.minPrice).toBeNull()
    expect(store.maxPrice).toBeNull()
  })

  it('clearFilters keeps sort state intact', () => {
    const store = useCatalogStore()
    store.sortField = 'price'
    store.sortOrder = -1
    store.toggleTaxon('t-1')

    store.clearFilters()

    expect(store.sortField).toBe('price')
    expect(store.sortOrder).toBe(-1)
  })
})
