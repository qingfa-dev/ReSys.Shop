<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useOrderStore } from '../stores/order.store';
import { storeToRefs } from 'pinia';
import { useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import { FilterMatchMode, FilterOperator as PrimeFilterOperator } from '@primevue/core/api';
import type {
  DataTablePageEvent,
  DataTableSortEvent,
  DataTableFilterMeta,
} from 'primevue/datatable';
import { useFormatter } from '@/shared/composables/formatter.use';
import { QueryBuilder } from '@/shared/utils/query-builder.utils';
import PageShell from '@/shared/components/PageShell.Component.vue';
import PageHeader from '@/shared/components/PageHeader.Component.vue';

// --- LOCALES & ALIASES ---
const { t } = useI18n();

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
    pageSize: event.rows,
  });
};

const onSort = (event: DataTableSortEvent) => {
  const builder = new QueryBuilder();
  if (event.sortField) {
    builder.orderBy(event.sortField as string, event.sortOrder === -1 ? 'desc' : 'asc');
  }

  store.fetchOrders({
    sort: [event.sortOrder === -1 ? `-${event.sortField as string}` : event.sortField as string],
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
    searchFields: globalFilter.value ? ['Number', 'Email'] : undefined,
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
  <PageShell>
    <PageHeader :title="t('ordering.titles.list')" :description="t('ordering.descriptions.list')">
      <template #badge>
        <Badge :value="totalRecords" severity="info" />
      </template>
      <template #actions>
        <Button :label="t('ordering.actions.new_order')" icon="pi pi-plus" severity="primary" class="rounded-xl" @click="router.push({ name: 'ordering.orders.create' })" />
      </template>
    </PageHeader>
    <DataTable 
            :value="orders" 
            :loading="loading" 
            lazy 
            paginator 
            :rows="query.pageSize" 
            :totalRecords="totalRecords" 
            @page="onPage"
            @sort="onSort"
            @filter="onFilter"
            dataKey="id"
            responsiveLayout="stack"
            breakpoint="960px"
            :first="((query.page || 1) - 1) * (query.pageSize || 10)"
            :sortField="query.sort?.[0]?.replace(/^-/, '')"
            :sortOrder="query.sort?.[0]?.startsWith('-') ? -1 : 1"
            filterDisplay="menu"
            removableSort
            scrollable
            rowHover
            stripedRows
            showGridlines
        >
          <template #header>
            <div class="flex items-center justify-between gap-4">
              <IconField iconPosition="left" class="w-full md:w-72">
                <InputIcon class="pi pi-search" />
                <InputText
                  v-model="(filters.global as any).value"
                  :placeholder="t('ordering.placeholders.search')"
                  @keyup.enter="onFilter"
                  class="w-full rounded-xl"
                />
              </IconField>
              <Button
                type="button"
                icon="pi pi-filter-slash"
                :label="t('ordering.table.clear_filter')"
                outlined
                @click="clearFilters"
                class="rounded-xl"
              />
            </div>
          </template>

          <Column field="number" :header="t('ordering.table.number')" sortable filter>
            <template #body="{ data }">
              <span class="font-black text-primary cursor-pointer hover:underline" @click="router.push({ name: 'ordering.orders.detail', params: { id: data.id } })">
                {{ data.number }}
              </span>
            </template>
          </Column>

          <Column field="email" :header="t('ordering.table.customer')" sortable>
            <template #body="{ data }">
              <div class="flex flex-col">
                <span class="font-bold">{{ data.email || 'Guest' }}</span>
              </div>
            </template>
          </Column>

          <Column field="createdAtUtc" :header="t('ordering.table.date')" sortable>
            <template #body="{ data }">
              <span class="text-sm font-medium">{{ formatDate(data.createdAtUtc) }}</span>
            </template>
          </Column>

          <Column field="totalCents" :header="t('ordering.table.total')" sortable>
            <template #body="{ data }">
              <span class="font-black text-lg">{{ formatCurrency(data.totalCents / 100) }}</span>
            </template>
          </Column>

          <Column field="state" :header="t('ordering.table.status')" filter>
            <template #body="{ data }">
              <Tag :value="data.state" :severity="getStatusSeverity(data.state)" rounded class="font-black px-3" />
            </template>
          </Column>

          <Column :header="t('ordering.table.actions')" class="w-32 text-right" frozen alignFrozen="right">
            <template #body="{ data }">
              <Button icon="pi pi-eye" severity="secondary" text rounded @click="router.push({ name: 'ordering.orders.detail', params: { id: data.id } })" />
            </template>
          </Column>
        </DataTable>
  </PageShell>
</template>
