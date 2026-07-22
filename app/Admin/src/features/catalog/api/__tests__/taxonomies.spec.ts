import { describe, it, expect, vi, beforeEach } from 'vitest'
import {
  getTaxonomies, getTaxonomy, createTaxonomy, updateTaxonomy, deleteTaxonomy,
  getTaxons, createTaxon, updateTaxon, deleteTaxon,
} from '../taxonomies'

const mockGet = vi.fn<(...args: unknown[]) => unknown>()
const mockPost = vi.fn<(...args: unknown[]) => unknown>()
const mockPut = vi.fn<(...args: unknown[]) => unknown>()
const mockDelete = vi.fn<(...args: unknown[]) => unknown>()

vi.mock('@/shared/api/client', () => ({
  default: {
    get: (...args: unknown[]) => mockGet(...args),
    post: (...args: unknown[]) => mockPost(...args),
    put: (...args: unknown[]) => mockPut(...args),
    delete: (...args: unknown[]) => mockDelete(...args),
  },
}))

const pagedEmpty = { isSuccess: true, items: [], page: 1, pageSize: 20, totalCount: 0, statusCode: 200 }
const singleOk = (value: unknown) => ({ isSuccess: true, value, statusCode: 200 })

describe('taxonomies API', () => {
  beforeEach(() => { vi.clearAllMocks() })

  it('getTaxonomies calls GET /catalog/taxonomies', async () => {
    mockGet.mockResolvedValue({ data: pagedEmpty })
    await getTaxonomies({ page: 1 })
    expect(mockGet).toHaveBeenCalledWith('/catalog/taxonomies', { params: { page: 1 } })
  })

  it('getTaxonomy calls GET /catalog/taxonomies/:id', async () => {
    mockGet.mockResolvedValue({ data: singleOk({ id: '1', name: 'Test' }) })
    await getTaxonomy('1')
    expect(mockGet).toHaveBeenCalledWith('/catalog/taxonomies/1')
  })

  it('createTaxonomy calls POST /catalog/taxonomies', async () => {
    mockPost.mockResolvedValue({ data: singleOk({ id: 'new', name: 'New' }) })
    await createTaxonomy({ name: 'New' })
    expect(mockPost).toHaveBeenCalledWith('/catalog/taxonomies', { name: 'New' })
  })

  it('updateTaxonomy calls PUT /catalog/taxonomies/:id', async () => {
    mockPut.mockResolvedValue({ data: singleOk({ id: '1', name: 'Updated' }) })
    await updateTaxonomy('1', { name: 'Updated' })
    expect(mockPut).toHaveBeenCalledWith('/catalog/taxonomies/1', { name: 'Updated' })
  })

  it('deleteTaxonomy calls DELETE /catalog/taxonomies/:id', async () => {
    mockDelete.mockResolvedValue({ data: { isSuccess: true, statusCode: 200 } })
    await deleteTaxonomy('1')
    expect(mockDelete).toHaveBeenCalledWith('/catalog/taxonomies/1')
  })

  it('getTaxons calls GET /catalog/taxonomies/:id/taxons', async () => {
    mockGet.mockResolvedValue({ data: singleOk([]) })
    await getTaxons('1')
    expect(mockGet).toHaveBeenCalledWith('/catalog/taxonomies/1/taxons')
  })

  it('createTaxon calls POST /catalog/taxonomies/:id/taxons', async () => {
    mockPost.mockResolvedValue({ data: singleOk({ id: 'new', name: 'Child' }) })
    await createTaxon('1', { name: 'Child' })
    expect(mockPost).toHaveBeenCalledWith('/catalog/taxonomies/1/taxons', { name: 'Child' })
  })

  it('updateTaxon calls PUT with taxonomy and taxon ids', async () => {
    mockPut.mockResolvedValue({ data: singleOk({ id: '2', name: 'Updated' }) })
    await updateTaxon('1', '2', { name: 'Updated' })
    expect(mockPut).toHaveBeenCalledWith('/catalog/taxonomies/1/taxons/2', { name: 'Updated' })
  })

  it('deleteTaxon calls DELETE with taxonomy and taxon ids', async () => {
    mockDelete.mockResolvedValue({ data: { isSuccess: true, statusCode: 200 } })
    await deleteTaxon('1', '2')
    expect(mockDelete).toHaveBeenCalledWith('/catalog/taxonomies/1/taxons/2')
  })
})
