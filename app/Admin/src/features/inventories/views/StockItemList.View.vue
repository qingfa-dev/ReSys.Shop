<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useInventoryStore } from '../stores/inventory.store';
import { storeToRefs } from 'pinia';
import { useFormatter } from '@/shared/composables/formatter.use';
import { useI18n } from 'vue-i18n';
import PageShell from '@/shared/components/PageShell.Component.vue';
import PageHeader from '@/shared/components/PageHeader.Component.vue';
import StockMovementTimeline from '../components/StockMovementTimeline.Component.vue';
import StockAdjustmentDialog from '../components/StockAdjustmentDialog.Component.vue';
import type { DataTablePageEvent, DataTableSortEvent, DataTableFilterMeta } from 'primevue/datatable';

const { t } = useI18n();

const store = useInventoryStore();
const { stocks, loading, totalStocks, stockQuery } = storeToRefs(store);
const { formatDate } = useFormatter();

const filters = ref<DataTableFilterMeta>({});

const historyDrawer = ref(false);
const adjustDialog = ref(false);
const selectedStockItem = ref<any>(null);
const selectedStockId = ref<string | undefined>(undefined);
const selectedSku = ref('');

const showHistory = (data: any) => {
    selectedStockId.value = data.id;
    selectedSku.value = data.sku;
    historyDrawer.value = true;
};

const showAdjust = (data: any) => {
    selectedStockItem.value = data;
    adjustDialog.value = true;
};

onMounted(() => {
    store.fetchStocks();
});

const onPage = (event: DataTablePageEvent) => {
    store.fetchStocks({
        page: event.page !== undefined ? event.page + 1 : 1,
        pageSize: event.rows,
    });
};

const onSort = (event: DataTableSortEvent) => {
    store.fetchStocks({
        sort: [event.sortOrder === -1 ? `-${event.sortField as string}` : event.sortField as string],
        page: 1,
    });
};

const onFilter = () => {
    store.fetchStocks({ page: 1 });
};

const clearFilters = () => {
    filters.value = {};
    stockQuery.value.lowStock = false;
    onFilter();
};

const toggleLowStock = () => {
    stockQuery.value.lowStock = !stockQuery.value.lowStock;
    store.fetchStocks();
};
</script>

