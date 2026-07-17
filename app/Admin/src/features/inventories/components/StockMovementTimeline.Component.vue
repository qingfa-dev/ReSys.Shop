<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { inventoryService } from '../services/inventory.service';
import type { StockMovement } from '../types/StockMovement.Response.Type';
import { useFormatter } from '@/shared/composables/formatter.use';

const props = defineProps<{
    stockItemId?: string;
}>();

const { formatDate } = useFormatter();
const movements = ref<StockMovement[]>([]);
const loading = ref(false);

const fetchMovements = async () => {
    if (!props.stockItemId) {
        movements.value = [];
        return;
    }
    loading.value = true;
    try {
        const res = await inventoryService.listMovements({ pageSize: 50 } as any);
        if (res.isSuccess && res.items) {
            movements.value = res.items;
        }
    } finally {
        loading.value = false;
    }
};

onMounted(fetchMovements);

const getIcon = (type: string) => {
    switch (type) {
        case 'Purchase': return 'pi pi-shopping-cart';
        case 'Sale': return 'pi pi-shopping-bag';
        case 'Adjustment': return 'pi pi-cog';
        case 'Audit': return 'pi pi-verified';
        case 'Transfer': return 'pi pi-arrow-right-arrow-left';
        default: return 'pi pi-box';
    }
};

const getColor = (type: string) => {
    switch (type) {
        case 'Purchase': return 'text-green-500';
        case 'Sale': return 'text-blue-500';
        case 'Loss': return 'text-red-500';
        case 'Audit': return 'text-purple-500';
        default: return 'text-surface-500';
    }
};
</script>

<template>
    <div class="stock-movement-timeline flex flex-col gap-4">
        <div v-if="loading && movements.length === 0" class="flex justify-center p-8">
            <ProgressSpinner style="width: 40px; height: 40px" />
        </div>

        <Timeline v-else :value="movements" class="customized-timeline">
            <template #opposite="slotProps">
                <small class="text-surface-500 font-mono">{{ formatDate(slotProps.item.createdAtUtc) }}</small>
            </template>
            <template #marker="slotProps">
                <span class="flex w-8 h-8 items-center justify-center bg-surface-100 dark:bg-surface-800 rounded-full shadow-sm">
                    <i :class="[getIcon(slotProps.item.action), getColor(slotProps.item.action)]" class="text-xs"></i>
                </span>
            </template>
            <template #content="slotProps">
                <div class="flex flex-col mb-6">
                    <div class="flex items-center gap-2">
                        <span class="font-bold text-sm">{{ slotProps.item.action }}</span>
                        <Tag :value="slotProps.item.quantity > 0 ? `+${slotProps.item.quantity}` : slotProps.item.quantity"
                             :severity="slotProps.item.quantity > 0 ? 'success' : 'danger'" class="text-[10px]" />
                    </div>
                    <p class="text-xs text-surface-500 mt-1" v-if="slotProps.item.reason">
                        {{ slotProps.item.reason }}
                    </p>
                    <div class="flex items-center gap-4 mt-2 text-[10px] text-surface-400 font-mono uppercase tracking-tighter">
                        <span>Previous Count: {{ slotProps.item.previousCountOnHand }}</span>
                        <span v-if="slotProps.item.reference">REF: {{ slotProps.item.reference }}</span>
                    </div>
                </div>
            </template>
        </Timeline>

        <div v-if="movements.length === 0 && !loading" class="p-8 text-center text-surface-400 italic">
            No movements recorded for this item.
        </div>
    </div>
</template>

<style scoped>
:deep(.p-timeline-event-opposite) {
    flex: 0;
    padding: 0 1rem 0 0;
    min-width: 120px;
    text-align: right;
}
</style>
