import { defineStore } from 'pinia';
import { ref } from 'vue';

interface DashboardSummary {
  totalProducts: number;
  activeProducts: number;
  totalVariants: number;
  totalTaxonomies: number;
  totalTaxons: number;
  totalDigitalProducts: number;
  recentlyAdded: Array<{ id: string; name: string; slug: string; createdAtUtc: string }>;
}

export const useCatalogDashboardStore = defineStore('catalog-dashboard', () => {
  const summary = ref<DashboardSummary | null>(null);
  const loading = ref(false);

  async function fetchSummary() {
    // No backend endpoint — re-add when backend adds GET api/catalog/dashboard/summary
    loading.value = false;
  }

  return { summary, loading, fetchSummary };
});
