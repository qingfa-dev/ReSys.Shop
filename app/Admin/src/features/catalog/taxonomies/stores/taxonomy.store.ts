import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { useToast } from '@/shared/composables/toast.use';
import { taxonomyService } from '../services/taxonomy.service';
import { taxonomyRepository } from '../api/taxonomy.api';
import type { TaxonomyListItem, TaxonomyDetail } from '../types/Taxonomy.Response.Type'
import type { TaxonomyQuery } from '../types/Taxonomy.Query.Type'
import type { CreateTaxonomyRequest, UpdateTaxonomyRequest } from '../types/Taxonomy.Request.Type'

export const useTaxonomyStore = defineStore('taxonomy', () => {
  const { showToast } = useToast();
  const { t } = useI18n();

  // --- STATE ---
  const taxonomies = ref<TaxonomyListItem[]>([]);
  const current_taxonomy = ref<TaxonomyDetail | null>(null);
  const loading = ref(false);
  const submitting = ref(false);
  const error = ref<string | null>(null);

  const query = ref<TaxonomyQuery>({
    page: 1,
    pageSize: 10,
    search: '',
    sort: ['position']
  });

  const totalRecords = ref(0);

  // --- ACTIONS ---
  async function fetchTaxonomies(params: TaxonomyQuery = {}) {
    loading.value = true;
    error.value = null;
    
    query.value = { ...query.value, ...params };

    try {
      const result = await taxonomyService.list(query.value);
      if (result.isSuccess && result.items) {
        taxonomies.value = result.items;
        totalRecords.value = result.totalCount || 0;
      } else if (!result.isSuccess) {
        error.value = result.errors?.[0]?.message || 'Failed to fetch taxonomies';
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
      if (result.isSuccess && result.value) {
        current_taxonomy.value = result.value;
      } else if (!result.isSuccess) {
        error.value = result.errors?.[0]?.message || 'Failed to fetch taxonomy';
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
      if (result.isSuccess) {
        showToast('success', t('common.created'), t('catalog.taxonomies.messages.create_success'));
        await fetchTaxonomies();
      } else if (!result.isSuccess) {
        error.value = result.errors?.[0]?.message || 'Failed to create taxonomy';
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
      if (result.isSuccess) {
        showToast('success', t('common.updated'), t('catalog.taxonomies.messages.update_success'));
        await fetchTaxonomies();
      } else if (!result.isSuccess) {
        error.value = result.errors?.[0]?.message || 'Failed to update taxonomy';
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
      if (result.isSuccess) {
        showToast('success', t('common.deleted'), t('catalog.taxonomies.messages.delete_success'));
        await fetchTaxonomies();
      } else if (!result.isSuccess) {
        error.value = result.errors?.[0]?.message || 'Failed to delete taxonomy';
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
        const result = await taxonomyRepository.restore(id)
        if (result.isSuccess) {
            showToast('success', t('common.success'), t('catalog.taxonomies.messages.rebuilt_success'));
        } else if (!result.isSuccess) {
            error.value = result.errors?.[0]?.message || 'Failed to rebuild taxonomy';
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
