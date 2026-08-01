import { describe, it, expect, vi, beforeEach } from 'vitest'

const { mockGet } = vi.hoisted(() => ({
  mockGet: vi.fn<any>(),
}))

vi.mock('@/shared/api/client', () => ({
  get: mockGet,
}))

import { DashboardApi } from '../../services/dashboardApi'

function dashboardResult() {
  return {
    isSuccess: true,
    statusCode: 200,
    message: null,
    errors: [],
    metadata: null,
    value: {
      sales: {
        totalRevenue: 0,
        orderCount: 0,
        averageOrderValue: 0,
        revenueTrendPercentage: 0,
        trendHistory: [],
      },
      inventory: {
        totalVariants: 0,
        outOfStockCount: 0,
        lowStockCount: 0,
        stockAccuracyPercentage: 0,
      },
      catalog: {
        totalProducts: 0,
        activeProducts: 0,
        totalVariants: 0,
        totalTaxonomies: 0,
        totalTaxons: 0,
        recentlyAdded: [],
      },
      recentActivities: [],
    },
  }
}

beforeEach(() => {
  vi.clearAllMocks()
})

describe('DashboardApi.getDashboard', () => {
  it('calls GET with the dashboard URL', async () => {
    mockGet.mockResolvedValue(dashboardResult())

    await DashboardApi.getDashboard()

    expect(mockGet).toHaveBeenCalledWith('api/dashboard')
  })

  it('resolves the dashboard summary result', async () => {
    mockGet.mockResolvedValue(dashboardResult())

    const result = await DashboardApi.getDashboard()

    expect(result.isSuccess).toBe(true)
    expect(result.value.sales.totalRevenue).toBe(0)
  })
})
