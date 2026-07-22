import { describe, it, expect, vi, beforeEach } from 'vitest'
import {
  getOptionTypes, getOptionType, createOptionType, updateOptionType, deleteOptionType,
  getOptionValues, createOptionValue, updateOptionValue, deleteOptionValue,
} from '../optionTypes'

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

describe('optionTypes API', () => {
  beforeEach(() => { vi.clearAllMocks() })

  it('getOptionTypes calls GET /catalog/option-types', async () => {
    mockGet.mockResolvedValue({ data: pagedEmpty })
    await getOptionTypes({ page: 1 })
    expect(mockGet).toHaveBeenCalledWith('/catalog/option-types', { params: { page: 1 } })
  })

  it('getOptionType calls GET /catalog/option-types/:id', async () => {
    mockGet.mockResolvedValue({ data: singleOk({ id: '1', name: 'Size' }) })
    await getOptionType('1')
    expect(mockGet).toHaveBeenCalledWith('/catalog/option-types/1')
  })

  it('createOptionType calls POST /catalog/option-types', async () => {
    mockPost.mockResolvedValue({ data: singleOk({ id: 'new', name: 'Color' }) })
    await createOptionType({ name: 'Color' })
    expect(mockPost).toHaveBeenCalledWith('/catalog/option-types', { name: 'Color' })
  })

  it('updateOptionType calls PUT /catalog/option-types/:id', async () => {
    mockPut.mockResolvedValue({ data: singleOk({ id: '1', name: 'Updated' }) })
    await updateOptionType('1', { name: 'Updated' })
    expect(mockPut).toHaveBeenCalledWith('/catalog/option-types/1', { name: 'Updated' })
  })

  it('deleteOptionType calls DELETE /catalog/option-types/:id', async () => {
    mockDelete.mockResolvedValue({ data: { isSuccess: true, statusCode: 200 } })
    await deleteOptionType('1')
    expect(mockDelete).toHaveBeenCalledWith('/catalog/option-types/1')
  })

  it('getOptionValues calls GET /catalog/option-types/:id/values', async () => {
    mockGet.mockResolvedValue({ data: singleOk([]) })
    await getOptionValues('1')
    expect(mockGet).toHaveBeenCalledWith('/catalog/option-types/1/values')
  })

  it('createOptionValue calls POST /catalog/option-types/:id/values', async () => {
    mockPost.mockResolvedValue({ data: singleOk({ id: 'new', name: 'Red' }) })
    await createOptionValue('1', { name: 'Red' })
    expect(mockPost).toHaveBeenCalledWith('/catalog/option-types/1/values', { name: 'Red' })
  })

  it('updateOptionValue calls PUT with type and value ids', async () => {
    mockPut.mockResolvedValue({ data: singleOk({ id: '2', name: 'Blue' }) })
    await updateOptionValue('1', '2', { name: 'Blue' })
    expect(mockPut).toHaveBeenCalledWith('/catalog/option-types/1/values/2', { name: 'Blue' })
  })

  it('deleteOptionValue calls DELETE with type and value ids', async () => {
    mockDelete.mockResolvedValue({ data: { isSuccess: true, statusCode: 200 } })
    await deleteOptionValue('1', '2')
    expect(mockDelete).toHaveBeenCalledWith('/catalog/option-types/1/values/2')
  })
})
