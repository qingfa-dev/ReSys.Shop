import { describe, it, expect, vi, beforeEach } from 'vitest'
import apiClient from '@/shared/api/client'
import { OptionTypeApi } from '../option-type.api'

vi.mock('@/shared/api/client', () => ({
  default: {
    get: vi.fn<(...args: unknown[]) => unknown>(),
    post: vi.fn<(...args: unknown[]) => unknown>(),
    put: vi.fn<(...args: unknown[]) => unknown>(),
    delete: vi.fn<(...args: unknown[]) => unknown>(),
  },
}))

const pagedEmpty = { isSuccess: true, items: [], page: 1, pageSize: 20, totalCount: 0, statusCode: 200 }
const singleOk = (value: unknown) => ({ isSuccess: true, value, statusCode: 200 })

describe('OptionTypeApi', () => {
  beforeEach(() => { vi.clearAllMocks() })

  it('getMany: GET /catalog/option-types', async () => {
    vi.mocked(apiClient.get).mockResolvedValue({ data: pagedEmpty })
    await OptionTypeApi.getMany({ page: 1 })
    expect(apiClient.get).toHaveBeenCalledWith('/catalog/option-types', { params: { page: 1 } })
  })

  it('get: GET /catalog/option-types/:id', async () => {
    vi.mocked(apiClient.get).mockResolvedValue({ data: singleOk({ id: '1', name: 'Size' }) })
    await OptionTypeApi.get('1')
    expect(apiClient.get).toHaveBeenCalledWith('/catalog/option-types/1')
  })

  it('create: POST /catalog/option-types', async () => {
    vi.mocked(apiClient.post).mockResolvedValue({ data: singleOk({ id: 'n', name: 'Color' }) })
    await OptionTypeApi.create({ name: 'Color' })
    expect(apiClient.post).toHaveBeenCalledWith('/catalog/option-types', { name: 'Color' })
  })

  it('update: PUT /catalog/option-types/:id', async () => {
    vi.mocked(apiClient.put).mockResolvedValue({ data: singleOk({ id: '1', name: 'Upd' }) })
    await OptionTypeApi.update('1', { name: 'Upd' })
    expect(apiClient.put).toHaveBeenCalledWith('/catalog/option-types/1', { name: 'Upd' })
  })

  it('delete: DELETE /catalog/option-types/:id', async () => {
    vi.mocked(apiClient.delete).mockResolvedValue({ data: { isSuccess: true, statusCode: 200 } })
    await OptionTypeApi.delete('1')
    expect(apiClient.delete).toHaveBeenCalledWith('/catalog/option-types/1')
  })

  it('getValues: GET .../option-types/:id/values', async () => {
    vi.mocked(apiClient.get).mockResolvedValue({ data: singleOk([]) })
    await OptionTypeApi.getValues('1')
    expect(apiClient.get).toHaveBeenCalledWith('/catalog/option-types/1/values')
  })

  it('createValue: POST .../option-types/:id/values', async () => {
    vi.mocked(apiClient.post).mockResolvedValue({ data: singleOk({ id: 'v1', name: 'Red' }) })
    await OptionTypeApi.createValue('1', { name: 'Red' })
    expect(apiClient.post).toHaveBeenCalledWith('/catalog/option-types/1/values', { name: 'Red' })
  })

  it('updateValue: PUT with type+value ids', async () => {
    vi.mocked(apiClient.put).mockResolvedValue({ data: singleOk({ id: 'v1', name: 'Blue' }) })
    await OptionTypeApi.updateValue('1', 'v1', { name: 'Blue' })
    expect(apiClient.put).toHaveBeenCalledWith('/catalog/option-types/1/values/v1', { name: 'Blue' })
  })

  it('deleteValue: DELETE with type+value ids', async () => {
    vi.mocked(apiClient.delete).mockResolvedValue({ data: { isSuccess: true, statusCode: 200 } })
    await OptionTypeApi.deleteValue('1', 'v1')
    expect(apiClient.delete).toHaveBeenCalledWith('/catalog/option-types/1/values/v1')
  })
})
