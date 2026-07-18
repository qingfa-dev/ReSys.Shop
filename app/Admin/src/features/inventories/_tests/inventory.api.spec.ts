import { describe, it, expect, vi } from 'vitest'
import apiClient from '@/shared/api/http/api.client'
import { stockRepository } from '../stock-items/api/stock.api'
import { locationRepository } from '../stock-locations/api/location.api'
import { transferRepository } from '../stock-transfers/api/transfer.api'

vi.mock('@/shared/api/http/api.client', () => ({
  default: { get: vi.fn().mockResolvedValue({ data: {} }), post: vi.fn().mockResolvedValue({ data: {} }), put: vi.fn().mockResolvedValue({ data: {} }), patch: vi.fn().mockResolvedValue({ data: {} }), delete: vi.fn().mockResolvedValue({ data: {} }) }
}))

describe('StockRepository', () => {
  it('list calls correct route', async () => {
    await stockRepository.list({ page: 1 })
    expect(apiClient.get).toHaveBeenCalledWith('inventory/stock-items', { params: { page: 1 } })
  })
  it('restock calls correct route', async () => {
    await stockRepository.restock('sid-1', { quantity: 10, type: 0 })
    expect(apiClient.post).toHaveBeenCalledWith('inventory/stock-items/sid-1/restock', { quantity: 10, type: 0 })
  })
})

describe('LocationRepository', () => {
  it('list calls correct route', async () => {
    await locationRepository.list({ page: 1 })
    expect(apiClient.get).toHaveBeenCalledWith('inventory/stock-locations', { params: { page: 1 } })
  })
})

describe('TransferRepository', () => {
  it('transfer calls correct route', async () => {
    await transferRepository.transfer('tid-1')
    expect(apiClient.post).toHaveBeenCalledWith('inventory/stock-transfers/tid-1/transfer')
  })
  it('receive calls correct route', async () => {
    await transferRepository.receive('tid-1')
    expect(apiClient.post).toHaveBeenCalledWith('inventory/stock-transfers/tid-1/receive')
  })
})
