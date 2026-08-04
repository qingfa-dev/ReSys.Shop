import { describe, it, expect, vi, beforeEach } from 'vitest'

const { mockPost, mockGet, mockPut, mockDel, mockGetPaged } = vi.hoisted(() => ({
  mockPost: vi.fn<(...args: unknown[]) => unknown>(),
  mockGet: vi.fn<(...args: unknown[]) => unknown>(),
  mockPut: vi.fn<(...args: unknown[]) => unknown>(),
  mockDel: vi.fn<(...args: unknown[]) => unknown>(),
  mockGetPaged: vi.fn<(...args: unknown[]) => unknown>(),
}))

vi.mock('@/shared/api/client', () => ({
  post: mockPost,
  get: mockGet,
  put: mockPut,
  del: mockDel,
}))

vi.mock('@/shared/api', () => ({
  getPaged: mockGetPaged,
}))

import { OptionTypeApi } from '../../services/optionTypeApi'

beforeEach(() => {
  vi.clearAllMocks()
})

describe('OptionTypeApi.getOptionTypes', () => {
  it('calls getPaged with option type query params', async () => {
    mockGetPaged.mockResolvedValue({
      items: [],
      page: 1,
      pageSize: 20,
      totalCount: 0,
      totalPages: 0,
      isSuccess: true,
      statusCode: 200,
      message: null,
      errors: [],
      metadata: null,
    })

    await OptionTypeApi.getOptionTypes({ filterable: true, page: 1, pageSize: 10 })

    expect(mockGetPaged).toHaveBeenCalledWith(
      'api/catalog/option-types',
      { filter: 'filterable=true', search: null, sort: null, pageNumber: 1, pageSize: 10 },
      expect.objectContaining({ allowedFilterFields: expect.any(Array) }),
    )
  })
})

describe('OptionTypeApi.getOptionType', () => {
  it('calls GET with correct URL', async () => {
    mockGet.mockResolvedValue({ value: { id: '1', name: 'Size' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })

    await OptionTypeApi.getOptionType('abc-123')
    expect(mockGet).toHaveBeenCalledWith('api/catalog/option-types/abc-123')
  })
})

describe('OptionTypeApi.createOptionType', () => {
  it('calls POST with request body', async () => {
    const req = { name: 'Size', presentation: 'Select a size', position: 1, filterable: true }
    mockPost.mockResolvedValue({ value: { id: '1', ...req }, isSuccess: true, statusCode: 201, message: null, errors: [], metadata: null })

    await OptionTypeApi.createOptionType(req)
    expect(mockPost).toHaveBeenCalledWith('api/catalog/option-types', req)
  })
})

describe('OptionTypeApi.updateOptionType', () => {
  it('calls PUT with request body', async () => {
    const req = { name: 'Size', presentation: 'Select a size', position: 1, filterable: false }
    mockPut.mockResolvedValue({ value: { id: '1', ...req }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })

    await OptionTypeApi.updateOptionType('abc-123', req)
    expect(mockPut).toHaveBeenCalledWith('api/catalog/option-types/abc-123', req)
  })
})

describe('OptionTypeApi.deleteOptionType', () => {
  it('calls DELETE with correct URL', async () => {
    mockDel.mockResolvedValue({ value: { id: '1', name: 'Size' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })

    await OptionTypeApi.deleteOptionType('abc-123')
    expect(mockDel).toHaveBeenCalledWith('api/catalog/option-types/abc-123')
  })
})
