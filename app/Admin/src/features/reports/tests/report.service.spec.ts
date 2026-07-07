import { describe, it, expect, vi, beforeEach } from 'vitest'
import { reportService } from '../services/report.service'
import apiClient from '@/shared/api/http/api.client'

// Mock apiClient
vi.mock('@/shared/api/http/api.client', () => ({
  default: {
    get: vi.fn(),
  },
}))

describe('ReportService', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  describe('getSalesSummary', () => {
    it('should call api.get with correct endpoint', async () => {
      const mockResponse = { data: {}, success: true }
      vi.mocked(apiClient.get).mockResolvedValue(mockResponse)

      const result = await reportService.getSalesSummary()

      expect(apiClient.get).toHaveBeenCalledWith('admin/dashboard/sales-summary', { params: {} })
      expect(result).toEqual(mockResponse)
    })
  })

  describe('getInventorySummary', () => {
    it('should call api.get with correct endpoint', async () => {
      const mockResponse = { data: {}, success: true }
      vi.mocked(apiClient.get).mockResolvedValue(mockResponse)

      const result = await reportService.getInventorySummary()

      expect(apiClient.get).toHaveBeenCalledWith('admin/dashboard/inventory-summary')
      expect(result).toEqual(mockResponse)
    })
  })

  describe('getCatalogSummary', () => {
    it('should call api.get with correct endpoint', async () => {
      const mockResponse = { data: {}, success: true }
      vi.mocked(apiClient.get).mockResolvedValue(mockResponse)

      const result = await reportService.getCatalogSummary()

      expect(apiClient.get).toHaveBeenCalledWith('admin/dashboard/catalog-summary')
      expect(result).toEqual(mockResponse)
    })
  })

  describe('getRecentActivity', () => {
    it('should call api.get with correct limit param', async () => {
      const mockResponse = { data: { items: [] }, success: true }
      vi.mocked(apiClient.get).mockResolvedValue(mockResponse)

      const result = await reportService.getRecentActivity(5)

      expect(apiClient.get).toHaveBeenCalledWith('admin/dashboard/recent-activity', { params: { limit: 5 } })
      expect(result).toEqual(mockResponse)
    })
  })
})
