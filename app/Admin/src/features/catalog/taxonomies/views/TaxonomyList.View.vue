<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { useTaxonomyStore } from '../stores/taxonomy.store';
import { storeToRefs } from 'pinia';
import { useRouter } from 'vue-router';
import { useConfirm } from 'primevue/useconfirm';
import { FilterMatchMode, FilterOperator as PrimeFilterOperator } from '@primevue/core/api';
import type {
  DataTablePageEvent,
  DataTableSortEvent,
  DataTableFilterMeta,
} from 'primevue/datatable';
import { useToast } from '@/shared/composables/toast.use';
import { useFormatter } from '@/shared/composables/formatter.use';
import { QueryBuilder } from '@/shared/utils/query-builder.utils';
import AppBreadcrumb from '@/shared/components/Breadcrumb.Component.vue';
import type { TaxonomyListItem } from '../types/Taxonomy.Response.Type';

const { t } = useI18n();

const store = useTaxonomyStore();
const { taxonomies, loading, totalRecords, query } = storeToRefs(store);
const router = useRouter();
const confirm = useConfirm();
const { showToast } = useToast();

const filters = ref<DataTableFilterMeta>({
  global: { value: query.value.search || null, matchMode: FilterMatchMode.CONTAINS },
  name: {
    operator: PrimeFilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.CONTAINS }],
  }
});

const loadTaxonomies = async () => {
  await store.fetchTaxonomies();
};

const createItem = () => {
  router.push({ name: 'catalog.taxonomies.create' });
};

const onPage = (event: DataTablePageEvent) => {
  store.fetchTaxonomies({
    page: event.page !== undefined ? event.page + 1 : 1,
    pageSize: event.rows,
  });
};

const onSort = (event: DataTableSortEvent) => {
  const builder = new QueryBuilder();
  if (event.sortField) {
    builder.orderBy(event.sortField as string, event.sortOrder === -1 ? 'desc' : 'asc');
  }

  store.fetchTaxonomies({
    sort: [event.sortOrder === -1 ? `-${event.sortField}` : event.sortField as string],
    page: 1,
  });
};

const onFilter = () => {
  const globalFilter = filters.value.global as { value: string | null };
  const nameFilter = filters.value.name as { constraints: { value: string | null }[] };

  const builder = new QueryBuilder();

  if (nameFilter.constraints[0]?.value) {
    builder.where('Name', '*', nameFilter.constraints[0].value);
  }

  const built = builder.build();

  store.fetchTaxonomies({
    search: globalFilter.value || undefined,
    filter: built.filter,
    page: 1,
  });
};

const clearFilters = () => {
  filters.value = {
    global: { value: null, matchMode: FilterMatchMode.CONTAINS },
    name: {
      operator: PrimeFilterOperator.AND,
      constraints: [{ value: null, matchMode: FilterMatchMode.CONTAINS }],
    }
  };
  onFilter();
};

const confirmDelete = (taxonomy: TaxonomyListItem) => {
  const messageStr = (t('catalog.taxonomies.confirm.delete_message') || 'Delete "{name}"?').replace('{name}', taxonomy.name);
  
  confirm.require({
    message: messageStr,
    header: t('catalog.taxonomies.confirm.delete_header') || 'Confirm Deletion',
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: t('catalog.taxonomies.actions.cancel'),
    acceptProps: {
      label: t('catalog.taxonomies.actions.delete'),
      severity: 'danger',
    },
    accept: async () => {
      const result = await store.deleteTaxonomy(taxonomy.id);
      if (result.isSuccess) {
        showToast('success', t('common.success'), t('catalog.taxonomies.messages.delete_success'));
      }
    },
  });
};

onMounted(() => {
  loadTaxonomies();
});
</script>

