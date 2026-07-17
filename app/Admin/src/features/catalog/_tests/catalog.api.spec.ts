import { describe, it, expect, vi } from 'vitest'
import apiClient from '@/shared/api/http/api.client'
import { productRepository } from '../repository/product.repository'
import { variantRepository } from '../repository/variant.repository'
import { optionTypeRepository } from '../repository/option-type.repository'
import { optionValueRepository } from '../repository/option-value.repository'
import { propertyTypeRepository } from '../repository/property-type.repository'
import { taxonomyRepository } from '../repository/taxonomy.repository'
import { taxonRepository } from '../repository/taxon.repository'

vi.mock('@/shared/api/http/api.client', () => ({
  default: {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    patch: vi.fn(),
    delete: vi.fn(),
  }
}))

describe('productRepository', () => {
  it('list calls correct route', async () => {
    await productRepository.list({ page: 1, pageSize: 10 })
    expect(apiClient.get).toHaveBeenCalledWith('api/catalog/products', expect.any(Object))
  })
  it('getById calls correct route', async () => {
    await productRepository.getById('guid-1')
    expect(apiClient.get).toHaveBeenCalledWith('api/catalog/products/guid-1', expect.any(Object))
  })
  it('create calls correct route', async () => {
    await productRepository.create({ name: 'Test', price: 10 })
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
    expect(apiClient.get).toHaveBeenCalledWith('api/catalog/products/variants/var-1', expect.any(Object))
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

describe('propertyTypeRepository', () => {
  it('list calls correct route', async () => {
    await propertyTypeRepository.list({ page: 1 })
    expect(apiClient.get).toHaveBeenCalledWith('api/catalog/property-types', expect.any(Object))
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
