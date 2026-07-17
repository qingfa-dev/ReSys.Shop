import { defineStore } from 'pinia';
import { ref } from 'vue';
import { catalogDashboardService } from '../services/catalog-dashboard.service';
import type { CatalogDashboardResponse } from '../services/catalog-dashboard.service';

export const useCatalogDashboardStore = defineStore('catalog-dashboard', () => {
  const summary = ref<CatalogDashboardResponse | null>(null);
  const loading = ref(false);

  async function fetchSummary() {
    loading.value = true;
    try {
      const { data } = await catalogDashboardService.fetchDashboard();
      summary.value = { ...data };
    } finally {
      loading.value = false;
    }
  }

  return { summary, loading, fetchSummary };
});
