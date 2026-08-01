import { describe, it, expect, vi, beforeEach } from 'vitest'
import { setBaseUrl, setAuthToken, get, post, put, patch, del, delWithBody } from '../client'

const { mockGet, mockPost, mockPut, mockPatch, mockDelete, mockDefaults } = vi.hoisted(() => ({
  mockGet: vi.fn(),
  mockPost: vi.fn(),
  mockPut: vi.fn(),
  mockPatch: vi.fn(),
  mockDelete: vi.fn(),
  mockDefaults: { baseURL: '' },
}))

vi.mock('../axios', () => ({
  getApiClient: vi.fn(() => ({
    get: mockGet,
    post: mockPost,
    put: mockPut,
    patch: mockPatch,
    delete: mockDelete,
    defaults: mockDefaults,
  })),
  createApiClient: vi.fn(),
  resetApiClient: vi.fn(),
}))

beforeEach(() => {
  vi.clearAllMocks()
  localStorage.clear()
})

describe('HTTP methods', () => {
  it('get calls axios get and returns data', async () => {
    mockGet.mockResolvedValue({ data: 'response-data' })
    const result = await get('/resource')
    expect(result).toBe('response-data')
    expect(mockGet).toHaveBeenCalledWith('/resource', { signal: undefined })
  })

  it('get passes signal', async () => {
    mockGet.mockResolvedValue({ data: null })
    const controller = new AbortController()
    await get('/resource', controller.signal)
    expect(mockGet).toHaveBeenCalledWith('/resource', { signal: controller.signal })
  })

  it('post sends body and returns data', async () => {
    mockPost.mockResolvedValue({ data: { id: 1 } })
    const result = await post('/items', { name: 'test' })
    expect(result).toEqual({ id: 1 })
    expect(mockPost).toHaveBeenCalledWith('/items', { name: 'test' }, { signal: undefined })
  })

  it('post works without body', async () => {
    mockPost.mockResolvedValue({ data: null })
    await post('/items')
    expect(mockPost).toHaveBeenCalledWith('/items', undefined, { signal: undefined })
  })

  it('put sends body and returns data', async () => {
    mockPut.mockResolvedValue({ data: { updated: true } })
    const result = await put('/items/1', { name: 'updated' })
    expect(result).toEqual({ updated: true })
  })

  it('patch sends body and returns data', async () => {
    mockPatch.mockResolvedValue({ data: { patched: true } })
    const result = await patch('/items/1', { name: 'patched' })
    expect(result).toEqual({ patched: true })
  })

  it('del calls axios delete and returns data', async () => {
    mockDelete.mockResolvedValue({ data: null })
    await del('/items/1')
    expect(mockDelete).toHaveBeenCalledWith('/items/1', { signal: undefined })
  })

  it('delWithBody sends body as data on axios delete and returns data', async () => {
    mockDelete.mockResolvedValue({ data: { deleted: true } })
    const result = await delWithBody('/items/1', { foo: 1 })
    expect(result).toEqual({ deleted: true })
    expect(mockDelete).toHaveBeenCalledWith('/items/1', { data: { foo: 1 }, signal: undefined })
  })

  it('delWithBody works without body', async () => {
    mockDelete.mockResolvedValue({ data: null })
    await delWithBody('/items/1')
    expect(mockDelete).toHaveBeenCalledWith('/items/1', { data: undefined, signal: undefined })
  })

  it('delWithBody passes signal', async () => {
    mockDelete.mockResolvedValue({ data: null })
    const controller = new AbortController()
    await delWithBody('/items/1', { foo: 1 }, controller.signal)
    expect(mockDelete).toHaveBeenCalledWith('/items/1', { data: { foo: 1 }, signal: controller.signal })
  })
})

describe('setBaseUrl', () => {
  it('updates the axios instance baseURL', () => {
    setBaseUrl('http://new-url')
    expect(mockDefaults.baseURL).toBe('http://new-url')
  })
})

describe('setAuthToken', () => {
  it('saves token to localStorage as accessToken', () => {
    setAuthToken('my-token')
    expect(localStorage.getItem('accessToken')).toBe('my-token')
  })

  it('removes token when null', () => {
    localStorage.setItem('accessToken', 'old')
    setAuthToken(null)
    expect(localStorage.getItem('accessToken')).toBeNull()
  })
})
