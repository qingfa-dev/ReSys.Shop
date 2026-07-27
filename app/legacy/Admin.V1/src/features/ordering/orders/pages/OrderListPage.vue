<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useOrderStore } from '../store/order.store';
import { storeToRefs } from 'pinia';
import { useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import { FilterMatchMode } from '@primevue/core/api';
import type {
  DataTablePageEvent,
  DataTableSortEvent,
  DataTableFilterMeta,
} from 'primevue/datatable';
import { getFilterValue } from '@/common/api/types/filter.types';
import { useFormatter } from '@/common/composables/formatter.use';
import PageShell from '@/shared/components/navigation/PageShell.vue';
import PageHeader from '@/shared/components/navigation/PageHeader.vue';
import DataTableShell from '@/shared/components/tables/DataTableShell.vue';
import type { ColumnDef } from '@/shared/components/tables/DataTableShell.vue';
import type { OrderListItemModel } from '../types/order.model';

const { t } = useI18n();

const store = useOrderStore();
const { orders, loading, totalRecords, query } = storeToRefs(store);
const router = useRouter();
const { formatDate } = useFormatter();

const filters = ref<DataTableFilterMeta>({
  global: { value: query.value.search || null, matchMode: FilterMatchMode.CONTAINS },
});

const columns: ColumnDef[] = [
  { field: 'number', header: t('ordering.table.number'), sortable: true },
  { field: 'email', header: t('ordering.table.customer'), sortable: true },
  { field: 'createdAtUtc', header: t('ordering.table.date'), sortable: true, body: (data) => formatDate(data.createdAtUtc) },
  { field: 'totalDisplay', header: t('ordering.table.total'), sortable: true },
  { field: 'statusLabel', header: t('ordering.table.status') },
];

const loadOrders = async () => {
  await store.fetchOrders();
};

const onPage = (event: DataTablePageEvent) => {
  store.fetchOrders({
    page: event.page !== undefined ? event.page + 1 : 1,
    pageSize: event.rows,
  });
};

const onSort = (event: DataTableSortEvent) => {
  store.fetchOrders({
    sort: [event.sortOrder === -1 ? `-${event.sortField as string}` : event.sortField as string],
    page: 1,
  });
};

const onFilter = () => {
  const globalValue = getFilterValue(filters.value, 'global') as string | null;

  store.fetchOrders({
    search: globalValue || undefined,
    searchFields: globalValue ? ['Number', 'Email'] : undefined,
    page: 1,
  });
};

onMounted(() => {
  loadOrders();
});
</script>

<template>
  <PageShell maxWidth="7xl">
    <PageHeader :title="t('ordering.titles.list')" :description="t('ordering.descriptions.list')">
      <template #badge>
        <Badge :value="totalRecords" severity="info" />
      </template>
    </PageHeader>

    <DataTableShell
      :columns="columns"
      :value="orders"
      :loading="loading"
      :total-records="totalRecords"
      :rows="query.pageSize || 10"
      :sort-field="query.sort?.[0]?.replace(/^-/, '')"
      :sort-order="query.sort?.[0]?.startsWith('-') ? -1 : 1"
      :search-placeholder="t('ordering.placeholders.search')"
      :empty-title="t('ordering.messages.empty_list')"
      :create-route="{ name: 'ordering.orders.create' }"
      :create-label="t('ordering.actions.new_order')"
      @page="onPage"
      @sort="onSort"
      @filter="onFilter"
      @refresh="loadOrders"
    >
      <template #row-actions="{ data }">
        <Button icon="pi pi-eye" severity="secondary" text rounded @click="router.push({ name: 'ordering.orders.detail', params: { id: data.id } })" />
      </template>
    </DataTableShell>
  </PageShell>
</template>
