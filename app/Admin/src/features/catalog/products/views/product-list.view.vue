<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useProductStore } from '../stores/product.store';
import { storeToRefs } from 'pinia';
import { useRouter } from 'vue-router';
import { useConfirm } from 'primevue/useconfirm';
import { productLocales as t } from '../locales/product.locales';
import { FilterMatchMode, FilterOperator as PrimeFilterOperator } from '@primevue/core/api';
import type {
  DataTablePageEvent,
  DataTableSortEvent,
  DataTableFilterMeta,
} from 'primevue/datatable';
import { useToast } from '@/shared/composables/toast.use';
import { useFormatter } from '@/shared/composables/formatter.use';
import { QueryBuilder, type FilterOperator } from '@/shared/utils/query-builder.utils';
import AppBreadcrumb from '@/shared/components/breadcrumb.component.vue';
import type { ProductSummary } from '../types/product.types';

// --- STORE & ROUTING ---
const store = useProductStore();
const { products, loading, totalRecords, query } = storeToRefs(store);
const router = useRouter();
const confirm = useConfirm();
const { formatCurrency } = useFormatter();
const { showToast } = useToast();

/**
 * PrimeVue Filter Configuration
 */
const filters = ref<DataTableFilterMeta>({
  global: { value: query.value.search || null, matchMode: FilterMatchMode.CONTAINS },
  name: {
    operator: PrimeFilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.CONTAINS }],
  },
  sku: {
    operator: PrimeFilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.CONTAINS }],
  }
});

// --- DATA ACTIONS ---

const loadProducts = async () => {
  await store.fetchProducts();
};

const onPage = (event: DataTablePageEvent) => {
  store.fetchProducts({
    page: event.page !== undefined ? event.page + 1 : 1,
    page_size: event.rows,
  });
};

const onSort = (event: DataTableSortEvent) => {
  const builder = new QueryBuilder();
  if (event.sortField) {
    builder.orderBy(event.sortField as string, event.sortOrder === -1 ? 'desc' : 'asc');
  }

  store.fetchProducts({
    sort_by: event.sortField as string,
    is_descending: event.sortOrder === -1,
    page: 1,
  });
};

const onFilter = () => {
  const globalFilter = filters.value.global as { value: string | null };
  const nameFilter = filters.value.name as { constraints: { value: string | null }[] };
  const skuFilter = filters.value.sku as { constraints: { value: string | null }[] };

  const builder = new QueryBuilder();

  if (nameFilter.constraints[0]?.value) {
    builder.where('Name', '*', nameFilter.constraints[0].value);
  }
  
  if (skuFilter.constraints[0]?.value) {
    builder.where('Sku', '*', skuFilter.constraints[0].value);
  }

  const built = builder.build();

  store.fetchProducts({
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
    },
    sku: {
      operator: PrimeFilterOperator.AND,
      constraints: [{ value: null, matchMode: FilterMatchMode.CONTAINS }],
    }
  };
  onFilter();
};

const confirmDelete = (product: ProductSummary) => {
  const messageStr = (t.confirm?.delete_message as string || 'Delete "{name}"?').replace('{name}', product.name);
  
  confirm.require({
    message: messageStr,
    header: t.confirm?.delete_header as string || 'Confirm Delete',
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: t.confirm?.reject_label as string || 'Cancel',
    acceptProps: {
      label: t.confirm?.accept_label as string || 'Delete',
      severity: 'danger',
    },
    accept: async () => {
      const result = await store.deleteProduct(product.id);
      if (result.success) {
        showToast('success', t.common?.success || 'Deleted', t.messages?.delete_success || 'Product removed.');
      }
    },
  });
};

onMounted(() => {
  loadProducts();
});
</script>

