import { describe, it, expect, vi, beforeEach } from 'vitest'
import apiClient from '@/shared/api/client'
import { TaxonApi } from '../taxon.api'

vi.mock('@/shared/api/client', () => ({
  default: {
    get: vi.fn<(...args: unknown[]) => unknown>(),
    post: vi.fn<(...args: unknown[]) => unknown>(),
    put: vi.fn<(...args: unknown[]) => unknown>(),
    delete: vi.fn<(...args: unknown[]) => unknown>(),
  },
}))

const singleOk = (value: unknown) => ({ isSuccess: true, value, statusCode: 200 })
const listOk = (value: unknown[]) => ({ isSuccess: true, value, statusCode: 200 })

describe('TaxonApi', () => {
  beforeEach(() => { vi.clearAllMocks() })

  it('getMany: GET /catalog/taxonomies/:id/taxons', async () => {
    vi.mocked(apiClient.get).mockResolvedValue({ data: listOk([{ id: '1', name: 'T1' }]) })
    await TaxonApi.getMany('tax1')
    expect(apiClient.get).toHaveBeenCalledWith('/catalog/taxonomies/tax1/taxons')
  })

  it('create: POST /catalog/taxonomies/:id/taxons', async () => {
    vi.mocked(apiClient.post).mockResolvedValue({ data: singleOk({ id: 'new', name: 'New' }) })
    await TaxonApi.create('tax1', { name: 'New', slug: 'new', taxonomyId: 'tax1' })
    expect(apiClient.post).toHaveBeenCalledWith('/catalog/taxonomies/tax1/taxons', { name: 'New', slug: 'new', taxonomyId: 'tax1' })
  })

  it('update: PUT /catalog/taxonomies/:taxonomyId/taxons/:id', async () => {
    vi.mocked(apiClient.put).mockResolvedValue({ data: singleOk({ id: 't1', name: 'Updated' }) })
    await TaxonApi.update('tax1', 't1', { name: 'Updated', slug: 'updated', taxonomyId: 'tax1' })
    expect(apiClient.put).toHaveBeenCalledWith('/catalog/taxonomies/tax1/taxons/t1', { name: 'Updated', slug: 'updated', taxonomyId: 'tax1' })
  })

  it('delete: DELETE /catalog/taxonomies/:taxonomyId/taxons/:id', async () => {
    vi.mocked(apiClient.delete).mockResolvedValue({ data: { isSuccess: true, statusCode: 200 } })
    await TaxonApi.delete('tax1', 't1')
    expect(apiClient.delete).toHaveBeenCalledWith('/catalog/taxonomies/tax1/taxons/t1')
  })
})
