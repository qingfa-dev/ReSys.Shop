import { describe, it, expect, vi } from 'vitest'
import apiClient from '@/shared/api/http/api.client'
import { catalogApi } from '../services/catalog.api'

vi.mock('@/shared/api/http/api.client', () => ({
  default: {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    delete: vi.fn(),
  }
}))

describe('catalogApi.products', () => {
  it('list calls correct route', async () => {
    await catalogApi.products.list({ page: 1, pageSize: 10 })
    expect(apiClient.get).toHaveBeenCalledWith('api/catalog/products', expect.any(Object))
  })
  it('getById calls correct route', async () => {
    await catalogApi.products.getById('guid-1')
    expect(apiClient.get).toHaveBeenCalledWith('api/catalog/products/guid-1')
  })
  it('create calls correct route', async () => {
    await catalogApi.products.create({ name: 'Test', price: 10 })
    expect(apiClient.post).toHaveBeenCalledWith('api/catalog/products', expect.any(Object))
  })
  it('delete calls correct route', async () => {
    await catalogApi.products.delete('guid-1')
    expect(apiClient.delete).toHaveBeenCalledWith('api/catalog/products/guid-1')
  })
})

describe('catalogApi.variants', () => {
  it('create calls correct route', async () => {
    await catalogApi.variants.create('prod-1', { sku: 'TST', price: 10 } as any)
    expect(apiClient.post).toHaveBeenCalledWith('api/catalog/products/prod-1/variants', expect.any(Object))
  })
  it('getById calls correct route', async () => {
    await catalogApi.variants.getById('var-1')
    expect(apiClient.get).toHaveBeenCalledWith('api/catalog/products/variants/var-1')
  })
})
