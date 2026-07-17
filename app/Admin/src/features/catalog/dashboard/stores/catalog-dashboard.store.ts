import { defineStore } from 'pinia';
import { ref } from 'vue';
import { catalogDashboardService } from '../services/catalog-dashboard.service';
import type { CatalogDashboardResponse } from '../services/catalog-dashboard.service';

export const useCatalogDashboardStore = defineStore('catalog-dashboard', () => {
  const summary = ref<CatalogDashboardResponse | null>(null);
  const loading = ref(false);
  const error = ref<string | null>(null);

  async function fetchSummary() {
    loading.value = true;
    error.value = null;
    try {
      const { data } = await catalogDashboardService.fetchDashboard();
      summary.value = { ...data };
    } catch (e) {
      error.value = 'Failed to load dashboard data';
      summary.value = null;
    } finally {
      loading.value = false;
    }
  }

  return { summary, loading, error, fetchSummary };
});
