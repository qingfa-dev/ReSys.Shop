import { defineStore } from 'pinia';
import { ref } from 'vue';
import { reportService } from '../services/report.service';
import type { 
  SalesSummary, 
  InventorySummary, 
  CatalogSummary, 
  ActivityItem,
  DashboardQuery
} from '../types/report.types';

export const useReportStore = defineStore('report', () => {
  // --- STATE ---
  const sales = ref<SalesSummary | null>(null);
  const inventory = ref<InventorySummary | null>(null);
  const catalog = ref<CatalogSummary | null>(null);
  const activities = ref<ActivityItem[]>([]);
  const is_loading = ref(false);

  // --- ACTIONS ---
  async function fetchDashboardData() {
    is_loading.value = true;
    try {
      const [salesRes, invRes, catRes, actRes] = await Promise.all([
        reportService.getSalesSummary(),
        reportService.getInventorySummary(),
        reportService.getCatalogSummary(),
        reportService.getRecentActivity()
      ]);

      if (salesRes.success) sales.value = salesRes.data;
      if (invRes.success) inventory.value = invRes.data;
      if (catRes.success) catalog.value = catRes.data;
      if (actRes.success) activities.value = actRes.data.items;

    } finally {
      is_loading.value = false;
    }
  }

  return {
    sales,
    inventory,
    catalog,
    activities,
    is_loading,
    fetchDashboardData
  };
});
