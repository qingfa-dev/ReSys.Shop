<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { useTaxonomyStore } from '../store/taxonomy.store';
import { storeToRefs } from 'pinia';
import { useRouter } from 'vue-router';
import { FilterMatchMode, FilterOperator as PrimeFilterOperator } from '@primevue/core/api';
import type {
  DataTablePageEvent,
  DataTableSortEvent,
  DataTableFilterMeta,
} from 'primevue/datatable';
import { getFilterValue } from '@/common/api/types/filter.types';
import { useToast } from '@/common/composables/toast.use';
import { QueryBuilder } from '@/common/utils/query-builder.utils';
import PageShell from '@/shared/components/navigation/PageShell.vue'
import PageHeader from '@/shared/components/navigation/PageHeader.vue'
import ConfirmDialog from '@/shared/components/overlays/ConfirmDialog.vue'


const { t } = useI18n();

const store = useTaxonomyStore();
const { taxonomies, loading, totalRecords, query } = storeToRefs(store);
const router = useRouter();
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
  const globalValue = getFilterValue(filters.value, 'global') as string | null;
  const nameFilter = filters.value.name as { constraints: { value: string | null }[] };

  const builder = new QueryBuilder();

  if (nameFilter.constraints[0]?.value) {
    builder.where('Name', '*', nameFilter.constraints[0].value);
  }

  const built = builder.build();

  store.fetchTaxonomies({
    search: globalValue || undefined,
    searchFields: globalValue ? ['Name', 'Presentation'] : undefined,
    filter: built.filter,
    page: 1,
  });
};

const deleteTaxonomy = async (taxonomyId: string) => {
  const result = await store.deleteTaxonomy(taxonomyId);
  if (result.isSuccess) {
    showToast('success', t('common.success'), t('catalog.taxonomies.messages.delete_success'));
  }
};

onMounted(() => {
  loadTaxonomies();
});
</script>

<template>
  <PageShell maxWidth="7xl">
    <PageHeader
      :title="t('catalog.taxonomies.titles.list')"
      :description="t('catalog.taxonomies.descriptions.list')"
    >
      <template #actions>
        <Button 
          :label="t('catalog.taxonomies.actions.create')" 
          icon="pi pi-plus" 
          @click="createItem"
          class="px-4 shadow-lg rounded-xl"
        />
        <Button icon="pi pi-refresh" severity="secondary" outlined @click="loadTaxonomies" :loading="loading" />
      </template>
    </PageHeader>

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
                v-model="(filters.global as { value: string | null }).value"
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

        <Column field="taxonsCount" :header="t('catalog.taxonomies.table.taxons')" class="text-center">
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
              <ConfirmDialog
                :header="t('catalog.taxonomies.confirm.delete_header')"
                :message="t('catalog.taxonomies.confirm.delete_message').replace('{name}', data.name)"
                :accept-label="t('catalog.taxonomies.actions.delete')"
                :reject-label="t('catalog.taxonomies.actions.cancel')"
                @confirm="deleteTaxonomy(data.id)" />
            </div>
          </template>
        </Column>
      </DataTable>
  </PageShell>
</template>
