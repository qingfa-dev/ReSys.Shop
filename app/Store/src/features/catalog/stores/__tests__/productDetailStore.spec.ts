import { describe, it, expect, vi, beforeEach } from 'vitest'
import { createTestingPinia } from '@pinia/testing'
import { setActivePinia } from 'pinia'
import { flushPromises } from '@vue/test-utils'
import { ProductApi } from '../../services/productApi'
import { useProductDetailStore } from '../productDetailStore'
import { ok, pagedOk } from '@/shared/types'
import type { Result, PagedResult } from '@/shared/types'
import type { StoreProductDetailResponse, StoreProductListItemResponse } from '../../types'

type SimilarItem = StoreProductListItemResponse & { similarityScore: number }

// Fixture: Minimal product detail payload — the store only reads id and masterVariant.
function makeProduct(id: string, slug: string): StoreProductDetailResponse {
  return {
    id,
    masterVariantId: `${id}-mv`,
    name: `Product ${id}`,
    status: 'active',
    description: null,
    slug,
    styleCode: null,
    seasonName: null,
    materialComposition: null,
    careInstructions: null,
    fitNotes: null,
    department: null,
    genderTarget: null,
    variantsCount: 0,
    availableOn: null,
    masterVariant: null,
    variants: [],
    classifications: [],
  }
}

// Fixture: Minimal list item for the similar/related rails.
function makeListItem(id: string): StoreProductListItemResponse {
  return {
    id,
    masterVariantId: `${id}-mv`,
    name: `Item ${id}`,
    status: 'active',
    description: null,
    slug: id,
    styleCode: null,
    seasonName: null,
    materialComposition: null,
    careInstructions: null,
    fitNotes: null,
    department: null,
    genderTarget: null,
    variantsCount: 0,
    availableOn: null,
    masterVariant: null,
    classifications: [],
  }
}

// Deferred: Manually resolvable promise for controlling response ordering in tests.
function deferred<T>() {
  let resolve!: (value: T) => void
  const promise = new Promise<T>(r => (resolve = r))
  return { promise, resolve }
}

describe('productDetailStore', () => {
  beforeEach(() => {
    setActivePinia(createTestingPinia({ stubActions: false, createSpy: vi.fn }))
    vi.restoreAllMocks()
  })

  it('discards stale detail and rail responses after rapid A→B navigation', async () => {
    const slugA = deferred<Result<StoreProductDetailResponse>>()
    const slugB = deferred<Result<StoreProductDetailResponse>>()
    const railASim = deferred<PagedResult<SimilarItem>>()
    const railARel = deferred<PagedResult<StoreProductListItemResponse>>()
    const railBSim = deferred<PagedResult<SimilarItem>>()
    const railBRel = deferred<PagedResult<StoreProductListItemResponse>>()

    vi.spyOn(ProductApi, 'getProductBySlug')
      .mockReturnValueOnce(slugA.promise)
      .mockReturnValueOnce(slugB.promise)
    vi.spyOn(ProductApi, 'getSimilar')
      .mockReturnValueOnce(railASim.promise)
      .mockReturnValueOnce(railBSim.promise)
    vi.spyOn(ProductApi, 'getRelated')
      .mockReturnValueOnce(railARel.promise)
      .mockReturnValueOnce(railBRel.promise)

    const store = useProductDetailStore()

    // A resolves first: detail shown, A rails still pending
    const loadA = store.load('product-a')
    slugA.resolve(ok(makeProduct('p-a', 'product-a')))
    await loadA
    expect(store.product?.id).toBe('p-a')

    // B loads while A rails are in flight
    const loadB = store.load('product-b')
    slugB.resolve(ok(makeProduct('p-b', 'product-b')))
    await loadB
    railBSim.resolve(pagedOk([{ ...makeListItem('b-1'), similarityScore: 0.9 }], 1, 12, 1))
    railBRel.resolve(pagedOk([makeListItem('b-2')], 1, 12, 1))
    await flushPromises()
    expect(store.product?.id).toBe('p-b')
    expect(store.relatedProducts.map(i => i.id)).toEqual(['b-2'])
    expect(store.similarProducts.map(i => i.id)).toEqual(['b-1'])

    // A rails resolve late: stale responses must not overwrite B's rails
    railASim.resolve(pagedOk([{ ...makeListItem('a-1'), similarityScore: 0.9 }], 1, 12, 1))
    railARel.resolve(pagedOk([makeListItem('a-2')], 1, 12, 1))
    await flushPromises()

    expect(store.product?.id).toBe('p-b')
    expect(store.relatedProducts.map(i => i.id)).toEqual(['b-2'])
    expect(store.similarProducts.map(i => i.id)).toEqual(['b-1'])
  })

  it('keeps the current product when a stale detail response resolves late', async () => {
    const slugA = deferred<Result<StoreProductDetailResponse>>()
    const slugB = deferred<Result<StoreProductDetailResponse>>()

    vi.spyOn(ProductApi, 'getProductBySlug')
      .mockReturnValueOnce(slugA.promise)
      .mockReturnValueOnce(slugB.promise)
    vi.spyOn(ProductApi, 'getSimilar').mockResolvedValue(pagedOk([], 1, 12, 0))
    vi.spyOn(ProductApi, 'getRelated').mockResolvedValue(pagedOk([], 1, 12, 0))

    const store = useProductDetailStore()

    const loadA = store.load('product-a')
    const loadB = store.load('product-b')
    slugB.resolve(ok(makeProduct('p-b', 'product-b')))
    await loadB

    // A's detail resolves after B — must be discarded
    slugA.resolve(ok(makeProduct('p-a', 'product-a')))
    await loadA

    expect(store.product?.id).toBe('p-b')
    expect(store.loading).toBe(false)
  })
})
