import { describe, it, expect, vi, beforeEach } from 'vitest'

const { mockGet } = vi.hoisted(() => ({
  mockGet: vi.fn<(...args: unknown[]) => unknown>(),
}))

vi.mock('@/shared/api/client', () => ({
  get: mockGet,
}))

import { InventoryDashboardApi } from '../../services/inventoryDashboardApi'

beforeEach(() => {
  vi.clearAllMocks()
})

describe('InventoryDashboardApi.getInventoryDashboard', () => {
  it('calls GET with dashboard URL', async () => {
    mockGet.mockResolvedValue({ value: { totalStockItems: 0 }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await InventoryDashboardApi.getInventoryDashboard()
    expect(mockGet).toHaveBeenCalledWith('api/admin/inventory/dashboard')
  })
})
