<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { useProductStore } from '../stores/product.store';
import { storeToRefs } from 'pinia';
import { useRouter } from 'vue-router';
import { useConfirm } from 'primevue/useconfirm';
import { FilterMatchMode } from '@primevue/core/api';
import type {
  DataTablePageEvent,
  DataTableSortEvent,
  DataTableFilterMeta,
} from 'primevue/datatable';
import { getFilterValue } from '@/common/api/types/filter.types';
import { useToast } from '@/common/composables/toast.use';
import PageShell from '@/shared/components/navigation/PageShell.vue';
import PageHeader from '@/shared/components/navigation/PageHeader.vue';
import DataTableShell from '@/shared/components/tables/DataTableShell.vue';
import type { ColumnDef } from '@/shared/components/tables/DataTableShell.vue';
import type { ProductSummaryModel } from '../types/product.model.type';

const { t } = useI18n();

const store = useProductStore();
const { products, loading, totalRecords, query } = storeToRefs(store);
const router = useRouter();
const confirm = useConfirm();
const { showToast } = useToast();

const filters = ref<DataTableFilterMeta>({
  global: { value: query.value.search || null, matchMode: FilterMatchMode.CONTAINS },
});

const columns: ColumnDef[] = [
  { field: 'name', header: t('catalog.products.table.name'), sortable: true },
  { field: 'variantsCount', header: t('catalog.products.table.variants'), class: 'text-center w-24' },
  { field: 'statusLabel', header: t('catalog.products.table.status') },
];

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
  store.fetchProducts({
    sort: [event.sortOrder === -1 ? `-${event.sortField as string}` : event.sortField as string],
    page: 1,
  });
};

const onFilter = () => {
  const globalValue = getFilterValue(filters.value, 'global') as string | null;

  store.fetchProducts({
    search: globalValue || undefined,
    searchFields: globalValue ? ['Name', 'Description', 'Slug', 'StyleCode'] : undefined,
    page: 1,
  });
};

const confirmDelete = (product: ProductSummaryModel) => {
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
        <Badge :value="totalRecords" severity="info" class="ml-2" />
      </template>
    </PageHeader>

    <DataTableShell
      :columns="columns"
      :value="products"
      :loading="loading"
      :total-records="totalRecords"
      :rows="query.pageSize || 10"
      :sort-field="query.sort?.[0]?.replace(/^-/, '')"
      :sort-order="query.sort?.[0]?.startsWith('-') ? -1 : 1"
      :search-placeholder="t('catalog.products.placeholders.search')"
      :empty-title="t('catalog.products.messages.empty_list')"
      :create-route="{ name: 'catalog.products.create' }"
      :create-label="t('catalog.products.actions.new')"
      @page="onPage"
      @sort="onSort"
      @filter="onFilter"
      @refresh="loadProducts"
    >
      <template #row-actions="{ data }">
        <Button icon="pi pi-pencil" severity="secondary" text rounded @click="router.push({ name: 'catalog.products.edit', params: { id: data.id } })" />
        <Button icon="pi pi-trash" severity="danger" text rounded @click="confirmDelete(data)" />
      </template>
    </DataTableShell>
  </PageShell>
</template>