<template>
    <PageShell maxWidth="7xl">
        <PageHeader :title="t('inventory.titles.list')" :description="t('inventory.descriptions.list')">
            <template #badge>
                <Badge :value="totalStocks" severity="info" />
            </template>
            <template #actions>
                <Button :label="t('inventory.actions.new_transfer')" icon="pi pi-arrow-right-arrow-left" severity="secondary" outlined class="rounded-xl" />
            </template>
        </PageHeader>
        <DataTable 
                    v-model:filters="filters"
                    :value="stocks" 
                    :loading="loading" 
                    :lazy="true" 
                    :paginator="true" 
                    :rows="stockQuery.pageSize || 10" 
                    :totalRecords="totalStocks" 
                    @page="onPage"
                    @sort="onSort"
                    @filter="onFilter"
                    dataKey="id"
                    rowHover
                    :first="((stockQuery.page || 1) - 1) * (stockQuery.pageSize || 10)"
                    :sortField="stockQuery.sort?.[0]?.replace(/^-/, '')"
                    :sortOrder="stockQuery.sort?.[0]?.startsWith('-') ? -1 : 1"
                    filterDisplay="menu"
                    removableSort
                    scrollable
                    stripedRows
                    showGridlines
                >
                    <template #header>
                        <div class="flex items-center justify-between gap-4">
                            <div class="flex items-center gap-2">
                                <Button
                                    type="button"
                                    :icon="stockQuery.lowStock ? 'pi pi-filter-fill' : 'pi pi-filter'"
                                    :label="stockQuery.lowStock ? 'Low Stock Only' : 'All Stock'"
                                    :severity="stockQuery.lowStock ? 'danger' : 'secondary'"
                                    outlined
                                    @click="toggleLowStock"
                                    class="rounded-xl"
                                />
                                <Button
                                    type="button"
                                    icon="pi pi-filter-slash"
                                    :label="t('inventory.table.clear_filter')"
                                    outlined
                                    @click="clearFilters"
                                    class="rounded-xl"
                                />
                            </div>
                        </div>
                    </template>

                    <template #empty>
                        <div class="flex flex-col items-center justify-center py-20 text-surface-400">
                            <i class="mb-4 text-6xl pi pi-box opacity-20"></i>
                            <p class="text-xl font-medium">{{ t('inventory.messages.empty_list') }}</p>
                        </div>
                    </template>

                    <Column field="sku" :header="t('inventory.table.sku')" sortable>
                        <template #body="{ data }">
                            <span class="font-mono text-xs uppercase tracking-widest font-bold">{{ data.sku }}</span>
                        </template>
                    </Column>

                    <Column field="variant_name" :header="t('inventory.table.product')">
                        <template #body="{ data }">
                            <span class="font-bold text-surface-900 dark:text-surface-0">{{ data.variant_name }}</span>
                        </template>
                    </Column>

                    <Column field="stock_location_name" :header="t('inventory.table.location')">
                        <template #body="{ data }">
                            <div class="flex items-center gap-2">
                                <i class="pi pi-building text-surface-400"></i>
                                <span>{{ data.stock_location_name }}</span>
                            </div>
                        </template>
                    </Column>

                    <Column field="countOnHand" :header="t('inventory.table.on_hand')" sortable class="text-center">
                        <template #body="{ data }">
                            <span class="font-black text-lg">{{ data.countOnHand }}</span>
                        </template>
                    </Column>

                    <Column field="quantityReserved" :header="t('inventory.table.reserved')" class="text-center">
                        <template #body="{ data }">
                            <span class="text-surface-500">{{ data.quantityReserved }}</span>
                        </template>
                    </Column>

                    <Column field="countAvailable" :header="t('inventory.table.available')" class="text-center">
                        <template #body="{ data }">
                            <Tag :value="data.countAvailable" 
                                 :severity="data.countAvailable > 10 ? 'success' : (data.countAvailable > 0 ? 'warning' : 'danger')" 
                                 class="px-3 font-bold" />
                        </template>
                    </Column>

                    <Column :header="t('inventory.table.actions')" class="w-32 text-right" frozen alignFrozen="right">
                        <template #body="{ data }">
                            <div class="flex justify-end gap-1">
                                <Button icon="pi pi-cog" severity="secondary" text rounded v-tooltip.top="'Adjust Stock'" @click="showAdjust(data)" />
                                <Button icon="pi pi-history" severity="secondary" text rounded v-tooltip.top="'History'" @click="showHistory(data)" />
                            </div>
                        </template>
                    </Column>
                </DataTable>

        <!-- Adjust Dialog -->
        <StockAdjustmentDialog 
            v-if="adjustDialog" 
            :stockItemId="selectedStockItem.id" 
            :sku="selectedStockItem.sku" 
            :variantName="selectedStockItem.variant_name" 
            @updated="store.fetchStocks()" 
            @close="adjustDialog = false" 
        />

        <!-- History Side Panel -->
        <Drawer v-model:visible="historyDrawer" position="right" :header="t('inventory.titles.stock_movement_history')" class="w-full md:w-[500px]">
            <template #header>
                <div class="flex flex-col gap-1">
                    <h3 class="text-xl font-black m-0">Movement History</h3>
                    <span class="font-mono text-xs text-surface-400 uppercase tracking-widest">{{ selectedSku }}</span>
                </div>
            </template>
            <div class="p-2">
                <StockMovementTimeline :key="selectedStockId" :stockItemId="selectedStockId" />
            </div>
        </Drawer>
    </PageShell>
</template>
