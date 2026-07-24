<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { useInventoryStore } from '../../store/inventory.store';
import { useToast } from '@/common/composables/toast.use';
import { useI18n } from 'vue-i18n';
import PageShell from '@/shared/components/navigation/PageShell.vue'
import PageHeader from '@/shared/components/navigation/PageHeader.vue'
import LocationSelector from '../../components/LocationSelector.vue';
import FormField from '@/shared/components/form/FormField.vue';
import type { CreateStockTransferRequest } from '../types/stock-transfer.request';

const { t } = useI18n();

const router = useRouter();
const store = useInventoryStore();
const { showToast } = useToast();

const loading = ref(false);
const form = ref<CreateStockTransferRequest>({
    sourceLocationId: '',
    destinationLocationId: '',
    reason: '',
    items: []
});

async function onSubmit() {
    if (form.value.sourceLocationId === form.value.destinationLocationId) {
        showToast('error', t('common.error'), t('inventory.messages.source_destination_same'));
        return;
    }

    loading.value = true;
    try {
        const res = await store.inventoryService.createTransfer(form.value);
        if (res.isSuccess && res.value) {
            showToast('success', t('common.success'), t('inventory.messages.create_transfer_success'));
            router.push({ name: 'inventory.transfers.detail', params: { id: res.value.id } });
        }
    } finally {
        loading.value = false;
    }
}
</script>

<template>
    <PageShell maxWidth="7xl">
        <PageHeader back :title="t('inventory.titles.create_transfer')" :description="t('inventory.descriptions.transfers')" />

        <form @submit.prevent="onSubmit" class="flex flex-col gap-6">
            <FormField :label="t('inventory.labels.source')" name="sourceLocationId">
                <LocationSelector v-model="form.sourceLocationId" placeholder="Select Source Warehouse" />
            </FormField>

            <FormField :label="t('inventory.labels.destination')" name="destinationLocationId">
                <LocationSelector v-model="form.destinationLocationId" placeholder="Select Destination Warehouse" />
            </FormField>

            <FormField :label="t('inventory.labels.reason')" name="reason">
                <Textarea v-model="form.reason" rows="3" class="w-full rounded-2xl p-4" placeholder="Optional notes for this movement..." />
            </FormField>

            <div class="flex justify-end gap-3 mt-4">
                <Button :label="t('inventory.actions.cancel')" severity="secondary" text @click="router.back()" />
                <Button type="submit" :label="t('inventory.actions.new_transfer')" icon="pi pi-check" :loading="loading" class="rounded-xl px-8" />
            </div>
        </form>
    </PageShell>
</template>
