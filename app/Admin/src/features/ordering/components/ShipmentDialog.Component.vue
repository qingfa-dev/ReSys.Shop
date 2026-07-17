<script setup lang="ts">
import { ref, computed } from 'vue';
import { useToast } from '@/shared/composables/toast.use';
import { orderService } from '../services/order.service';
import LocationSelector from '@/features/inventories/components/LocationSelector.Component.vue';
import type { OrderDetail } from '../types/Order.Response.Type';
import type { CreateShipmentRequest } from '../types/Order.Request.Type';
import { useI18n } from 'vue-i18n';

const props = defineProps<{
    order: OrderDetail;
}>();

const emit = defineEmits(['updated', 'close']);

const { t } = useI18n();
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
        showToast('error', t('common.error'), t('ordering.messages.warehouse_required'));
        return;
    }
    if (selectedUnitIds.value.length === 0) {
        showToast('error', t('common.error'), t('ordering.messages.items_to_ship_required'));
        return;
    }

    loading.value = true;
    try {
        const payload: CreateShipmentRequest = {
            stockLocationId: stockLocationId.value,
            inventoryUnitIds: selectedUnitIds.value
        };
        const res = await orderService.createShipment(props.order.id, payload);
        if (res.isSuccess) {
            showToast('success', t('common.success'), t('ordering.messages.shipment_created'));
            emit('updated');
            emit('close');
        }
    } finally {
        loading.value = false;
    }
};
</script>

<template>
    <Dialog v-model:visible="visible" :header="t('ordering.actions.create_shipment')" modal class="w-full max-w-3xl" @hide="emit('close')">
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
                        <Column field="sku" :header="t('ordering.table.sku')" />
                        <Column field="state" :header="t('ordering.table.status')" />
                    </DataTable>
                </div>
            </div>
        </div>

        <template #footer>
            <Button :label="t('common.cancel')" severity="secondary" text @click="emit('close')" />
            <Button :label="t('ordering.actions.create_shipment')" icon="pi pi-check" :loading="loading" @click="onSubmit" />
        </template>
    </Dialog>
</template>
