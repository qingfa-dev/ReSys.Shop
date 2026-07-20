<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useInventoryStore } from '../../stores/inventory.store';
import { storeToRefs } from 'pinia';
import { useFormatter } from '@/common/composables/formatter.use';
import { useI18n } from 'vue-i18n';
import PageShell from '@/shared/components/PageShell.Component.vue'
import PageHeader from '@/shared/components/PageHeader.Component.vue'
import { FilterMatchMode } from '@primevue/core/api';
import type { DataTablePageEvent, DataTableSortEvent, DataTableFilterMeta } from 'primevue/datatable';
import type { InventoryUnit } from '../types/inventory-unit.response.type';

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

const getStatusSeverity = (state: number) => {
    switch (state) {
        case 0: return 'success';
        case 1: return 'info';
        case 2: return 'secondary';
        case 3: return 'danger';
        case 4: return 'warning';
        case 5: return 'contrast';
        default: return 'secondary';
    }
};

const statusLabel = (state: number) => {
    switch (state) {
        case 0: return 'Available';
        case 1: return 'Reserved';
        case 2: return 'Shipped';
        case 3: return 'Damaged';
        case 4: return 'Returned';
        case 5: return 'Sold';
        default: return 'Unknown';
    }
};
</script>

<template>
    <PageShell maxWidth="7xl">
        <PageHeader
          title="Serialized Units"
          description="Track individual items by serial number and lifecycle state."
        />

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

                <Column field="stockItemId" header="Stock Item" sortable>
                    <template #body="{ data }">
                        <span class="font-mono text-xs font-bold">{{ data.stockItemId }}</span>
                    </template>
                </Column>

                <Column field="state" :header="t('inventory.table.status')" sortable class="text-center">
                    <template #body="{ data }">
                        <Tag :value="statusLabel(data.state)" :severity="getStatusSeverity(data.state)" rounded class="px-3" />
                    </template>
                </Column>

                <Column field="createdAtUtc" :header="t('inventory.table.registered')" sortable>
                    <template #body="{ data }">
                        <span class="text-sm">{{ formatDate(data.createdAtUtc) }}</span>
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
    </PageShell>
</template>
