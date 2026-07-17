<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { useApiErrorHandler } from '@/shared/composables/api-error-handler.use';
import { useToast } from '@/shared/composables/toast.use';
import { useFormatter } from '@/shared/composables/formatter.use';
import { inventoryService } from '@/features/inventories/services/inventory.service';
import { variantService } from '../services/variant.service';
import StockMovementTimeline from '@/features/inventories/components/StockMovementTimeline.Component.vue';
import StockAdjustmentDialog from '@/features/inventories/components/StockAdjustmentDialog.Component.vue';

const { t } = useI18n();

const props = defineProps<{
    productId: string;
}>();

const { handleApiResult } = useApiErrorHandler();
const { showToast } = useToast();
const { formatCurrency } = useFormatter();

const variants = ref<any[]>([]);
const stockItems = ref<any[]>([]);
const loading = ref(false);

const historyDrawer = ref(false);
const adjustDialog = ref(false);
const selectedStockItem = ref<any>(null);

const loadData = async () => {
    loading.value = true;
    try {
        const varResult = await variantService.listByProductId(props.productId);
        if (varResult.success && varResult.data) {
            variants.value = varResult.data || [];
        }

        const stockResult = await inventoryService.listStocks({ filter: `Variant.ProductId=${props.productId}` });
        if (stockResult.success && stockResult.data) {
            stockItems.value = stockResult.data || [];
        }
    } finally {
        loading.value = false;
    }
};

const getStockForVariant = (variantId: string) => {
    return stockItems.value.filter(si => si.variant_id === variantId);
};

const showHistory = (data: any) => {
    selectedStockItem.value = data;
    historyDrawer.value = true;
};

const showAdjust = (data: any) => {
    selectedStockItem.value = data;
    adjustDialog.value = true;
};

onMounted(() => {
    loadData();
});
</script>

<template>
    <div class="flex flex-col gap-6">
        <div>
            <h3 class="text-lg font-bold m-0">{{ t('catalog.products.inventory_table.title') }}</h3>
            <p class="text-sm text-surface-500 m-0">{{ t('catalog.products.descriptions.inventory_management') }}</p>
        </div>

        <div v-if="loading" class="flex justify-center py-12">
            <ProgressSpinner />
        </div>

        <div v-else-if="variants.length === 0" class="text-center py-8 text-surface-500 italic">
            {{ t('catalog.products.variants.empty') }}
        </div>

        <div v-else class="flex flex-col gap-6">
            <div v-for="variant in variants" :key="variant.id" class="border border-surface-200 dark:border-surface-700 rounded-2xl overflow-hidden bg-surface-0 dark:bg-surface-900">
                <div class="p-4 bg-surface-50 dark:bg-surface-800/50 border-b border-surface-200 dark:border-surface-700 flex justify-between items-center">
                    <div class="flex flex-col">
                        <span class="font-bold text-lg">{{ variant.sku }}</span>
                        <div class="flex gap-2 mt-1">
                            <Tag v-for="(opt, idx) in variant.options || []" :key="idx" :value="`${opt.name}: ${opt.value}`" severity="secondary" class="text-xs" />
                        </div>
                    </div>
                </div>

                <div class="p-0">
                    <DataTable :value="getStockForVariant(variant.id)" size="small" class="border-none">
                        <template #empty>
                            <div class="p-4 text-center text-sm text-surface-400">{{ t('catalog.products.inventory_table.no_records') }}</div>
                        </template>
                        <Column field="stock_location_name" :header="t('catalog.products.inventory_table.location')"></Column>
                        <Column field="quantity_on_hand" :header="t('catalog.products.inventory_table.on_hand')" class="text-right font-mono font-bold"></Column>
                        <Column field="quantity_reserved" :header="t('catalog.products.inventory_table.reserved')" class="text-right font-mono text-orange-500"></Column>
                        <Column :header="t('catalog.products.inventory_table.available')" class="text-right font-mono text-green-600">
                            <template #body="{ data }">
                                {{ data.quantity_on_hand - data.quantity_reserved }}
                            </template>
                        </Column>
                        <Column field="backorderable" :header="t('catalog.products.inventory_table.backorder')" class="text-center">
                            <template #body="{ data }">
                                <i v-if="data.backorderable" class="pi pi-check text-green-500"></i>
                                <i v-else class="pi pi-times text-surface-400"></i>
                            </template>
                        </Column>
                        <Column class="w-24 text-right">
                            <template #body="{ data }">
                                <div class="flex justify-end gap-1">
                                    <Button icon="pi pi-cog" severity="secondary" text rounded @click="showAdjust(data)" v-tooltip.top="'Adjust'" />
                                    <Button icon="pi pi-history" severity="secondary" text rounded @click="showHistory(data)" v-tooltip.top="'Timeline'" />
                                </div>
                            </template>
                        </Column>
                    </DataTable>
                </div>
            </div>
        </div>

        <StockAdjustmentDialog 
            v-if="adjustDialog" 
            :stockItemId="selectedStockItem.id" 
            :sku="selectedStockItem.sku" 
            :variantName="selectedStockItem.variant_name" 
            @updated="loadData" 
            @close="adjustDialog = false" 
        />

        <Drawer v-model:visible="historyDrawer" position="right" class="w-full md:w-[500px]">
            <template #header>
                <div class="flex flex-col gap-1">
                    <h3 class="text-xl font-black m-0">Movement History</h3>
                    <span class="font-mono text-xs text-surface-400 uppercase tracking-widest">{{ selectedStockItem?.sku }}</span>
                </div>
            </template>
            <div class="p-2">
                <StockMovementTimeline :key="selectedStockItem?.id" :stockItemId="selectedStockItem?.id" />
            </div>
        </Drawer>
    </div>
</template>
