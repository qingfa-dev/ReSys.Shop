import { describe, it, expect, vi } from 'vitest'
import apiClient from '@/shared/api/http/api.client'
import { productRepository } from '../products/repositories/product.repository'
import { variantRepository } from '../products/repositories/variant.repository'
import { optionTypeRepository } from '../option-types/repositories/option-type.repository'
import { optionValueRepository } from '../option-types/option-values/repositories/option-value.repository'
import { taxonomyRepository } from '../taxonomies/repositories/taxonomy.repository'
import { taxonRepository } from '../taxonomies/taxa/repositories/taxon.repository'

vi.mock('@/shared/api/http/api.client', () => ({
  default: {
    get: vi.fn().mockResolvedValue({ data: {} }),
    post: vi.fn().mockResolvedValue({ data: {} }),
    put: vi.fn().mockResolvedValue({ data: {} }),
    patch: vi.fn().mockResolvedValue({ data: {} }),
    delete: vi.fn().mockResolvedValue({ data: {} }),
  }
}))

describe('productRepository', () => {
  it('list calls correct route', async () => {
    await productRepository.list({ page: 1, pageSize: 10 })
    expect(apiClient.get).toHaveBeenCalledWith('api/catalog/products', expect.any(Object))
  })
  it('getById calls correct route', async () => {
    await productRepository.getById('guid-1')
    expect(apiClient.get).toHaveBeenCalledWith('api/catalog/products/guid-1')
  })
  it('create calls correct route', async () => {
    await productRepository.create({ name: 'Test', slug: 'test', price: 10, trackInventory: true })
    expect(apiClient.post).toHaveBeenCalledWith('api/catalog/products', expect.any(Object))
  })
  it('delete calls correct route', async () => {
    await productRepository.delete('guid-1')
    expect(apiClient.delete).toHaveBeenCalledWith('api/catalog/products/guid-1')
  })
})

describe('variantRepository', () => {
  it('create calls correct route', async () => {
    await variantRepository.create('prod-1', { sku: 'TST', price: 10 } as any)
    expect(apiClient.post).toHaveBeenCalledWith('api/catalog/products/prod-1/variants', expect.any(Object))
  })
  it('getById calls correct route', async () => {
    await variantRepository.getById('var-1')
    expect(apiClient.get).toHaveBeenCalledWith('api/catalog/products/variants/var-1')
  })
})

describe('optionTypeRepository', () => {
  it('list calls correct route', async () => {
    await optionTypeRepository.list({ page: 1 })
    expect(apiClient.get).toHaveBeenCalledWith('api/catalog/option-types', expect.any(Object))
  })
})

describe('optionValueRepository', () => {
  it('listByOptionTypeId calls correct nested route', async () => {
    await optionValueRepository.listByOptionTypeId('ot-1', { page: 1 })
    expect(apiClient.get).toHaveBeenCalledWith('api/catalog/option-types/ot-1/values', expect.any(Object))
  })
})

describe('taxonomyRepository', () => {
  it('list calls correct route', async () => {
    await taxonomyRepository.list({ page: 1 })
    expect(apiClient.get).toHaveBeenCalledWith('api/catalog/taxonomies', expect.any(Object))
  })
})

describe('taxonRepository', () => {
  it('listByTaxonomyId calls correct nested route', async () => {
    await taxonRepository.listByTaxonomyId('tax-1', { page: 1 })
    expect(apiClient.get).toHaveBeenCalledWith('api/catalog/taxonomies/tax-1/taxons', expect.any(Object))
  })
})
