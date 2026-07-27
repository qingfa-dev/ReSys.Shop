import { describe, it, expect, vi } from 'vitest'
import apiClient from '@/common/api/http/api.client'
import { productRepository } from '../products/api/product.api'
import { variantRepository } from '../variants/api/variant.api'
import { optionTypeRepository } from '../option-types/api/option-type.api'
import { optionValueRepository } from '../option-values/api/option-value.api'
import { taxonomyRepository } from '../taxonomies/api/taxonomy.api'
import { taxonRepository } from '../taxa/api/taxon.api'
import type { CreateVariantRequest } from '../variants/models/variant.request'

vi.mock('@/common/api/http/api.client', () => ({
  default: {
    get: vi.fn<() => void>().mockResolvedValue({ data: {} }),
    post: vi.fn<() => void>().mockResolvedValue({ data: {} }),
    put: vi.fn<() => void>().mockResolvedValue({ data: {} }),
    patch: vi.fn<() => void>().mockResolvedValue({ data: {} }),
    delete: vi.fn<() => void>().mockResolvedValue({ data: {} }),
  }
}))

describe('productRepository', () => {
  it('list calls correct route', async () => {
    await productRepository.list({ page: 1, pageSize: 10 })
    expect(apiClient.get).toHaveBeenCalledWith('/catalog/products', expect.any(Object))
  })
  it('getById calls correct route', async () => {
    await productRepository.getById('guid-1')
    expect(apiClient.get).toHaveBeenCalledWith('/catalog/products/guid-1')
  })
  it('create calls correct route', async () => {
    await productRepository.create({ name: 'Test', slug: 'test', price: 0, trackInventory: true })
    expect(apiClient.post).toHaveBeenCalledWith('/catalog/products', expect.any(Object))
  })
  it('delete calls correct route', async () => {
    await productRepository.delete('guid-1')
    expect(apiClient.delete).toHaveBeenCalledWith('/catalog/products/guid-1')
  })
})

describe('variantRepository', () => {
  it('create calls correct route', async () => {
    await variantRepository.create('prod-1', { sku: 'TST', price: 10 } as unknown as CreateVariantRequest)
    expect(apiClient.post).toHaveBeenCalledWith('/catalog/products/prod-1/variants', expect.any(Object))
  })
  it('getById calls correct route', async () => {
    await variantRepository.getById('var-1')
    expect(apiClient.get).toHaveBeenCalledWith('/catalog/variants/var-1')
  })
})

describe('optionTypeRepository', () => {
  it('list calls correct route', async () => {
    await optionTypeRepository.list({ page: 1 })
    expect(apiClient.get).toHaveBeenCalledWith('/catalog/option-types', expect.any(Object))
  })
})

describe('optionValueRepository', () => {
  it('listByOptionTypeId calls correct nested route', async () => {
    await optionValueRepository.listByOptionTypeId('ot-1', { page: 1 })
    expect(apiClient.get).toHaveBeenCalledWith('/catalog/option-types/ot-1/values', expect.any(Object))
  })
})

describe('taxonomyRepository', () => {
  it('list calls correct route', async () => {
    await taxonomyRepository.list({ page: 1 })
    expect(apiClient.get).toHaveBeenCalledWith('/catalog/taxonomies', expect.any(Object))
  })
})

describe('taxonRepository', () => {
  it('listByTaxonomyId calls correct nested route', async () => {
    await taxonRepository.listByTaxonomyId('tax-1', { page: 1 })
    expect(apiClient.get).toHaveBeenCalledWith('/catalog/taxonomies/tax-1/taxons', expect.any(Object))
  })
})
