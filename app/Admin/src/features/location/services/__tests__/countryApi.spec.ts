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

import { CountryApi } from '../countryApi'

beforeEach(() => {
  vi.clearAllMocks()
})

describe('CountryApi.getCountries', () => {
  it('calls getPaged with country query params', async () => {
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

    await CountryApi.getCountries({ isActive: true, page: 1, pageSize: 10 })

    expect(mockGetPaged).toHaveBeenCalledWith(
      'api/admin/location/countries',
      { filter: 'isActive=true', search: null, sort: null, pageNumber: 1, pageSize: 10 },
      expect.objectContaining({ allowedFilterFields: expect.any(Array) }),
    )
  })
})

describe('CountryApi.getCountry', () => {
  it('calls GET with correct URL', async () => {
    mockGet.mockResolvedValue({ value: { id: '1', name: 'US' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })

    await CountryApi.getCountry('abc-123')
    expect(mockGet).toHaveBeenCalledWith('api/admin/location/countries/abc-123')
  })
})

describe('CountryApi.getCountryByIso', () => {
  it('calls GET with by-iso URL', async () => {
    mockGet.mockResolvedValue({ value: { id: '1', name: 'United States', isoCode: 'US' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })

    await CountryApi.getCountryByIso('US')
    expect(mockGet).toHaveBeenCalledWith('api/admin/location/countries/by-iso/US')
  })
})

describe('CountryApi.createCountry', () => {
  it('calls POST with request body', async () => {
    const req = { name: 'Canada', isoCode: 'CA', callingCode: '+1', statesRequired: true, isActive: true }
    mockPost.mockResolvedValue({ value: { id: '1', ...req }, isSuccess: true, statusCode: 201, message: null, errors: [], metadata: null })

    await CountryApi.createCountry(req)
    expect(mockPost).toHaveBeenCalledWith('api/admin/location/countries', req)
  })
})

describe('CountryApi.updateCountry', () => {
  it('calls PUT with request body', async () => {
    const req = { name: 'Canada', isoCode: 'CA', callingCode: '+1', statesRequired: false, isActive: true }
    mockPut.mockResolvedValue({ value: { id: '1', ...req }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })

    await CountryApi.updateCountry('abc-123', req)
    expect(mockPut).toHaveBeenCalledWith('api/admin/location/countries/abc-123', req)
  })
})

describe('CountryApi.deleteCountry', () => {
  it('calls DELETE with correct URL', async () => {
    mockDel.mockResolvedValue({ value: { id: '1', name: 'Canada' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })

    await CountryApi.deleteCountry('abc-123')
    expect(mockDel).toHaveBeenCalledWith('api/admin/location/countries/abc-123')
  })
})
