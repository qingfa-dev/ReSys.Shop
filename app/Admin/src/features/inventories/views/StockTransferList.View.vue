<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useInventoryStore } from '../stores/inventory.store';
import { storeToRefs } from 'pinia';
import { useRouter } from 'vue-router';
import { useFormatter } from '@/shared/composables/formatter.use';
import { useI18n } from 'vue-i18n';
import AppBreadcrumb from '@/shared/components/Breadcrumb.Component.vue';
import { FilterMatchMode } from '@primevue/core/api';
import type { DataTablePageEvent, DataTableSortEvent, DataTableFilterMeta } from 'primevue/datatable';

const { t } = useI18n();

const store = useInventoryStore();
const { transfers, loading, totalTransfers, transferQuery } = storeToRefs(store);
const router = useRouter();
const { formatDate } = useFormatter();

onMounted(() => {
    store.fetchTransfers();
});

const onPage = (event: DataTablePageEvent) => {
    store.fetchTransfers({
        page: event.page !== undefined ? event.page + 1 : 1,
        pageSize: event.rows,
    });
};

const getStatusSeverity = (state: string) => {
    switch (state) {
        case 'Received': return 'success';
        case 'InTransit': return 'info';
        case 'Draft': return 'warning';
        case 'Canceled': return 'danger';
        default: return 'secondary';
    }
};
</script>

<template>
    <div class="p-6 max-w-7xl mx-auto">
        <AppBreadcrumb :locales="t" />
        <Card class="rounded-3xl shadow-sm border-none bg-surface-0 dark:bg-surface-900 overflow-hidden">
            <template #title>
                <div class="flex items-center justify-between p-4">
                    <div class="flex flex-col gap-1">
                        <div class="flex items-center gap-3">
                            <span class="text-xl font-bold">{{ t('inventory.titles.transfers') }}</span>
                            <Badge :value="totalTransfers" severity="info" />
                        </div>
                        <span class="text-sm text-surface-500">{{ t('inventory.descriptions.transfers') }}</span>
                    </div>
                    <Button :label="t('inventory.actions.new_transfer')" icon="pi pi-plus" severity="primary" class="rounded-xl" @click="router.push({ name: 'inventory.transfers.create' })" />
                </div>
            </template>
            <template #content>
                <DataTable 
                    :value="transfers" 
                    :loading="loading" 
                    :lazy="true" 
                    :paginator="true" 
                    :rows="transferQuery.pageSize || 10" 
                    :totalRecords="totalTransfers" 
                    @page="onPage"
                    dataKey="id"
                    rowHover
                    scrollable
                    stripedRows
                    showGridlines
                >
                    <template #empty>
                        <div class="flex flex-col items-center justify-center py-20 text-surface-400">
                            <i class="mb-4 text-6xl pi pi-arrow-right-arrow-left opacity-20"></i>
                            <p class="text-xl font-medium">No transfer history found.</p>
                        </div>
                    </template>

                    <Column field="referenceNumber" :header="t('inventory.table.reference')">
                        <template #body="{ data }">
                            <span class="font-mono font-bold">{{ data.referenceNumber }}</span>
                        </template>
                    </Column>

                    <Column :header="t('inventory.table.location')">
                        <template #body="{ data }">
                            <div class="flex items-center gap-3">
                                <span class="font-medium">{{ data.sourceLocationName }}</span>
                                <i class="pi pi-arrow-right text-surface-300"></i>
                                <span class="font-medium text-primary">{{ data.destinationLocationName }}</span>
                            </div>
                        </template>
                    </Column>

                    <Column field="state" :header="t('inventory.table.status')" class="text-center">
                        <template #body="{ data }">
                            <Tag :value="data.state" :severity="getStatusSeverity(data.state)" rounded class="px-3" />
                        </template>
                    </Column>

                    <Column field="createdAtUtc" header="Initiated">
                        <template #body="{ data }">
                            <span class="text-sm">{{ formatDate(data.createdAtUtc) }}</span>
                        </template>
                    </Column>

                    <Column :header="t('inventory.table.actions')" class="w-24 text-right">
                        <template #body="{ data }">
                            <Button icon="pi pi-eye" text rounded severity="secondary" v-tooltip.top="'View Details'" @click="router.push({ name: 'inventory.transfers.detail', params: { id: data.id } })" />
                        </template>
                    </Column>
                </DataTable>
            </template>
        </Card>
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
