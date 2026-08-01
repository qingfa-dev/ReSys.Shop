import { describe, it, expect, vi, beforeEach } from 'vitest'

const { mockPost, mockPut, mockDel, mockGetPaged } = vi.hoisted(() => ({
  mockPost: vi.fn<any>(),
  mockPut: vi.fn<any>(),
  mockDel: vi.fn<any>(),
  mockGetPaged: vi.fn<any>(),
}))

vi.mock('@/shared/api/client', () => ({
  post: mockPost,
  put: mockPut,
  del: mockDel,
}))

vi.mock('@/shared/api', () => ({
  getPaged: mockGetPaged,
}))

import { ProfileApi } from '../../services/profileApi'

beforeEach(() => {
  vi.clearAllMocks()
})

describe('ProfileApi.getProfiles', () => {
  it('calls getPaged with the doubled profile route and allowed fields', async () => {
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

    await ProfileApi.getProfiles({
      gender: 'Male',
      isActive: true,
      sortBy: 'firstName',
      sortDirection: 'desc',
      page: 1,
      pageSize: 10,
    })

    expect(mockGetPaged).toHaveBeenCalledWith(
      'api/profiles/profiles/all',
      {
        filter: 'Gender=Male,IsActive=true',
        search: null,
        searchFields: ['FirstName', 'LastName', 'Email', 'Bio'],
        sort: ['-firstName'],
        pageNumber: 1,
        pageSize: 10,
      },
      expect.objectContaining({
        allowedFilterFields: ['Gender', 'IsActive', 'CreatedAtUtc', 'ModifiedAtUtc'],
        allowedSortFields: ['FirstName', 'LastName', 'CreatedAtUtc', 'ModifiedAtUtc'],
        allowedSearchFields: ['FirstName', 'LastName', 'Email', 'Bio'],
      }),
    )
  })
})

describe('ProfileApi.createProfile', () => {
  it('calls POST with the doubled profile route and request body', async () => {
    const req = {
      userId: 'u-1',
      firstName: 'A',
      lastName: 'B',
      email: 'a@b.com',
    }
    mockPost.mockResolvedValue({ value: { id: 'p-1', ...req, fullName: 'A B' }, isSuccess: true, statusCode: 201, message: null, errors: [], metadata: null })
    await ProfileApi.createProfile(req)
    expect(mockPost).toHaveBeenCalledWith('api/profiles/profiles', req)
  })
})

describe('ProfileApi.updateProfile', () => {
  it('calls PUT with the doubled profile route and request body', async () => {
    const req = {
      userId: 'u-1',
      firstName: 'A',
      lastName: 'B',
      email: 'a@b.com',
    }
    mockPut.mockResolvedValue({ value: { id: 'p-1', ...req, fullName: 'A B' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await ProfileApi.updateProfile(req)
    expect(mockPut).toHaveBeenCalledWith('api/profiles/profiles', req)
  })
})

describe('ProfileApi.deleteProfile', () => {
  it('calls DELETE with userId query parameter', async () => {
    mockDel.mockResolvedValue({ value: null, isSuccess: true, statusCode: 204, message: null, errors: [], metadata: null })
    await ProfileApi.deleteProfile('u-1')
    expect(mockDel).toHaveBeenCalledWith('api/profiles/profiles?userId=u-1')
  })
})
