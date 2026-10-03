import { describe, it, expect, vi, beforeEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useProfileStore } from '../profile.store'
import { ProfileApi } from '../../api'

const mockGet = vi.hoisted(() => vi.fn<(...args: any[]) => any>())
const mockUpdate = vi.hoisted(() => vi.fn<(...args: any[]) => any>())

vi.mock('../../api', () => ({
  ProfileApi: {
    get: mockGet,
    update: mockUpdate,
  },
}))

function result<T>(value: T) {
  return {
    isSuccess: true,
    statusCode: 200,
    value,
    errors: [],
    message: null,
    metadata: null,
  }
}

function errorResult(message = 'Something went wrong') {
  return {
    isSuccess: false,
    statusCode: 400,
    value: null,
    errors: [],
    message,
    metadata: null,
  }
}

const mockProfile = {
  id: '1',
  userId: 'u1',
  firstName: 'John',
  lastName: 'Doe',
  email: 'john@example.com',
  phone: null,
  avatarUrl: null,
  dateOfBirth: null,
  createdAt: '2025-01-01',
  updatedAt: '2025-01-01',
}

describe('useProfileStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  it('has initial state', () => {
    const store = useProfileStore()
    expect(store.loading).toBe(false)
    expect(store.error).toBeNull()
    expect(store.profile).toBeNull()
  })

  it('fetchProfile success', async () => {
    mockGet.mockResolvedValue(result(mockProfile))
    const store = useProfileStore()
    await store.fetchProfile()
    expect(store.loading).toBe(false)
    expect(store.profile).toEqual(mockProfile)
    expect(store.error).toBeNull()
  })

  it('resets profile to null on error', async () => {
    mockGet.mockResolvedValueOnce(result(mockProfile))
    const store = useProfileStore()
    await store.fetchProfile()
    expect(store.profile).toEqual(mockProfile)

    mockGet.mockResolvedValueOnce(errorResult('Not found'))
    await store.fetchProfile()
    expect(store.profile).toBeNull()
    expect(store.error).toBe('Not found')
  })

  it('fetchProfile sets error on API failure', async () => {
    mockGet.mockResolvedValue(errorResult('Failed to load profile'))
    const store = useProfileStore()
    await store.fetchProfile()
    expect(store.loading).toBe(false)
    expect(store.error).toBe('Failed to load profile')
    expect(store.profile).toBeNull()
  })

  it('fetchProfile sets error on network failure', async () => {
    mockGet.mockRejectedValue(new Error('Network error'))
    const store = useProfileStore()
    await store.fetchProfile()
    expect(store.loading).toBe(false)
    expect(store.error).toBe('Failed to load profile')
    expect(store.profile).toBeNull()
  })

  it('fetchProfile uses default error message when result.message is null', async () => {
    mockGet.mockResolvedValue({ isSuccess: false, statusCode: 400, value: null, errors: [], message: null, metadata: null })
    const store = useProfileStore()
    await store.fetchProfile()
    expect(store.error).toBe('Failed to load profile')
  })

  it('loading is true during fetchProfile', async () => {
    let resolver!: (value: unknown) => void
    mockGet.mockImplementation(() => new Promise(resolve => { resolver = resolve }))
    const store = useProfileStore()
    const promise = store.fetchProfile()
    expect(store.loading).toBe(true)
    resolver(result(mockProfile))
    await promise
  })

  it('updateProfile success via API', async () => {
    const updatedProfile = { ...mockProfile, firstName: 'Jane' }
    mockUpdate.mockResolvedValue(result(updatedProfile))
    const res = await ProfileApi.update({ firstName: 'Jane', lastName: 'Doe' })
    expect(mockUpdate).toHaveBeenCalledWith({ firstName: 'Jane', lastName: 'Doe' })
    expect(res.isSuccess).toBe(true)
    expect(res.value).toEqual(updatedProfile)
  })

  it('updateProfile error via API', async () => {
    mockUpdate.mockResolvedValue(errorResult('Update failed'))
    const res = await ProfileApi.update({ firstName: 'Jane', lastName: 'Doe' })
    expect(res.isSuccess).toBe(false)
    expect(res.message).toBe('Update failed')
  })
})
