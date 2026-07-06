<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useOrderStore } from '../stores/order.store';
import { storeToRefs } from 'pinia';
import { useRouter } from 'vue-router';
import { orderLocales } from '../locales/order.locales';
import { FilterMatchMode, FilterOperator as PrimeFilterOperator } from '@primevue/core/api';
import type {
  DataTablePageEvent,
  DataTableSortEvent,
  DataTableFilterMeta,
} from 'primevue/datatable';
import { useFormatter } from '@/shared/composables/formatter.use';
import { QueryBuilder } from '@/shared/utils/query-builder.utils';
import AppBreadcrumb from '@/shared/components/breadcrumb.component.vue';
import type { FeatureLocales } from '@/shared/locales/locale.types';

// --- LOCALES & ALIASES ---
const t = orderLocales as Required<FeatureLocales>;

// --- STORE & ROUTING ---
const store = useOrderStore();
const { orders, loading, totalRecords, query } = storeToRefs(store);
const router = useRouter();
const { formatCurrency, formatDate } = useFormatter();

/**
 * PrimeVue Filter Configuration
 */
const filters = ref<DataTableFilterMeta>({
  global: { value: query.value.search || null, matchMode: FilterMatchMode.CONTAINS },
  number: {
    operator: PrimeFilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.CONTAINS }],
  },
  state: {
    operator: PrimeFilterOperator.AND,
    constraints: [{ value: null, matchMode: FilterMatchMode.EQUALS }],
  }
});

// --- DATA ACTIONS ---

const loadOrders = async () => {
  await store.fetchOrders();
};

const onPage = (event: DataTablePageEvent) => {
  store.fetchOrders({
    page: event.page !== undefined ? event.page + 1 : 1,
    page_size: event.rows,
  });
};

const onSort = (event: DataTableSortEvent) => {
  const builder = new QueryBuilder();
  if (event.sortField) {
    builder.orderBy(event.sortField as string, event.sortOrder === -1 ? 'desc' : 'asc');
  }

  store.fetchOrders({
    sort_by: event.sortField as string,
    is_descending: event.sortOrder === -1,
    page: 1,
  });
};

const onFilter = () => {
  const globalFilter = filters.value.global as { value: string | null };
  const numberFilter = filters.value.number as { constraints: { value: string | null }[] };
  const stateFilter = filters.value.state as { constraints: { value: string | null }[] };

  const builder = new QueryBuilder();

  if (numberFilter.constraints[0]?.value) {
    builder.where('Number', '*', numberFilter.constraints[0].value);
  }
  
  if (stateFilter.constraints[0]?.value) {
    builder.where('State', '=', stateFilter.constraints[0].value);
  }

  const built = builder.build();

  store.fetchOrders({
    search: globalFilter.value || undefined,
    filter: built.filter,
    page: 1,
  });
};

const clearFilters = () => {
  filters.value = {
    global: { value: null, matchMode: FilterMatchMode.CONTAINS },
    number: {
      operator: PrimeFilterOperator.AND,
      constraints: [{ value: null, matchMode: FilterMatchMode.CONTAINS }],
    },
    state: {
      operator: PrimeFilterOperator.AND,
      constraints: [{ value: null, matchMode: FilterMatchMode.EQUALS }],
    }
  };
  onFilter();
};

const getStatusSeverity = (status: string) => {
    switch (status?.toLowerCase()) {
        case 'complete': return 'success';
        case 'processing': return 'info';
        case 'canceled': return 'danger';
        case 'payment_required': return 'warn';
        default: return 'secondary';
    }
};

onMounted(() => {
  loadOrders();
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
      <div class="flex items-center gap-3">
        <Button label="New Order" icon="pi pi-plus" class="rounded-xl px-6 shadow-lg shadow-primary/20" @click="router.push({ name: 'ordering.orders.create' })" />
      </div>
    </div>

    <div class="overflow-hidden border shadow-sm bg-surface-0 dark:bg-surface-900 rounded-2xl border-surface-100 dark:border-surface-800">
        <DataTable 
            :value="orders" 
            :loading="loading" 
            lazy 
            paginator 
            :rows="query.page_size" 
            :totalRecords="totalRecords" 
            @page="onPage"
            @sort="onSort"
            @filter="onFilter"
            dataKey="id"
            responsiveLayout="stack"
            breakpoint="960px"
            :first="((query.page || 1) - 1) * (query.page_size || 10)"
            :sortField="query.sort_by"
            :sortOrder="query.is_descending ? -1 : 1"
            filterDisplay="menu"
            removableSort
            scrollable
            rowHover
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

        <Column field="number" :header="t.table.number" sortable filter>
            <template #body="{ data }">
                <span class="font-black text-primary cursor-pointer hover:underline" @click="router.push({ name: 'ordering.orders.detail', params: { id: data.id } })">
                    {{ data.number }}
                </span>
            </template>
        </Column>

        <Column field="email" :header="t.table.customer" sortable>
            <template #body="{ data }">
                <div class="flex flex-col">
                    <span class="font-bold">{{ data.email || 'Guest' }}</span>
                </div>
            </template>
        </Column>

        <Column field="created_at" :header="t.table.date" sortable>
            <template #body="{ data }">
                <span class="text-sm font-medium">{{ formatDate(data.created_at) }}</span>
            </template>
        </Column>

        <Column field="total_cents" :header="t.table.total" sortable>
            <template #body="{ data }">
                <span class="font-black text-lg">{{ formatCurrency(data.total_cents / 100) }}</span>
            </template>
        </Column>

        <Column field="state" :header="t.table.status" filter>
            <template #body="{ data }">
                <Tag :value="data.state" :severity="getStatusSeverity(data.state)" rounded class="font-black px-3" />
            </template>
        </Column>

        <Column :header="t.table.actions" class="w-32 text-right" frozen alignFrozen="right">
          <template #body="{ data }">
            <Button icon="pi pi-eye" severity="secondary" text rounded @click="router.push({ name: 'ordering.orders.detail', params: { id: data.id } })" />
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