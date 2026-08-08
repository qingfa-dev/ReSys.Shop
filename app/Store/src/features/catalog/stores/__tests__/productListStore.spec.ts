import { describe, it, expect, vi, beforeEach } from 'vitest'
import { createTestingPinia } from '@pinia/testing'
import { setActivePinia } from 'pinia'
import { ProductApi } from '../../services/productApi'
import { useProductListStore } from '../productListStore'
import { useCatalogStore } from '../catalogStore'
import { toProductQueryParams } from '../../types'
import { pagedOk } from '@/shared/types'

describe('productListStore', () => {
  beforeEach(() => {
    setActivePinia(createTestingPinia({ stubActions: false, createSpy: vi.fn }))
    vi.restoreAllMocks()
  })

  it('fetch forwards the catalog store active filters into the API request', async () => {
    const spy = vi.spyOn(ProductApi, 'getProducts').mockResolvedValue(pagedOk([], 1, 20, 0))
    const catalog = useCatalogStore()
    catalog.toggleTaxon('t-1')
    catalog.toggleOptionValue('o-1')
    catalog.setPriceRange(10000, 500000)

    const store = useProductListStore()
    await store.fetch()

    expect(spy).toHaveBeenCalledTimes(1)
    expect(spy.mock.calls[0]![0]).toMatchObject({
      pageNumber: 1,
      pageSize: 20,
      search: undefined,
      sort: ['-CreatedAtUtc'],
      taxonIds: ['t-1'],
      optionValueIds: ['o-1'],
      minPrice: 10000,
      maxPrice: 500000,
    })
  })

  it('fetch omits the dedicated filter params when none are active', async () => {
    const spy = vi.spyOn(ProductApi, 'getProducts').mockResolvedValue(pagedOk([], 1, 20, 0))

    const store = useProductListStore()
    await store.fetch()

    // Serialized params must not contain dedicated filter keys when nothing is selected
    const params = toProductQueryParams(spy.mock.calls[0]![0])
    expect(params).not.toHaveProperty('taxonId')
    expect(params).not.toHaveProperty('optionValueId')
    expect(params).not.toHaveProperty('minPrice')
    expect(params).not.toHaveProperty('maxPrice')
  })
})

describe('toProductQueryParams', () => {
  it('maps dedicated storefront filters to the backend query param names', () => {
    const params = toProductQueryParams({
      taxonIds: ['t-1', 't-2'],
      optionValueIds: ['o-1'],
      minPrice: 10000,
      maxPrice: 500000,
    })

    // Backend model: GetStorefrontProducts.Parameters — TaxonId[], OptionValueId[], MinPrice, MaxPrice
    expect(params.taxonId).toEqual(['t-1', 't-2'])
    expect(params.optionValueId).toEqual(['o-1'])
    expect(params.minPrice).toBe(10000)
    expect(params.maxPrice).toBe(500000)
  })

  it('omits unset filter params entirely', () => {
    const params = toProductQueryParams({ pageNumber: 1, pageSize: 20 })

    expect(params).toEqual({ pageNumber: 1, pageSize: 20 })
    expect(params).not.toHaveProperty('taxonId')
    expect(params).not.toHaveProperty('minPrice')
  })
})
