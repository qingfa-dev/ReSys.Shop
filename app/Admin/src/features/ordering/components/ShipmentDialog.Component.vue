<script setup lang="ts">
import { ref, computed } from 'vue';
import { useToast } from '@/shared/composables/toast.use';
import { orderService } from '../services/order.service';
import LocationSelector from '@/features/inventories/components/LocationSelector.Component.vue';
import type { OrderDetail, CreateShipmentRequest } from '../types/order.types';

const props = defineProps<{
    order: OrderDetail;
}>();

const emit = defineEmits(['updated', 'close']);

const { showToast } = useToast();
const visible = ref(true);
const loading = ref(false);

const stockLocationId = ref('');
const selectedUnitIds = ref<string[]>([]);

// Filter only units that are NOT shipped/canceled
// InventoryUnitDetail: { id, sku, state, pending }
// We assume pending means not shipped yet.
const availableUnits = computed(() => {
    const units: any[] = [];
    if (!props.order.lineItems) return [];
    
    props.order.lineItems.forEach((item: any) => {
        if (item.inventory_units) {
            item.inventory_units.forEach((unit: any) => {
                if (unit.state !== 'Shipped' && unit.state !== 'Canceled') {
                    units.push(unit);
                }
            });
        }
    });
    return units; 
});

const onSubmit = async () => {
    if (!stockLocationId.value) {
        showToast('error', 'Error', 'Please select a source warehouse.');
        return;
    }
    if (selectedUnitIds.value.length === 0) {
        showToast('error', 'Error', 'Please select at least one item to ship.');
        return;
    }

    loading.value = true;
    try {
        const payload: CreateShipmentRequest = {
            stockLocationId: stockLocationId.value,
            inventoryUnitIds: selectedUnitIds.value
        };
        const res = await orderService.createShipment(props.order.id, payload);
        if (res.success) {
            showToast('success', 'Success', 'Shipment created successfully');
            emit('updated');
            emit('close');
        }
    } finally {
        loading.value = false;
    }
};
</script>

<template>
    <Dialog v-model:visible="visible" header="Create Shipment" modal class="w-full max-w-3xl" @hide="emit('close')">
        <div class="flex flex-col gap-6 py-4">
            <div class="flex flex-col gap-2">
                <label class="font-bold text-sm">Ship From</label>
                <LocationSelector v-model="stockLocationId" placeholder="Select Warehouse" />
            </div>

            <div class="flex flex-col gap-2">
                <label class="font-bold text-sm">Items to Ship</label>
                <div class="border rounded-xl overflow-hidden">
                    <DataTable :value="availableUnits" v-model:selection="selectedUnitIds" dataKey="id">
                        <Column selectionMode="multiple" headerStyle="width: 3rem"></Column>
                        <Column field="sku" header="SKU" />
                        <Column field="state" header="Status" />
                    </DataTable>
                </div>
            </div>
        </div>

        <template #footer>
            <Button label="Cancel" severity="secondary" text @click="emit('close')" />
            <Button label="Create Shipment" icon="pi pi-check" :loading="loading" @click="onSubmit" />
        </template>
    </Dialog>
</template>
