<script setup lang="ts">
import { ref } from 'vue';
import { inventoryService } from '../services/inventory.service';
import { useToast } from '@/common/composables/toast.use';
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

const props = defineProps<{
    stockItemId: string;
    sku: string;
    variantName: string;
}>();

const emit = defineEmits(['updated', 'close']);

const { showToast } = useToast();
const visible = ref(true);
const loading = ref(false);

const form = ref({
    quantity: 0,
    type: 0, // Adjustment
    reason: '',
    reference: ''
});

const typeOptions = [
    { label: 'Manual Adjustment', value: 0 },
    { label: 'Purchase Receipt', value: 1 },
    { label: 'Sales Deduction', value: 2 },
    { label: 'Return to Stock', value: 3 },
    { label: 'Inter-Warehouse', value: 4 },
    { label: 'Inventory Loss', value: 5 }
];

async function onSubmit() {
    loading.value = true;
    try {
        const res = await inventoryService.adjustStock(props.stockItemId, form.value);
        if (res.isSuccess) {
            showToast('success', t('common.success'), t('inventory.messages.adjust_success'));
            emit('updated');
            emit('close');
        }
    } finally {
        loading.value = false;
    }
}
</script>

<template>
    <Dialog v-model:visible="visible" :header="t('inventory.titles.adjust')" modal class="w-full max-w-lg" @hide="emit('close')">
        <div class="flex flex-col gap-6 py-4">
            <div class="bg-surface-50 dark:bg-surface-900 p-4 rounded-2xl border border-surface-100 dark:border-surface-800">
                <span class="text-xs font-mono uppercase text-surface-400">{{ sku }}</span>
                <h4 class="text-lg font-bold m-0">{{ variantName }}</h4>
            </div>

            <div class="grid grid-cols-2 gap-4">
                <div class="flex flex-col gap-2">
                    <label class="font-bold text-sm">{{ t('inventory.labels.quantity') }}</label>
                    <InputNumber v-model="form.quantity" showButtons :min="-10000" :max="10000" class="w-full" />
                </div>

            </div>

            <div class="flex flex-col gap-2">
                <label class="font-bold text-sm">{{ t('inventory.table.type') }}</label>
                <Select v-model="form.type" :options="typeOptions" optionLabel="label" optionValue="value" class="w-full" />
            </div>

            <div class="flex flex-col gap-2">
                <label class="font-bold text-sm">{{ t('inventory.labels.reason') }}</label>
                <Textarea v-model="form.reason" rows="2" class="w-full" />
            </div>

            <div class="flex flex-col gap-2">
                <label class="font-bold text-sm">{{ t('inventory.table.reference') }}</label>
                <InputText v-model="form.reference" placeholder="PO #, Order ID, etc." class="w-full" />
            </div>
        </div>

        <template #footer>
            <Button :label="t('inventory.actions.cancel')" severity="secondary" text @click="emit('close')" />
            <Button :label="t('inventory.actions.save')" icon="pi pi-check" :loading="loading" @click="onSubmit" />
        </template>
    </Dialog>
</template>
