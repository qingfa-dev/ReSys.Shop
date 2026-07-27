import { describe, it, expect, vi, beforeEach } from 'vitest'
import apiClient from '@/shared/api/client'
import { TaxonomyApi } from '../taxonomy.api'
import { TaxonApi } from '../taxon.api'

vi.mock('@/shared/api/client', () => ({
  default: {
    get: vi.fn<(...args: unknown[]) => unknown>(),
    post: vi.fn<(...args: unknown[]) => unknown>(),
    put: vi.fn<(...args: unknown[]) => unknown>(),
    delete: vi.fn<(...args: unknown[]) => unknown>(),
  },
}))

const pagedEmpty = { isSuccess: true, items: [], page: 1, pageSize: 20, totalCount: 0, statusCode: 200 }
const singleOk = (value: unknown) => ({ isSuccess: true, value, statusCode: 200 })
const defaultQuery = { page: 1, pageSize: 20, sort: [{ field: 'createdAt', direction: 'Descending' as const }] }

describe('TaxonomyApi', () => {
  beforeEach(() => { vi.clearAllMocks() })

  it('getMany: GET /catalog/taxonomies with serialized params', async () => {
    vi.mocked(apiClient.get).mockResolvedValue({ data: pagedEmpty })
    await TaxonomyApi.getMany(defaultQuery)
    expect(apiClient.get).toHaveBeenCalledWith('/catalog/taxonomies', {
      params: {
        'page.page': 1,
        'page.pageSize': 20,
        'sort.clauses[0].field': 'createdAt',
        'sort.clauses[0].direction': 'Descending',
      },
    })
  })

  it('get: GET /catalog/taxonomies/:id', async () => {
    vi.mocked(apiClient.get).mockResolvedValue({ data: singleOk({ id: '1', name: 'Cat' }) })
    await TaxonomyApi.get('1')
    expect(apiClient.get).toHaveBeenCalledWith('/catalog/taxonomies/1')
  })

  it('create: POST /catalog/taxonomies', async () => {
    vi.mocked(apiClient.post).mockResolvedValue({ data: singleOk({ id: 'new', name: 'New' }) })
    await TaxonomyApi.create({ name: 'New' })
    expect(apiClient.post).toHaveBeenCalledWith('/catalog/taxonomies', { name: 'New' })
  })

  it('update: PUT /catalog/taxonomies/:id', async () => {
    vi.mocked(apiClient.put).mockResolvedValue({ data: singleOk({ id: '1', name: 'Upd' }) })
    await TaxonomyApi.update('1', { name: 'Upd' })
    expect(apiClient.put).toHaveBeenCalledWith('/catalog/taxonomies/1', { name: 'Upd' })
  })

  it('delete: DELETE /catalog/taxonomies/:id', async () => {
    vi.mocked(apiClient.delete).mockResolvedValue({ data: { isSuccess: true, statusCode: 200 } })
    await TaxonomyApi.delete('1')
    expect(apiClient.delete).toHaveBeenCalledWith('/catalog/taxonomies/1')
  })

  it('getTaxons: GET .../taxonomies/:id/taxons', async () => {
    vi.mocked(apiClient.get).mockResolvedValue({ data: singleOk([]) })
    await TaxonApi.getMany('1')
    expect(apiClient.get).toHaveBeenCalledWith('/catalog/taxonomies/1/taxons')
  })

  it('createTaxon: POST .../taxonomies/:id/taxons', async () => {
    vi.mocked(apiClient.post).mockResolvedValue({ data: singleOk({ id: 't1', name: 'Child' }) })
    await TaxonApi.create('1', { name: 'Child' })
    expect(apiClient.post).toHaveBeenCalledWith('/catalog/taxonomies/1/taxons', { name: 'Child' })
  })

  it('updateTaxon: PUT with taxonomy+taxon ids', async () => {
    vi.mocked(apiClient.put).mockResolvedValue({ data: singleOk({ id: 't1', name: 'Upd' }) })
    await TaxonApi.update('1', 't1', { name: 'Upd' })
    expect(apiClient.put).toHaveBeenCalledWith('/catalog/taxonomies/1/taxons/t1', { name: 'Upd' })
  })

  it('deleteTaxon: DELETE with taxonomy+taxon ids', async () => {
    vi.mocked(apiClient.delete).mockResolvedValue({ data: { isSuccess: true, statusCode: 200 } })
    await TaxonApi.delete('1', 't1')
    expect(apiClient.delete).toHaveBeenCalledWith('/catalog/taxonomies/1/taxons/t1')
  })
})
