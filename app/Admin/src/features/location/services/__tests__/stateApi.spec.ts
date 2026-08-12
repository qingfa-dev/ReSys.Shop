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

import { StateApi } from '../stateApi'

beforeEach(() => {
  vi.clearAllMocks()
})

describe('StateApi.getStates', () => {
  it('calls getPaged with state query and countryId filter', async () => {
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

    await StateApi.getStates({ filter: 'countryId=abc-123' })

    expect(mockGetPaged).toHaveBeenCalledWith(
      '/api/admin/location/states',
      expect.objectContaining({ filter: 'countryId=abc-123' }),
      expect.any(Object),
    )
  })
})

describe('StateApi.getState', () => {
  it('calls GET with correct URL', async () => {
    mockGet.mockResolvedValue({ value: { id: '1', name: 'California' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })

    await StateApi.getState('xyz-456')
    expect(mockGet).toHaveBeenCalledWith('/api/admin/location/states/xyz-456')
  })
})

describe('StateApi.getStateByIso', () => {
  it('calls GET with by-iso URL', async () => {
    mockGet.mockResolvedValue({ value: { id: '1', name: 'New York', abbreviation: 'NY' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })

    await StateApi.getStateByIso('NY')
    expect(mockGet).toHaveBeenCalledWith('/api/admin/location/states/by-iso/NY')
  })
})

describe('StateApi.createState', () => {
  it('calls POST with request body', async () => {
    const req = { name: 'Texas', abbreviation: 'TX', countryId: 'us-id', isActive: true }
    mockPost.mockResolvedValue({ value: { id: '1', ...req }, isSuccess: true, statusCode: 201, message: null, errors: [], metadata: null })

    await StateApi.createState(req)
    expect(mockPost).toHaveBeenCalledWith('/api/admin/location/states', req)
  })
})

describe('StateApi.updateState', () => {
  it('calls PUT with request body', async () => {
    const req = { name: 'Texas', abbreviation: 'TX', countryId: 'us-id', isActive: true }
    mockPut.mockResolvedValue({ value: { id: '1', ...req }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })

    await StateApi.updateState('xyz-456', req)
    expect(mockPut).toHaveBeenCalledWith('/api/admin/location/states/xyz-456', req)
  })
})

describe('StateApi.deleteState', () => {
  it('calls DELETE with correct URL', async () => {
    mockDel.mockResolvedValue({ value: { id: '1', name: 'Texas' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })

    await StateApi.deleteState('xyz-456')
    expect(mockDel).toHaveBeenCalledWith('/api/admin/location/states/xyz-456')
  })
})
