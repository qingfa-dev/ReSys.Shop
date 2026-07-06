import { setActivePinia, createPinia } from 'pinia';
import { describe, it, expect, beforeEach, vi } from 'vitest';
import { useReportStore } from '../stores/report.store';
import { reportService } from '../services/report.service';

vi.mock('../services/report.service', () => ({
  reportService: {
    getSalesSummary: vi.fn(),
    getInventorySummary: vi.fn(),
    getCatalogSummary: vi.fn(),
    getRecentActivity: vi.fn(),
  }
}));

describe('ReportStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia());
    vi.clearAllMocks();
  });

  describe('fetchDashboardData', () => {
    it('updates state with data from all services', async () => {
      const store = useReportStore();
      
      const mockSales = { total_revenue: 1000 } as any;
      const mockInventory = { low_stock_count: 5 } as any;
      const mockCatalog = { active_products: 50 } as any;
      const mockActivity = { items: [{ id: '1', title: 'Test' }] } as any;

      vi.mocked(reportService.getSalesSummary).mockResolvedValue({ success: true, data: mockSales });
      vi.mocked(reportService.getInventorySummary).mockResolvedValue({ success: true, data: mockInventory });
      vi.mocked(reportService.getCatalogSummary).mockResolvedValue({ success: true, data: mockCatalog });
      vi.mocked(reportService.getRecentActivity).mockResolvedValue({ success: true, data: mockActivity });

      await store.fetchDashboardData();

      expect(store.sales).toEqual(mockSales);
      expect(store.inventory).toEqual(mockInventory);
      expect(store.catalog).toEqual(mockCatalog);
      expect(store.activities).toEqual(mockActivity.items);
      expect(store.is_loading).toBe(false);
    });
  });
});
