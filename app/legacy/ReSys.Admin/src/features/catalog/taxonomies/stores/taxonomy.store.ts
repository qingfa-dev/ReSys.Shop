import { defineStore } from 'pinia';
import { ref, watch, computed } from 'vue';
import { useToast } from '@/shared/composables/toast.use';
import { taxonomyService } from '../services/taxonomy.service';
import type { 
  TaxonomyListItem, 
  TaxonomyDetail, 
  TaxonomyQuery, 
  CreateTaxonomyRequest, 
  UpdateTaxonomyRequest 
} from '../types/taxonomy.types';
import apiClient from '@/shared/api/api.client';
import type { ApiResult } from '@/shared/api/api.types';

export const useTaxonomyStore = defineStore('taxonomy', () => {
  const { showToast } = useToast();

  // --- STATE ---
  const taxonomies = ref<TaxonomyListItem[]>([]);
  const current_taxonomy = ref<TaxonomyDetail | null>(null);
  const loading = ref(false);
  const submitting = ref(false);
  const error = ref<string | null>(null);

  const query = ref<TaxonomyQuery>({
    page: 1,
    page_size: 10,
    search: '',
    sort_by: 'position',
    is_descending: false
  });

  const totalRecords = ref(0);

  // --- ACTIONS ---
  async function fetchTaxonomies(params: TaxonomyQuery = {}) {
    loading.value = true;
    error.value = null;
    
    query.value = { ...query.value, ...params };

    try {
      const result = await taxonomyService.list(query.value);
      if (result.success && result.data) {
        taxonomies.value = result.data;
        totalRecords.value = result.meta?.total_count || 0;
      } else if (!result.success) {
        error.value = result.error.detail || 'Failed to fetch taxonomies';
      }
      return result;
    } finally {
      loading.value = false;
    }
  }

  async function fetchTaxonomyById(id: string) {
    loading.value = true;
    error.value = null;
    try {
      const result = await taxonomyService.getById(id);
      if (result.success && result.data) {
        current_taxonomy.value = result.data;
      } else if (!result.success) {
        error.value = result.error.detail || 'Failed to fetch taxonomy';
      }
      return result;
    } finally {
      loading.value = false;
    }
  }

  async function createTaxonomy(data: CreateTaxonomyRequest) {
    submitting.value = true;
    error.value = null;
    try {
      const result = await taxonomyService.create(data);
      if (result.success) {
        showToast('success', 'Created', 'Taxonomy created successfully');
        await fetchTaxonomies();
      } else if (!result.success) {
        error.value = result.error.detail || 'Failed to create taxonomy';
      }
      return result;
    } finally {
      submitting.value = false;
    }
  }

  async function updateTaxonomy(id: string, data: UpdateTaxonomyRequest) {
    submitting.value = true;
    error.value = null;
    try {
      const result = await taxonomyService.update(id, data);
      if (result.success) {
        showToast('success', 'Updated', 'Taxonomy updated successfully');
        await fetchTaxonomies();
      } else if (!result.success) {
        error.value = result.error.detail || 'Failed to update taxonomy';
      }
      return result;
    } finally {
      submitting.value = false;
    }
  }

  async function deleteTaxonomy(id: string) {
    loading.value = true;
    error.value = null;
    try {
      const result = await taxonomyService.delete(id);
      if (result.success) {
        showToast('success', 'Deleted', 'Taxonomy removed successfully');
        await fetchTaxonomies();
      } else if (!result.success) {
        error.value = result.error.detail || 'Failed to delete taxonomy';
      }
      return result;
    } finally {
      loading.value = false;
    }
  }

  async function rebuildTaxonomy(id: string) {
    loading.value = true;
    error.value = null;
    try {
        const result = (await apiClient.post(`/admin/catalog/taxonomies/${id}/rebuild`)) as unknown as ApiResult<void>;
        if (result.success) {
            showToast('success', 'Rebuilt', 'Taxonomy tree successfully rebuilt');
        } else if (!result.success) {
            error.value = result.error.detail || 'Failed to rebuild taxonomy';
        }
        return result;
    } finally {
        loading.value = false;
    }
  }

  return {
    // State
    taxonomies,
    current_taxonomy,
    loading,
    submitting,
    error,
    query,
    totalRecords,
    
    // Actions
    fetchTaxonomies,
    fetchTaxonomyById,
    createTaxonomy,
    updateTaxonomy,
    deleteTaxonomy,
    rebuildTaxonomy,
    // Legacy aliases
    fetchList: fetchTaxonomies,
    fetchById: fetchTaxonomyById,
    items: computed(() => taxonomies.value),
    currentItem: computed(() => current_taxonomy.value),
    clearCurrent: () => { current_taxonomy.value = null; }
  };
});