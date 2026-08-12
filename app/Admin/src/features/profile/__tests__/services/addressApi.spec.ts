import { describe, it, expect, vi, beforeEach } from 'vitest'

const { mockGet, mockPost, mockPut, mockDel, mockGetPaged } = vi.hoisted(() => ({
  mockGet: vi.fn<(...args: unknown[]) => unknown>(),
  mockPost: vi.fn<(...args: unknown[]) => unknown>(),
  mockPut: vi.fn<(...args: unknown[]) => unknown>(),
  mockDel: vi.fn<(...args: unknown[]) => unknown>(),
  mockGetPaged: vi.fn<(...args: unknown[]) => unknown>(),
}))

vi.mock('@/shared/api/client', () => ({
  get: mockGet,
  post: mockPost,
  put: mockPut,
  del: mockDel,
}))

vi.mock('@/shared/api', () => ({
  getPaged: mockGetPaged,
}))

import { AddressApi } from '../../services/addressApi'

beforeEach(() => {
  vi.clearAllMocks()
})

describe('AddressApi.getAddresses', () => {
  it('calls getPaged with userId query parameter and allowed fields', async () => {
    mockGetPaged.mockResolvedValue({
      items: [],
      page: 1,
      pageSize: 10,
      totalCount: 0,
      totalPages: 0,
      isSuccess: true,
      statusCode: 200,
      message: null,
      errors: [],
      metadata: null,
    })

    await AddressApi.getAddresses('u-1', {
      filter: 'addressType=Shipping,isDefault=true',
      search: null,
      searchFields: ['firstName', 'lastName', 'address1', 'city', 'countryName', 'label', 'phone'],
      sort: ['-city'],
      pageNumber: 1,
      pageSize: 10,
    })

    expect(mockGetPaged).toHaveBeenCalledWith(
      '/api/admin/customer/addresses?userId=u-1',
      {
        filter: 'addressType=Shipping,isDefault=true',
        search: null,
        searchFields: ['firstName', 'lastName', 'address1', 'city', 'countryName', 'label', 'phone'],
        sort: ['-city'],
        pageNumber: 1,
        pageSize: 10,
      },
      expect.objectContaining({
        allowedFilterFields: ['addressType', 'countryCode', 'stateCode', 'isDefault', 'isDefaultBilling', 'isDefaultShipping', 'userProfileId'],
        allowedSortFields: ['firstName', 'city', 'countryName', 'addressType'],
        allowedSearchFields: ['firstName', 'lastName', 'address1', 'city', 'countryName', 'label', 'phone'],
      }),
    )
  })
})

describe('AddressApi.getAddress', () => {
  it('calls GET with id in URL and userId query parameter', async () => {
    mockGet.mockResolvedValue({ value: { id: 'a-1' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await AddressApi.getAddress('u-1', 'a-1')
    expect(mockGet).toHaveBeenCalledWith('/api/admin/customer/addresses/a-1?userId=u-1')
  })
})

describe('AddressApi.createAddress', () => {
  it('calls POST with request body including userId', async () => {
    const req = {
      userId: 'u-1',
      addressType: 'Shipping',
      firstName: 'A',
      address1: '1 Main St',
      city: 'Hanoi',
      isDefault: true,
      countryName: 'Vietnam',
    }
    mockPost.mockResolvedValue({ value: { id: 'a-1', ...req }, isSuccess: true, statusCode: 201, message: null, errors: [], metadata: null })
    await AddressApi.createAddress(req)
    expect(mockPost).toHaveBeenCalledWith('/api/admin/customer/addresses', req)
  })
})

describe('AddressApi.updateAddress', () => {
  it('calls PUT with id in URL and request body including userId', async () => {
    const req = {
      userId: 'u-1',
      addressType: 'Shipping',
      firstName: 'A',
      address1: '1 Main St',
      city: 'Hanoi',
      isDefault: true,
      countryName: 'Vietnam',
    }
    mockPut.mockResolvedValue({ value: { id: 'a-1', ...req }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await AddressApi.updateAddress('a-1', req)
    expect(mockPut).toHaveBeenCalledWith('/api/admin/customer/addresses/a-1', req)
  })
})

describe('AddressApi.deleteAddress', () => {
  it('calls DELETE with id in URL and userId query parameter', async () => {
    mockDel.mockResolvedValue({ value: null, isSuccess: true, statusCode: 204, message: null, errors: [], metadata: null })
    await AddressApi.deleteAddress('u-1', 'a-1')
    expect(mockDel).toHaveBeenCalledWith('/api/admin/customer/addresses/a-1?userId=u-1')
  })
})