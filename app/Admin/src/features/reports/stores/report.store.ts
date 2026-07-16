import { defineStore } from 'pinia';
import { ref } from 'vue';
import type { SalesSummary, InventorySummary, CatalogSummary, ActivityItem } from '../types/report.types';

export const useReportStore = defineStore('report', () => {
  const sales = ref<SalesSummary | null>(null);
  const inventory = ref<InventorySummary | null>(null);
  const catalog = ref<CatalogSummary | null>(null);
  const activities = ref<ActivityItem[]>([]);
  const is_loading = ref(false);

  async function fetchDashboardData() {
    // No backend endpoint — re-add when Dashboard module endpoints are added
  }

  return { sales, inventory, catalog, activities, is_loading, fetchDashboardData };
});
