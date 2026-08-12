import { describe, it, expect, vi, beforeEach } from 'vitest'

const { mockGet } = vi.hoisted(() => ({
  mockGet: vi.fn<(...args: unknown[]) => unknown>(),
}))

vi.mock('@/shared/api/client', () => ({
  get: mockGet,
}))

import { CatalogDashboardApi } from '../../services/catalogDashboardApi'

beforeEach(() => {
  vi.clearAllMocks()
})

describe('CatalogDashboardApi.getCatalogDashboard', () => {
  it('calls GET with dashboard URL', async () => {
    mockGet.mockResolvedValue({
      value: {
        totalProducts: 0,
        activeProducts: 0,
        draftProducts: 0,
        totalVariants: 0,
        totalTaxonomies: 0,
        totalTaxons: 0,
        recentProducts: [],
      },
      isSuccess: true,
      statusCode: 200,
      message: null,
      errors: [],
      metadata: null,
    })
    await CatalogDashboardApi.getCatalogDashboard()
    expect(mockGet).toHaveBeenCalledWith('/api/admin/catalog/dashboard')
  })
})
