import { describe, it, expect, vi } from 'vitest'
import apiClient from '@/shared/api/http/api.client'
import { inventoryApi } from '../services/inventory.api'

vi.mock('@/shared/api/http/api.client', () => ({
  default: { get: vi.fn(), post: vi.fn(), put: vi.fn(), patch: vi.fn(), delete: vi.fn() }
}))

describe('inventoryApi', () => {
  it('stocks.list calls correct route', async () => {
    await inventoryApi.stocks.list({ page: 1 })
    expect(apiClient.get).toHaveBeenCalledWith('api/inventory/stock-items', expect.any(Object))
  })
  it('stocks.restock calls correct route', async () => {
    await inventoryApi.stocks.restock('sid-1', { quantity: 10, type: 0 })
    expect(apiClient.post).toHaveBeenCalledWith('api/inventory/stock-items/sid-1/restock', expect.any(Object))
  })
  it('locations.list calls correct route', async () => {
    await inventoryApi.locations.list({ page: 1 })
    expect(apiClient.get).toHaveBeenCalledWith('api/inventory/stock-locations', expect.any(Object))
  })
  it('transfers.transfer calls correct route', async () => {
    await inventoryApi.transfers.transfer('tid-1')
    expect(apiClient.post).toHaveBeenCalledWith('api/inventory/stock-transfers/tid-1/transfer')
  })
  it('transfers.receive calls correct route', async () => {
    await inventoryApi.transfers.receive('tid-1')
    expect(apiClient.post).toHaveBeenCalledWith('api/inventory/stock-transfers/tid-1/receive')
  })
})
