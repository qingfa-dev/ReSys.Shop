<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useInventoryStore } from '../stores/inventory.store';
import { storeToRefs } from 'pinia';
import { useRouter } from 'vue-router';
import { useFormatter } from '@/shared/composables/formatter.use';
import { useI18n } from 'vue-i18n';
import PageShell from '@/shared/components/PageShell.Component.vue';
import PageHeader from '@/shared/components/PageHeader.Component.vue';
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
    <PageShell maxWidth="7xl">
        <PageHeader :title="t('inventory.titles.transfers')" :description="t('inventory.descriptions.transfers')">
            <template #badge>
                <Badge :value="totalTransfers" severity="info" />
            </template>
            <template #actions>
                <Button :label="t('inventory.actions.new_transfer')" icon="pi pi-plus" severity="primary" class="rounded-xl" @click="router.push({ name: 'inventory.transfers.create' })" />
            </template>
        </PageHeader>
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

                    <Column field="createdAtUtc" :header="t('inventory.table.initiated')">
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
    </PageShell>
</template>
