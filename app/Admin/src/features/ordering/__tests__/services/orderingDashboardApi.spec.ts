import { describe, it, expect, vi, beforeEach } from 'vitest'

const { mockGet } = vi.hoisted(() => ({
  mockGet: vi.fn<(...args: unknown[]) => unknown>(),
}))

vi.mock('@/shared/api/client', () => ({
  get: mockGet,
}))

import { OrderingDashboardApi } from '../../services/orderingDashboardApi'

beforeEach(() => {
  vi.clearAllMocks()
})

describe('OrderingDashboardApi.getOrderingDashboard', () => {
  it('calls GET with dashboard URL', async () => {
    mockGet.mockResolvedValue({ value: {}, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await OrderingDashboardApi.getOrderingDashboard()
    expect(mockGet).toHaveBeenCalledWith('/api/admin/ordering/dashboard')
  })
})
