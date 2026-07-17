<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useUserStore } from '../stores/user.store';
import { storeToRefs } from 'pinia';
import { useRouter } from 'vue-router';
import { useFormatter } from '@/shared/composables/formatter.use';
import { useI18n } from 'vue-i18n';
import PageShell from '@/shared/components/PageShell.Component.vue';
import PageHeader from '@/shared/components/PageHeader.Component.vue';
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
        searchFields: globalFilter.value ? ['UserName', 'Email', 'FirstName', 'LastName'] : undefined,
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
    <PageShell maxWidth="7xl">
        <PageHeader :title="t('users.titles.customers')" :description="t('users.descriptions.customers')">
            <template #actions>
                <div class="flex items-center gap-2">
                    <Badge :value="totalRecords" severity="info" class="px-3" />
                    <span class="text-sm font-bold uppercase text-surface-400">Total Registered</span>
                </div>
            </template>
        </PageHeader>

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
    </PageShell>
</template>
