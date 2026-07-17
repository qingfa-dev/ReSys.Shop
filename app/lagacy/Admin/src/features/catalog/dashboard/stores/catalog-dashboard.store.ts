import { defineStore } from 'pinia';
import { ref } from 'vue';

export const useCatalogDashboardStore = defineStore('catalog-dashboard', () => {
  const summary = ref(null);
  const loading = ref(false);

  async function fetchSummary() {
    // No backend endpoint — re-add when backend adds GET api/catalog/dashboard/summary
    loading.value = false;
  }

  return { summary, loading, fetchSummary };
});
