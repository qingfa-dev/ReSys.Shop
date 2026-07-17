<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useUserStore } from '../stores/user.store';
import { storeToRefs } from 'pinia';
import { useRouter } from 'vue-router';
import { useFormatter } from '@/shared/composables/formatter.use';
import { useI18n } from 'vue-i18n';
import AppBreadcrumb from '@/shared/components/Breadcrumb.Component.vue';
import { FilterMatchMode } from '@primevue/core/api';
import type { DataTablePageEvent, DataTableSortEvent, DataTableFilterMeta } from 'primevue/datatable';

const store = useUserStore();
const { customers, loading, totalRecords, query } = storeToRefs(store);
const router = useRouter();
const { formatCurrency, formatDate } = useFormatter();
const { t } = useI18n();

const filters = ref<DataTableFilterMeta>({
  global: { value: query.value.search || null, matchMode: FilterMatchMode.CONTAINS },
});

onMounted(() => {
    store.fetchCustomers();
});

const onPage = (event: DataTablePageEvent) => {
    store.fetchCustomers({
        page: event.page !== undefined ? event.page + 1 : 1,
        pageSize: event.rows,
    });
};

const onSort = (event: DataTableSortEvent) => {
    store.fetchCustomers({
        sort: [event.sortOrder === -1 ? `-${event.sortField as string}` : event.sortField as string],
        page: 1,
    });
};

const onFilter = () => {
    const globalFilter = filters.value.global as { value: string | null };
    store.fetchCustomers({
        search: globalFilter.value || undefined,
        page: 1,
    });
};

const clearFilters = () => {
    filters.value = {
        global: { value: null, matchMode: FilterMatchMode.CONTAINS },
    };
    onFilter();
};
</script>

<template>
    <Card>
        <template #content>
            <AppBreadcrumb />
        
        <div class="flex flex-col md:flex-row md:items-center justify-between gap-4 mt-4 mb-8">
            <div>
                <h2 class="text-4xl font-black tracking-tighter text-surface-900 dark:text-surface-50 m-0">
                    {{ t('users.titles.customers') }}
                </h2>
                <p class="text-surface-500 m-0">{{ t('users.descriptions.customers') }}</p>
            </div>
            <div class="flex items-center gap-2">
                <Badge :value="totalRecords" severity="info" class="px-3" />
                <span class="text-sm font-bold uppercase text-surface-400">Total Registered</span>
            </div>
        </div>

        <div class="overflow-hidden border shadow-sm bg-surface-0 dark:bg-surface-900 rounded-3xl border-surface-100 dark:border-surface-800">
            <DataTable 
                v-model:filters="filters"
                :value="customers" 
                :loading="loading" 
                :lazy="true" 
                :paginator="true" 
                :rows="query.pageSize || 10" 
                :totalRecords="totalRecords" 
                @page="onPage"
                @sort="onSort"
                @filter="onFilter"
                dataKey="id"
                rowHover
                scrollable
                :first="((query.page || 1) - 1) * (query.pageSize || 10)"
                :sortField="query.sort?.[0]?.replace(/^-/, '')"
                :sortOrder="query.sort?.[0]?.startsWith('-') ? -1 : 1"
                filterDisplay="menu"
                removableSort
                stripedRows
                showGridlines
            >
                <template #header>
                    <div class="flex flex-col items-center justify-between gap-4 md:flex-row p-2">
                        <IconField iconPosition="left" class="w-full md:w-72">
                            <InputIcon class="pi pi-search" />
                            <InputText
                                v-model="(filters.global as any).value"
                                :placeholder="t('users.placeholders.search')"
                                @keyup.enter="onFilter"
                                class="w-full rounded-xl"
                            />
                        </IconField>

                        <Button
                            type="button"
                            icon="pi pi-filter-slash"
                            :label="t('users.table.clear_filter')"
                            outlined
                            @click="clearFilters"
                            class="w-full rounded-xl md:w-auto"
                        />
                    </div>
                </template>

                <Column field="fullName" :header="t('users.table.user')" sortable>
                    <template #body="{ data }">
                        <div class="flex flex-col">
                            <span class="font-bold text-surface-900 dark:text-surface-0">{{ data.fullName || 'Anonymous' }}</span>
                            <small class="font-mono text-[10px] text-surface-500 uppercase tracking-widest">{{ data.email }}</small>
                        </div>
                    </template>
                </Column>

                <Column field="ordersCount" :header="t('users.table.orders')" sortable class="text-center">
                    <template #body="{ data }">
                        <Badge :value="data.ordersCount || 0" severity="secondary" class="font-bold" />
                    </template>
                </Column>

                <Column field="totalSpent" :header="t('users.table.total_spent')" sortable>
                    <template #body="{ data }">
                        <span class="font-black text-primary">{{ formatCurrency((data.totalSpent || 0) / 100) }}</span>
                    </template>
                </Column>

                <Column field="createdAtUtc" :header="t('users.table.joined')" sortable>
                    <template #body="{ data }">
                        <span class="text-sm">{{ formatDate(data.createdAtUtc) }}</span>
                    </template>
                </Column>

                <Column field="isActive" :header="t('users.table.status')">
                    <template #body="{ data }">
                        <Tag :value="data.isActive ? 'Active' : 'Inactive'" :severity="data.isActive ? 'success' : 'secondary'" rounded class="font-bold px-3" />
                    </template>
                </Column>

                <Column :header="t('users.table.actions')" class="w-32 text-right" frozen alignFrozen="right">
                    <template #body="{ data }">
                        <Button icon="pi pi-eye" text rounded @click="router.push({ name: 'customer-detail', params: { id: data.id } })" />
                    </template>
                </Column>
            </DataTable>
        </div>
    </template>
</Card>
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
