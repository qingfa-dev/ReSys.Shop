import { defineStore } from 'pinia';
import { ref } from 'vue';
import { reportService } from '../services/report.service';
import type { SalesSummary, InventorySummary, CatalogSummary, ActivityItem } from '../types/report.types';

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
      const value = data.value;
      sales.value = { ...value.sales };
      inventory.value = { ...value.inventory };
      catalog.value = { ...value.catalog };
      activities.value = value.recentActivities;
    } finally {
      is_loading.value = false;
    }
  }

  return { sales, inventory, catalog, activities, is_loading, fetchDashboardData };
});
