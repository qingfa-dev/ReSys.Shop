import { defineStore } from 'pinia';
import { ref } from 'vue';
import { catalogDashboardService } from '../services/catalog-dashboard.service';
import type { CatalogSummary } from '../types/catalog-dashboard.types';

export const useCatalogDashboardStore = defineStore('catalog-dashboard', () => {
  const summary = ref<CatalogSummary | null>(null);
  const loading = ref(false);
  const error = ref<string | null>(null);

  async function fetchSummary() {
    loading.value = true;
    error.value = null;
    try {
      const result = await catalogDashboardService.getSummary();
      if (result.success && result.data) {
        summary.value = result.data;
      } else {
        error.value = result.error?.title || 'Failed to fetch catalog summary';
      }
      return result;
    } finally {
      loading.value = false;
    }
  }

  return {
    summary,
    loading,
    error,
    fetchSummary
  };
});
