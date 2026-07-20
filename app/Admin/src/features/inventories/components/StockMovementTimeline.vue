<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { inventoryService } from '../api/inventory.api';
import type { StockMovement } from '../stock-movements/types/stock-movement.response';
import type { ServerQueryingParameters } from '@/common/api/types/query.types';
import { useFormatter } from '@/common/composables/formatter.use';

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
        const res = await inventoryService.listMovements({ pageSize: 50 } as ServerQueryingParameters);
        if (res.isSuccess && res.items) {
            movements.value = res.items;
        }
    } finally {
        loading.value = false;
    }
};

onMounted(fetchMovements);

const movementLabel = (type: number) => {
    switch (type) {
        case 0: return 'Purchase';
        case 1: return 'Sale';
        case 2: return 'Adjustment';
        case 3: return 'Audit';
        case 4: return 'Transfer';
        case 5: return 'Loss';
        default: return 'Movement';
    }
};

const getIcon = (type: number) => {
    switch (type) {
        case 0: return 'pi pi-shopping-cart';
        case 1: return 'pi pi-shopping-bag';
        case 2: return 'pi pi-cog';
        case 3: return 'pi pi-verified';
        case 4: return 'pi pi-arrow-right-arrow-left';
        default: return 'pi pi-box';
    }
};

const getColor = (type: number) => {
    switch (type) {
        case 0: return 'text-green-500';
        case 1: return 'text-blue-500';
        case 5: return 'text-red-500';
        case 3: return 'text-purple-500';
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
                    <i :class="[getIcon(slotProps.item.type), getColor(slotProps.item.type)]" class="text-xs"></i>
                </span>
            </template>
            <template #content="slotProps">
                <div class="flex flex-col mb-6">
                    <div class="flex items-center gap-2">
                        <span class="font-bold text-sm">{{ movementLabel(slotProps.item.type) }}</span>
                        <Tag :value="slotProps.item.quantity > 0 ? `+${slotProps.item.quantity}` : slotProps.item.quantity"
                             :severity="slotProps.item.quantity > 0 ? 'success' : 'danger'" class="text-[10px]" />
                    </div>
                    <p class="text-xs text-surface-500 mt-1" v-if="slotProps.item.reason">
                        {{ slotProps.item.reason }}
                    </p>
                    <div class="flex items-center gap-4 mt-2 text-[10px] text-surface-400 font-mono uppercase tracking-tighter">
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
