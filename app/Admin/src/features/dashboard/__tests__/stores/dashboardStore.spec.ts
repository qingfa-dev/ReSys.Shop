import { describe, it, expect, vi, beforeEach } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'

const { mockGetDashboard } = vi.hoisted(() => ({
  mockGetDashboard: vi.fn<any>(),
}))

vi.mock('../../services/dashboardApi', () => ({
  DashboardApi: {
    getDashboard: mockGetDashboard,
  },
}))

import { useDashboardStore } from '../../stores/dashboardStore'

function dashboardResult() {
  return {
    isSuccess: true,
    statusCode: 200,
    message: null,
    errors: [],
    metadata: null,
    value: {
      sales: {
        totalRevenue: 100,
        orderCount: 2,
        averageOrderValue: 50,
        revenueTrendPercentage: 10,
        trendHistory: [{ date: '2026-01-01', revenue: 50 }],
      },
      inventory: {
        totalVariants: 3,
        outOfStockCount: 1,
        lowStockCount: 0,
        stockAccuracyPercentage: 80,
      },
      catalog: {
        totalProducts: 4,
        activeProducts: 3,
        totalVariants: 5,
        totalTaxonomies: 2,
        totalTaxons: 6,
        recentlyAdded: [{ id: 'p-1', name: 'Shirt', slug: 'shirt', createdAtUtc: '2026-01-01T00:00:00Z' }],
      },
      recentActivities: [{ id: 'a-1', type: 'order', title: 'New order', description: 'desc', status: 'created', timestamp: '2026-01-01T00:00:00Z' }],
    },
  }
}

describe('useDashboardStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  it('fetchDashboard calls getDashboard once and populates summary', async () => {
    mockGetDashboard.mockResolvedValue(dashboardResult())
    const store = useDashboardStore()

    await store.fetchDashboard()

    expect(mockGetDashboard).toHaveBeenCalledTimes(1)
    expect(store.summary?.sales.totalRevenue).toBe(100)
    expect(store.summary?.catalog.recentlyAdded).toHaveLength(1)
    expect(store.loaded).toBe(true)
  })

  it('fetchDashboard does not refetch after loaded', async () => {
    mockGetDashboard.mockResolvedValue(dashboardResult())
    const store = useDashboardStore()

    await store.fetchDashboard()
    await store.fetchDashboard()

    expect(mockGetDashboard).toHaveBeenCalledTimes(1)
  })
})
