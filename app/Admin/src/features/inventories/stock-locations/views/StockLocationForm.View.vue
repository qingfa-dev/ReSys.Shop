<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useInventoryStore } from '../../stores/inventory.store';
import { useToast } from '@/common/composables/toast.use';
import { useI18n } from 'vue-i18n';
import PageShell from '@/shared/components/PageShell.Component.vue';
import PageHeader from '@/shared/components/PageHeader.Component.vue';
import LocationSelector from '../../components/LocationSelector.Component.vue';
import type { CreateStockLocationRequest } from '../types/stock-location.request.type';

const { t } = useI18n();

const props = defineProps({
    hideHeader: {
        type: Boolean,
        default: false
    }
});

const route = useRoute();
const router = useRouter();
const store = useInventoryStore();
const { showToast } = useToast();

const isEdit = computed(() => !!route.params.id);
const locationId = computed(() => route.params.id as string);
const loading = ref(false);
const submitting = ref(false);

const form = ref<CreateStockLocationRequest>({
    name: '',
    code: '',
    type: 0,
    isDefault: false,
    active: true,
    address1: '',
    address2: '',
    city: '',
    zipCode: '',
    countryCode: 'US',
    stateCode: '',
    phone: '',
    backorderableDefault: false,
    propagateAllVariants: false,
    notifyOnLowStock: false,
    position: 0,
});

// Note: Ensure parentId is handled
const parentId = ref<string | null>(route.query.parentId as string || null);

const typeOptions = [
    { label: 'Warehouse', value: 0 },
    { label: 'Retail Store', value: 1 },
    { label: 'Transit', value: 2 },
    { label: 'Returns Center', value: 3 }
];

async function loadLocation() {
    loading.value = true;
    try {
        const res = await store.inventoryService.getLocationDetail(locationId.value);
        if (res.isSuccess && res.value) {
            form.value = {
                ...form.value,
                name: res.value.name,
                code: res.value.code,
                type: 0,
                isDefault: res.value.isDefault,
                active: res.value.isActive,
                address1: res.value.address,
                city: res.value.city,
                zipCode: res.value.postalCode,
                countryCode: res.value.country,
                stateCode: res.value.stateProvince || '',
            };
        }
    } finally {
        loading.value = false;
    }
}

async function onSubmit() {
    submitting.value = true;
    try {
        const payload = {
            ...form.value,
            parentId: parentId.value
        };

        const res = isEdit.value 
            ? await store.inventoryService.updateLocation(locationId.value, payload)
            : await store.inventoryService.createLocation(payload);
            
        if (res.isSuccess) {
            showToast('success', t('common.success'), isEdit.value ? t('inventory.messages.location_updated') : t('inventory.messages.location_created'));
            await store.fetchLocationTree();
            await store.fetchLocations();
            
            if (props.hideHeader) {
                // If in manager, we might want to just stay here or redirect to edit of new item
                if (!isEdit.value && res.value) {
                    router.push({ name: 'inventory.locations.edit', params: { id: res.value.id } });
                }
            } else {
                router.push({ name: 'inventory.locations.list' });
            }
        }
    } finally {
        submitting.value = false;
    }
}

onMounted(() => {
    if (isEdit.value) {
        loadLocation();
    }
});
</script>

<template>
    <PageShell maxWidth="7xl">
        <PageHeader back :title="isEdit ? 'Edit Location' : t('inventory.titles.create_location')" :description="t('inventory.descriptions.locations')" />

        <div v-if="loading" class="flex justify-center p-20">
            <ProgressSpinner />
        </div>

        <form v-else @submit.prevent="onSubmit" class="grid grid-cols-1 lg:grid-cols-3 gap-8">
            <div class="lg:col-span-2 flex flex-col gap-6">
                <!-- Basic Info -->
                <Card class="rounded-3xl shadow-sm border-none bg-surface-0 dark:bg-surface-900 overflow-hidden">
                    <template #title><span class="text-sm font-black uppercase tracking-widest text-surface-400">{{ t('inventory.titles.identification') }}</span></template>
                    <template #content>
                        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                            <div class="flex flex-col gap-2">
                                <label class="font-bold text-sm">{{ t('inventory.labels.name') }}</label>
                                <InputText v-model="form.name" required class="w-full rounded-xl" />
                            </div>
                            <div class="flex flex-col gap-2">
                                <label class="font-bold text-sm">{{ t('inventory.labels.code') }}</label>
                                <InputText v-model="form.code" required class="w-full font-mono rounded-xl" :disabled="isEdit" />
                            </div>
                            <div class="flex flex-col gap-2 md:col-span-2">
                                <label class="font-bold text-sm">Hierarchy (Parent Location)</label>
                                <LocationSelector v-model="parentId" placeholder="No Parent (Top Level)" />
                            </div>
                        </div>
                    </template>
                </Card>

                <!-- Address Info -->
                <Card class="rounded-3xl shadow-sm border-none bg-surface-0 dark:bg-surface-900 overflow-hidden">
                    <template #title><span class="text-sm font-black uppercase tracking-widest text-surface-400">Physical Address</span></template>
                    <template #content>
                        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                            <div class="flex flex-col gap-2 md:col-span-2">
                                <label class="font-bold text-sm">{{ t('inventory.labels.address') }}</label>
                                <InputText v-model="form.address1" required class="w-full rounded-xl" />
                            </div>
                            <div class="flex flex-col gap-2">
                                <label class="font-bold text-sm">{{ t('inventory.labels.city') }}</label>
                                <InputText v-model="form.city" required class="w-full rounded-xl" />
                            </div>
                            <div class="flex flex-col gap-2">
                                <label class="font-bold text-sm">{{ t('inventory.labels.zip') }}</label>
                                <InputText v-model="form.zipCode" required class="w-full rounded-xl" />
                            </div>
                            <div class="flex flex-col gap-2">
                                <label class="font-bold text-sm">{{ t('inventory.labels.country') }}</label>
                                <InputText v-model="form.countryCode" required class="w-full rounded-xl" maxlength="2" placeholder="e.g. US" />
                            </div>
                        </div>
                    </template>
                </Card>
            </div>

            <!-- Sidebar -->
            <div class="flex flex-col gap-6">
                <Card class="rounded-3xl shadow-sm border-none bg-surface-0 dark:bg-surface-900 overflow-hidden">
                    <template #content>
                        <div class="flex flex-col gap-6">
                            <div class="flex flex-col gap-2">
                                <label class="font-bold text-sm">Location Type</label>
                                <Select v-model="form.type" :options="typeOptions" optionLabel="label" optionValue="value" class="w-full rounded-xl" />
                            </div>

                            <div class="flex items-center justify-between p-4 bg-surface-50 dark:bg-surface-800/50 rounded-2xl border border-surface-100 dark:border-surface-800">
                                <div class="flex flex-col">
                                    <span class="font-bold text-sm">{{ t('inventory.labels.is_default') }}</span>
                                    <small class="text-surface-500">Fallback for inventory logic</small>
                                </div>
                                <ToggleSwitch v-model="form.isDefault" />
                            </div>

                            <Divider />

                            <Button type="submit" :label="isEdit ? t('inventory.actions.save') : t('inventory.actions.new_location')" icon="pi pi-check" class="w-full h-12 rounded-xl" :loading="submitting" />
                            <Button v-if="!hideHeader" :label="t('inventory.actions.cancel')" severity="secondary" text class="w-full" @click="router.back()" />
                        </div>
                    </template>
                </Card>
            </div>
        </form>
    </PageShell>
</template>