<template>
  <div class="card">
    <div class="flex flex-col md:flex-row md:items-center justify-between mb-6 gap-4">
      <div>
        <h2 class="text-3xl font-black tracking-tight text-surface-900 dark:text-surface-50 mb-2">
          {{ t('catalog.taxonomies.titles.list') }}
        </h2>
        <div class="text-muted-color font-medium">
          {{ t('catalog.taxonomies.descriptions.list') }}
        </div>
      </div>
      <div class="flex gap-2">
        <Button 
          :label="t('catalog.taxonomies.actions.create')" 
          icon="pi pi-plus" 
          @click="createItem"
          class="px-4 shadow-lg rounded-xl"
        />
        <Button icon="pi pi-refresh" severity="secondary" outlined @click="loadTaxonomies" :loading="loading" />
      </div>
    </div>

    <div class="overflow-hidden border shadow-sm bg-surface-0 dark:bg-surface-900 rounded-2xl border-surface-100 dark:border-surface-800">
      <DataTable
        v-model:filters="filters"
        :value="taxonomies"
        :loading="loading"
        :totalRecords="totalRecords"
        lazy
        paginator
        :rows="query.pageSize || 10"
        :first="((query.page || 1) - 1) * (query.pageSize || 10)"
        @page="onPage"
        @sort="onSort"
        @filter="onFilter"
        filterDisplay="menu"
        removableSort
        scrollable
        rowHover
        dataKey="id"
      >
        <template #header>
          <div class="flex justify-end p-2">
            <IconField iconPosition="left" class="w-full md:w-72">
              <InputIcon class="pi pi-search" />
              <InputText
                v-model="(filters.global as any).value"
                :placeholder="t('catalog.taxonomies.placeholders.search') || 'Search...'"
                @keyup.enter="onFilter"
                class="w-full rounded-xl"
              />
            </IconField>
          </div>
        </template>

        <template #empty>
          <div class="flex flex-col items-center justify-center py-20 text-surface-400">
            <i class="mb-4 text-6xl pi pi-sitemap opacity-20"></i>
            <p class="text-xl font-medium">{{ t('catalog.taxonomies.messages.empty_list') }}</p>
          </div>
        </template>

        <Column field="name" :header="t('catalog.taxonomies.table.name')" sortable filter>
            <template #body="{ data }">
                <span class="font-bold text-surface-900 dark:text-surface-0">{{ data.name }}</span>
            </template>
        </Column>

        <Column field="presentation" :header="t('catalog.taxonomies.table.presentation')" sortable>
            <template #body="{ data }">
                <span class="font-medium text-surface-600 dark:text-surface-300">{{ data.presentation || '-' }}</span>
            </template>
        </Column>

        <Column field="taxonsCount" :header="t('catalog.taxonomies.table.taxons')" sortable class="text-center">
            <template #body="{ data }">
                <Badge :value="data.taxonsCount" severity="secondary" class="font-bold" />
            </template>
        </Column>

        <Column field="position" :header="t('catalog.taxonomies.table.position')" sortable class="text-center"></Column>

        <Column :header="t('catalog.taxonomies.table.actions')" class="w-48 text-right" frozen alignFrozen="right">
          <template #body="{ data }">
            <div class="flex justify-end gap-1">
              <Button icon="pi pi-sitemap" :label="t('catalog.taxonomies.actions.manage_tree')" text rounded severity="info" size="small" v-tooltip.top="'Manage Tree'" @click="router.push({ name: 'catalog.taxa.manager', params: { taxonomyId: data.id } })" />
              <Button icon="pi pi-pencil" severity="secondary" text rounded @click="router.push({ name: 'catalog.taxonomies.edit', params: { id: data.id } })" />
              <Button icon="pi pi-trash" severity="danger" text rounded @click="confirmDelete(data)" />
            </div>
          </template>
        </Column>
      </DataTable>
    </div>
  </div>
</template>

<style scoped>
:deep(.p-datatable-header) {
  background: transparent;
  padding: 1rem;
}
:deep(.p-datatable-thead > tr > th) {
  background: var(--p-content-background);
  color: var(--p-text-color);
  font-size: 0.875rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.025em;
  padding: 1rem 1.5rem;
  border-bottom: 2px solid var(--p-primary-color);
}
:deep(.p-datatable-tbody > tr > td) {
  padding: 1rem 1.5rem;
  border-bottom: 1px solid var(--p-content-border-color);
}
</style>
