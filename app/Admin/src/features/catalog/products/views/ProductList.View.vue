<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { useProductStore } from '../stores/product.store';
import { storeToRefs } from 'pinia';
import { useRouter } from 'vue-router';
import { useConfirm } from 'primevue/useconfirm';
import { FilterMatchMode, FilterOperator as PrimeFilterOperator } from '@primevue/core/api';
import type {
  DataTablePageEvent,
  DataTableSortEvent,
  DataTableFilterMeta,
} from 'primevue/datatable';
import { getFilterValue } from '@/shared/api/types/filter.types';
import { useToast } from '@/shared/composables/toast.use';
import { useFormatter } from '@/shared/composables/formatter.use';
import { QueryBuilder, type FilterOperator } from '@/shared/utils/query-builder.utils';
import PageShell from '@/shared/components/PageShell.Component.vue';
import PageHeader from '@/shared/components/PageHeader.Component.vue';
import type { ProductSummary } from '../types/Product.Response.Type';

const { t } = useI18n();

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
});

// --- DATA ACTIONS ---

const loadProducts = async () => {
  await store.fetchProducts();
};

const onPage = (event: DataTablePageEvent) => {
  store.fetchProducts({
    page: event.page !== undefined ? event.page + 1 : 1,
    pageSize: event.rows,
  });
};

const onSort = (event: DataTableSortEvent) => {
  const builder = new QueryBuilder();
  if (event.sortField) {
    builder.orderBy(event.sortField as string, event.sortOrder === -1 ? 'desc' : 'asc');
  }

  store.fetchProducts({
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

  store.fetchProducts({
    search: globalValue || undefined,
    searchFields: globalValue ? ['Name', 'Description', 'Slug', 'StyleCode'] : undefined,
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
  };
  onFilter();
};

const confirmDelete = (product: ProductSummary) => {
  const messageStr = t('catalog.products.confirm.delete_message').replace('{name}', product.name);
  
  confirm.require({
    message: messageStr,
    header: t('catalog.products.confirm.delete_header'),
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: t('catalog.products.confirm.reject_label'),
    acceptProps: {
      label: t('catalog.products.confirm.accept_label'),
      severity: 'danger',
    },
    accept: async () => {
      const result = await store.deleteProduct(product.id);
      if (result.isSuccess) {
        showToast('success', t('common.success'), t('catalog.products.messages.delete_success'));
      }
    },
  });
};

onMounted(() => {
  loadProducts();
});
</script>

<template>
  <PageShell maxWidth="7xl">
    <PageHeader :title="t('catalog.products.titles.list')" :description="t('catalog.products.descriptions.list')">
      <template #badge>
        <Badge :value="totalRecords" severity="info" class="ml-2"></Badge>
      </template>
      <template #actions>
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
          :label="t('catalog.products.actions.new')"
          icon="pi pi-plus"
          @click="router.push({ name: 'catalog.products.create' })"
          class="px-4 shadow-lg rounded-xl"
        />
      </template>
    </PageHeader>

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
      :rows="query.pageSize || 10"
      :first="((query.page || 1) - 1) * (query.pageSize || 10)"
      :sortField="query.sort?.[0]?.replace(/^-/, '')"
      :sortOrder="query.sort?.[0]?.startsWith('-') ? -1 : 1"
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
              v-model="(filters.global as { value: string | null }).value"
              :placeholder="t('catalog.products.placeholders.search')"
              @keyup.enter="onFilter"
              class="w-full rounded-xl"
            />
          </IconField>

          <Button
            type="button"
            icon="pi pi-filter-slash"
            :label="t('catalog.products.table.clear_filter')"
            outlined
            @click="clearFilters"
            class="w-full rounded-xl md:w-auto"
          />
        </div>
      </template>

      <template #empty>
        <div class="flex flex-col items-center justify-center py-20 text-surface-400">
          <i class="mb-4 text-6xl pi pi-shopping-bag opacity-20"></i>
          <p class="text-xl font-medium">{{ t('catalog.products.messages.empty_list') }}</p>
        </div>
      </template>

      <Column field="imageUrl" :header="t('catalog.products.table.preview')" class="w-24">
        <template #body="{ data }">
          <div class="w-14 h-14 rounded-xl overflow-hidden border border-surface-100 dark:border-surface-700 bg-surface-50 flex items-center justify-center">
              <Image v-if="data.imageUrl" :src="data.imageUrl" :alt="data.name" preview imageClass="w-full h-full object-cover" />
              <i v-else class="pi pi-image text-surface-300 text-xl"></i>
          </div>
        </template>
      </Column>

      <Column field="name" :header="t('catalog.products.table.name')" sortable filter>
          <template #body="{ data }">
              <span class="font-bold text-surface-900 dark:text-surface-0">{{ data.name }}</span>
          </template>
          <template #filter="{ filterModel, filterCallback }">
              <InputText v-model="filterModel.value" type="text" @keydown.enter="filterCallback()" class="p-column-filter" :placeholder="t('catalog.products.placeholders.name')" />
          </template>
      </Column>

      <Column field="sku" :header="t('catalog.products.table.sku')" filter>
          <template #body="{ data }">
              <span class="font-mono text-xs uppercase tracking-widest text-surface-500">{{ data.sku || '-' }}</span>
          </template>
          <template #filter="{ filterModel, filterCallback }">
              <InputText v-model="filterModel.value" type="text" @keydown.enter="filterCallback()" class="p-column-filter" placeholder="Search SKU" />
          </template>
      </Column>

      <Column field="price" :header="t('catalog.products.table.price')">
          <template #body="{ data }">
              <span class="font-black">{{ formatCurrency(data.price) }}</span>
          </template>
      </Column>

      <Column field="variantsCount" :header="t('catalog.products.table.variants')" class="text-center w-24">
          <template #body="{ data }">
              <Badge :value="data.variantsCount" severity="secondary" />
          </template>
      </Column>

      <Column field="status" :header="t('catalog.products.table.status')">
          <template #body="{ data }">
              <Tag :value="data.status" :severity="data.status === 'Active' ? 'success' : 'secondary'" rounded class="font-bold px-3" />
          </template>
      </Column>

      <Column :header="t('catalog.products.table.actions')" class="w-32 text-right" frozen alignFrozen="right">
        <template #body="{ data }">
          <div class="flex justify-end gap-1">
            <Button icon="pi pi-pencil" severity="secondary" text rounded @click="router.push({ name: 'catalog.products.edit', params: { id: data.id } })" />
            <Button icon="pi pi-trash" severity="danger" text rounded @click="confirmDelete(data)" />
          </div>
        </template>
      </Column>
    </DataTable>
  </PageShell>
</template>
