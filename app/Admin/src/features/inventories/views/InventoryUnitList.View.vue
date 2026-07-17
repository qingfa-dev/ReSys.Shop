<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useInventoryStore } from '../stores/inventory.store';
import { storeToRefs } from 'pinia';
import { useFormatter } from '@/shared/composables/formatter.use';
import { useI18n } from 'vue-i18n';
import AppBreadcrumb from '@/shared/components/Breadcrumb.Component.vue';
import { FilterMatchMode } from '@primevue/core/api';
import type { DataTablePageEvent, DataTableSortEvent, DataTableFilterMeta } from 'primevue/datatable';
import type { InventoryUnit } from '../types/inventory.domain.types';

const { t } = useI18n();

const store = useInventoryStore();
const { units, loading, totalUnits, unitQuery } = storeToRefs(store);
const { formatDate } = useFormatter();

onMounted(() => {
    store.fetchUnits();
});

const onPage = (event: DataTablePageEvent) => {
    store.fetchUnits({
        page: event.page !== undefined ? event.page + 1 : 1,
        pageSize: event.rows,
    });
};

const onSort = (event: DataTableSortEvent) => {
    store.fetchUnits({
        sort: [event.sortOrder === -1 ? `-${event.sortField as string}` : event.sortField as string],
        page: 1,
    });
};

const getStatusSeverity = (state: string) => {
    switch (state) {
        case 'Available': return 'success';
        case 'Reserved': return 'info';
        case 'Shipped': return 'secondary';
        case 'Damaged': return 'danger';
        case 'Returned': return 'warning';
        case 'Sold': return 'contrast';
        default: return 'secondary';
    }
};
</script>

<template>
    <div class="p-6 max-w-7xl mx-auto">
        <AppBreadcrumb :locales="t" />
        
        <div class="flex flex-col md:flex-row md:items-center justify-between gap-4 mt-4 mb-8">
            <div>
                <h2 class="text-4xl font-black tracking-tighter text-surface-900 dark:text-surface-50 m-0">
                    Serialized Units
                </h2>
                <p class="text-surface-500 m-0">Track individual items by serial number and lifecycle state.</p>
            </div>
        </div>

        <div class="overflow-hidden border shadow-sm bg-surface-0 dark:bg-surface-900 rounded-3xl border-surface-100 dark:border-surface-800">
            <DataTable 
                :value="units" 
                :loading="loading" 
                :lazy="true" 
                :paginator="true" 
                :rows="unitQuery.pageSize || 20" 
                :totalRecords="totalUnits" 
                @page="onPage"
                @sort="onSort"
                dataKey="id"
                rowHover
                :first="((unitQuery.page || 1) - 1) * (unitQuery.pageSize || 20)"
                :sortField="unitQuery.sort?.[0]?.replace(/^-/, '')"
                :sortOrder="unitQuery.sort?.[0]?.startsWith('-') ? -1 : 1"
                removableSort
                scrollable
            >
                <template #empty>
                    <div class="flex flex-col items-center justify-center py-20 text-surface-400">
                        <i class="mb-4 text-6xl pi pi-barcode opacity-20"></i>
                        <p class="text-xl font-medium">No serialized units found.</p>
                    </div>
                </template>

                <Column field="sku" :header="t('inventory.table.sku')" sortable>
                    <template #body="{ data }">
                        <span class="font-mono text-xs font-bold">{{ data.sku }}</span>
                    </template>
                </Column>

                <Column field="serial_number" header="Serial Number" sortable>
                    <template #body="{ data }">
                        <span v-if="data.serial_number" class="font-mono text-sm bg-surface-100 dark:bg-surface-800 px-2 py-1 rounded">
                            {{ data.serial_number }}
                        </span>
                        <span v-else class="text-surface-400 italic">Not Assigned</span>
                    </template>
                </Column>

                <Column field="state" :header="t('inventory.table.status')" sortable class="text-center">
                    <template #body="{ data }">
                        <Tag :value="data.state" :severity="getStatusSeverity(data.state)" rounded class="px-3" />
                    </template>
                </Column>

                <Column field="created_at" header="Registered" sortable>
                    <template #body="{ data }">
                        <span class="text-sm">{{ formatDate(data.created_at) }}</span>
                    </template>
                </Column>

                <Column :header="t('inventory.table.actions')" class="w-24 text-right" frozen alignFrozen="right">
                    <template #body="{ data }">
                        <div class="flex justify-end gap-1">
                            <Button icon="pi pi-pencil" severity="secondary" text rounded v-tooltip.top="'Edit Serial'" />
                            <Button v-if="data.state !== 'Damaged'" icon="pi pi-exclamation-circle" severity="danger" text rounded v-tooltip.top="'Mark Damaged'" />
                        </div>
                    </template>
                </Column>
            </DataTable>
        </div>
    </div>
</template>

<style scoped>
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
