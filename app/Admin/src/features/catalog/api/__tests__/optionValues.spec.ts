import { describe, it, expect, vi, beforeEach } from 'vitest'
import apiClient from '@/shared/api/client'
import { OptionValueApi } from '../option-value.api'

vi.mock('@/shared/api/client', () => ({
  default: {
    get: vi.fn<(...args: unknown[]) => unknown>(),
    post: vi.fn<(...args: unknown[]) => unknown>(),
    put: vi.fn<(...args: unknown[]) => unknown>(),
    delete: vi.fn<(...args: unknown[]) => unknown>(),
  },
}))

const singleOk = (value: unknown) => ({ isSuccess: true, value, statusCode: 200 })
const listOk = (value: unknown[]) => ({ isSuccess: true, value, statusCode: 200 })

describe('OptionValueApi', () => {
  beforeEach(() => { vi.clearAllMocks() })

  it('getMany: GET /catalog/option-types/:id/values', async () => {
    vi.mocked(apiClient.get).mockResolvedValue({ data: listOk([{ id: '1', value: 'Red' }]) })
    await OptionValueApi.getMany('opt1')
    expect(apiClient.get).toHaveBeenCalledWith('/catalog/option-types/opt1/values')
  })

  it('create: POST /catalog/option-types/:id/values', async () => {
    vi.mocked(apiClient.post).mockResolvedValue({ data: singleOk({ id: 'new', value: 'Red' }) })
    await OptionValueApi.create('opt1', { optionTypeId: 'opt1', name: 'Red', value: 'red', displayOrder: 1 })
    expect(apiClient.post).toHaveBeenCalledWith('/catalog/option-types/opt1/values', { optionTypeId: 'opt1', name: 'Red', value: 'red', displayOrder: 1 })
  })

  it('update: PUT /catalog/option-types/:optionTypeId/values/:id', async () => {
    vi.mocked(apiClient.put).mockResolvedValue({ data: singleOk({ id: 'v1', value: 'Blue' }) })
    await OptionValueApi.update('opt1', 'v1', { optionTypeId: 'opt1', name: 'Blue', value: 'blue', displayOrder: 2 })
    expect(apiClient.put).toHaveBeenCalledWith('/catalog/option-types/opt1/values/v1', { optionTypeId: 'opt1', name: 'Blue', value: 'blue', displayOrder: 2 })
  })

  it('delete: DELETE /catalog/option-types/:optionTypeId/values/:id', async () => {
    vi.mocked(apiClient.delete).mockResolvedValue({ data: { isSuccess: true, statusCode: 200 } })
    await OptionValueApi.delete('opt1', 'v1')
    expect(apiClient.delete).toHaveBeenCalledWith('/catalog/option-types/opt1/values/v1')
  })
})
