<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { useInventoryStore } from '../stores/inventory.store';
import { useToast } from '@/shared/composables/toast.use';
import { useI18n } from 'vue-i18n';
import AppBreadcrumb from '@/shared/components/Breadcrumb.Component.vue';
import LocationSelector from '../components/LocationSelector.Component.vue';
import type { CreateStockTransferRequest } from '../types/inventory.types';

const { t } = useI18n();

const router = useRouter();
const store = useInventoryStore();
const { showToast } = useToast();

const loading = ref(false);
const form = ref<CreateStockTransferRequest>({
    sourceLocationId: '',
    destinationLocationId: '',
    reason: ''
});

async function onSubmit() {
    if (form.value.sourceLocationId === form.value.destinationLocationId) {
        showToast('error', 'Error', 'Source and destination cannot be the same.');
        return;
    }

    loading.value = true;
    try {
        const res = await store.inventoryService.createTransfer(form.value);
        if (res.success && res.data) {
            showToast('success', 'Success', t('inventory.messages.create_transfer_success') || 'Transfer created');
            router.push({ name: 'inventory.transfers.detail', params: { id: res.data.id } });
        }
    } finally {
        loading.value = false;
    }
}
</script>

<template>
    <div class="p-6 max-w-2xl mx-auto">
        <AppBreadcrumb :locales="t" />
        
        <div class="flex items-center gap-4 mt-4 mb-8">
            <Button icon="pi pi-arrow-left" text rounded severity="secondary" @click="router.back()" class="bg-surface-100 dark:bg-surface-800" />
            <div>
                <h2 class="text-4xl font-black tracking-tighter text-surface-900 dark:text-surface-50 m-0">
                    {{ t('inventory.titles.create_transfer') }}
                </h2>
                <p class="text-sm text-surface-500 m-0">{{ t('inventory.descriptions.transfers') }}</p>
            </div>
        </div>

        <Card class="rounded-3xl shadow-sm border-none bg-surface-0 dark:bg-surface-900">
            <template #content>
                <form @submit.prevent="onSubmit" class="flex flex-col gap-6">
                    <div class="flex flex-col gap-2">
                        <label class="font-bold text-sm">{{ t('inventory.labels.source') }}</label>
                        <LocationSelector v-model="form.sourceLocationId" placeholder="Select Source Warehouse" />
                    </div>

                    <div class="flex flex-col gap-2">
                        <label class="font-bold text-sm">{{ t('inventory.labels.destination') }}</label>
                        <LocationSelector v-model="form.destinationLocationId" placeholder="Select Destination Warehouse" />
                    </div>

                    <div class="flex flex-col gap-2">
                        <label class="font-bold text-sm">{{ t('inventory.labels.reason') }}</label>
                        <Textarea v-model="form.reason" rows="3" class="w-full rounded-2xl p-4" placeholder="Optional notes for this movement..." />
                    </div>

                    <div class="flex justify-end gap-3 mt-4">
                        <Button :label="t('inventory.actions.cancel')" severity="secondary" text @click="router.back()" />
                        <Button type="submit" :label="t('inventory.actions.new_transfer')" icon="pi pi-check" :loading="loading" class="rounded-xl px-8" />
                    </div>
                </form>
            </template>
        </Card>
    </div>
</template>