<template>
  <div class="p-6">
    <AppBreadcrumb :locales="t" />
    <div class="flex flex-col items-start justify-between gap-4 mb-8 md:flex-row md:items-center">
      <div>
        <h2 class="text-3xl font-black tracking-tight text-surface-900 dark:text-surface-50">
          {{ t.titles.list }}
        </h2>
        <div class="flex items-center gap-2 mt-1">
          <span class="text-surface-500 dark:text-surface-400">
            {{ t.descriptions?.list }}
          </span>
          <Badge :value="totalRecords" severity="info" class="ml-2"></Badge>
        </div>
      </div>
      <div class="flex w-full gap-3 md:w-auto">
        <Button
          icon="pi pi-refresh"
          severity="secondary"
          outlined
          @click="loadProducts"
          :loading="loading"
          class="rounded-xl"
          v-tooltip.top="'Refresh'"
        />
        <Button
          :label="t.actions.new"
          icon="pi pi-plus"
          @click="router.push({ name: 'catalog.products.create' })"
          class="px-4 shadow-lg rounded-xl"
        />
      </div>
    </div>

    <div class="overflow-hidden border shadow-sm bg-surface-0 dark:bg-surface-900 rounded-2xl border-surface-100 dark:border-surface-800">
      <DataTable
        v-model:filters="filters"
        :value="products"
        :loading="loading"
        :totalRecords="totalRecords"
        :lazy="true"
        @page="onPage"
        @sort="onSort"
        @filter="onFilter"
        :paginator="true"
        :rows="query.page_size || 10"
        :first="((query.page || 1) - 1) * (query.page_size || 10)"
        :sortField="query.sort_by"
        :sortOrder="query.is_descending ? -1 : 1"
        filterDisplay="menu"
        removableSort
        scrollable
        rowHover
        stripedRows
        showGridlines
        dataKey="id"
        breakpoint="960px"
      >
        <template #header>
          <div class="flex flex-col items-center justify-between gap-4 md:flex-row">
            <IconField iconPosition="left" class="w-full md:w-72">
              <InputIcon class="pi pi-search" />
              <InputText
                v-model="(filters.global as any).value"
                :placeholder="t.placeholders?.search"
                @keyup.enter="onFilter"
                class="w-full rounded-xl"
              />
            </IconField>

            <Button
              type="button"
              icon="pi pi-filter-slash"
              :label="t.table?.clear_filter"
              outlined
              @click="clearFilters"
              class="w-full rounded-xl md:w-auto"
            />
          </div>
        </template>

        <template #empty>
          <div class="flex flex-col items-center justify-center py-20 text-surface-400">
            <i class="mb-4 text-6xl pi pi-shopping-bag opacity-20"></i>
            <p class="text-xl font-medium">{{ t.messages?.empty_list }}</p>
          </div>
        </template>

        <Column field="image_url" :header="t.table?.preview" class="w-24">
          <template #body="{ data }">
            <div class="w-14 h-14 rounded-xl overflow-hidden border border-surface-100 dark:border-surface-700 bg-surface-50 flex items-center justify-center">
                <Image v-if="data.image_url" :src="data.image_url" :alt="data.name" preview imageClass="w-full h-full object-cover" />
                <i v-else class="pi pi-image text-surface-300 text-xl"></i>
            </div>
          </template>
        </Column>

        <Column field="name" :header="t.table?.name" sortable filter>
            <template #body="{ data }">
                <span class="font-bold text-surface-900 dark:text-surface-0">{{ data.name }}</span>
            </template>
            <template #filter="{ filterModel, filterCallback }">
                <InputText v-model="filterModel.value" type="text" @keydown.enter="filterCallback()" class="p-column-filter" :placeholder="t.placeholders?.name" />
            </template>
        </Column>

        <Column field="sku" :header="t.table?.sku" sortable filter>
            <template #body="{ data }">
                <span class="font-mono text-xs uppercase tracking-widest text-surface-500">{{ data.sku || '-' }}</span>
            </template>
            <template #filter="{ filterModel, filterCallback }">
                <InputText v-model="filterModel.value" type="text" @keydown.enter="filterCallback()" class="p-column-filter" placeholder="Search SKU" />
            </template>
        </Column>

        <Column field="price" :header="t.table?.price" sortable>
            <template #body="{ data }">
                <span class="font-black">{{ formatCurrency(data.price) }}</span>
            </template>
        </Column>

        <Column field="variant_count" header="Variants" class="text-center w-24">
            <template #body="{ data }">
                <Badge :value="data.variant_count" severity="secondary" />
            </template>
        </Column>

        <Column field="is_active" :header="t.table?.status">
            <template #body="{ data }">
                <Tag :value="data.is_active ? 'Active' : 'Inactive'" :severity="data.is_active ? 'success' : 'secondary'" rounded class="font-bold px-3" />
            </template>
        </Column>

        <Column :header="t.table?.actions" class="w-32 text-right" frozen alignFrozen="right">
          <template #body="{ data }">
            <div class="flex justify-end gap-1">
              <Button icon="pi pi-pencil" severity="secondary" text rounded @click="router.push({ name: 'catalog.products.edit', params: { id: data.id } })" />
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