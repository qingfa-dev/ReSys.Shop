import { defineStore } from 'pinia';
import { ref } from 'vue';
import { reportService } from '../services/report.service';
import type { SalesSummary, InventorySummary, CatalogSummary, ActivityItem } from '../types/report.response.type';

export const useReportStore = defineStore('report', () => {
  const sales = ref<SalesSummary | null>(null);
  const inventory = ref<InventorySummary | null>(null);
  const catalog = ref<CatalogSummary | null>(null);
  const activities = ref<ActivityItem[]>([]);
  const is_loading = ref(false);

  async function fetchDashboardData() {
    is_loading.value = true;
    try {
      const { data } = await reportService.fetchDashboard();
      sales.value = { ...data.sales };
      inventory.value = { ...data.inventory };
      catalog.value = { ...data.catalog };
      activities.value = data.recentActivities.map((item: Record<string, unknown>) => ({
        ...item,
        type: item.type as 'Order' | 'Stock',
      })) as ActivityItem[];
    } finally {
      is_loading.value = false;
    }
  }

  return { sales, inventory, catalog, activities, is_loading, fetchDashboardData };
});
