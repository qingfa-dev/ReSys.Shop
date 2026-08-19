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

import { OptionValueApi } from '../../services/optionValueApi'

beforeEach(() => {
  vi.clearAllMocks()
})

describe('OptionValueApi.getOptionValues', () => {
  it('calls getPaged with optionTypeId filter', async () => {
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

    await OptionValueApi.getOptionValues({ filter: 'optionTypeId=abc-123' })

    expect(mockGetPaged).toHaveBeenCalledWith(
      '/api/admin/catalog/option-values',
      expect.objectContaining({ filter: 'optionTypeId=abc-123' }),
      expect.any(Object),
    )
  })
})

describe('OptionValueApi.getOptionValue', () => {
  it('calls GET with correct URL', async () => {
    mockGet.mockResolvedValue({ value: { id: '1', name: 'Medium' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })

    await OptionValueApi.getOptionValue('xyz-456')
    expect(mockGet).toHaveBeenCalledWith('/api/admin/catalog/option-values/xyz-456')
  })
})

describe('OptionValueApi.createOptionValue', () => {
  it('calls POST with request body', async () => {
    const req = { optionTypeId: 'abc-123', name: 'Medium', presentation: 'Medium', position: 2 }
    mockPost.mockResolvedValue({ value: { id: '1', ...req }, isSuccess: true, statusCode: 201, message: null, errors: [], metadata: null })

    await OptionValueApi.createOptionValue(req)
    expect(mockPost).toHaveBeenCalledWith('/api/admin/catalog/option-values', req)
  })
})

describe('OptionValueApi.updateOptionValue', () => {
  it('calls PUT with request body', async () => {
    const req = { optionTypeId: 'abc-123', name: 'Large', presentation: 'Large', position: 3 }
    mockPut.mockResolvedValue({ value: { id: '1', ...req }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })

    await OptionValueApi.updateOptionValue('xyz-456', req)
    expect(mockPut).toHaveBeenCalledWith('/api/admin/catalog/option-values/xyz-456', req)
  })
})

describe('OptionValueApi.deleteOptionValue', () => {
  it('calls DELETE with correct URL', async () => {
    mockDel.mockResolvedValue({ value: { id: '1', name: 'Medium' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })

    await OptionValueApi.deleteOptionValue('xyz-456')
    expect(mockDel).toHaveBeenCalledWith('/api/admin/catalog/option-values/xyz-456')
  })
})
