<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useInventoryStore } from '../stores/inventory.store';
import { useToast } from '@/shared/composables/toast.use';
import { inventoryLocales as t } from '../locales/inventory.locales';
import AppBreadcrumb from '@/shared/components/breadcrumb.component.vue';
import LocationSelector from '../components/LocationSelector.vue';
import type { CreateStockLocationRequest } from '../types/inventory.types';

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
    presentation: '',
    type: 0, // Warehouse
    is_default: false,
    address: {
        address1: '',
        city: '',
        zip_code: '',
        country_code: 'US'
    }
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
        if (res.success && res.data) {
            form.value = {
                name: res.data.name,
                code: res.data.code,
                presentation: res.data.presentation || '',
                type: 0, // Need to map enum if necessary, backend might use string
                is_default: res.data.is_default,
                address: {
                    address1: res.data.address.address1,
                    address2: res.data.address.address2 || '',
                    city: res.data.address.city,
                    zip_code: res.data.address.zip_code,
                    country_code: res.data.address.country_code,
                    state_code: res.data.address.state_code || '',
                    phone: res.data.address.phone || '',
                    first_name: res.data.address.first_name || '',
                    last_name: res.data.address.last_name || '',
                    company: res.data.address.company || ''
                }
            };
            parentId.value = (res.data as any).parent_id;
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
            
        if (res.success) {
            showToast('success', 'Success', isEdit.value ? 'Location updated' : t.messages?.create_location_success || 'Location created');
            await store.fetchLocationTree();
            await store.fetchLocations();
            
            if (props.hideHeader) {
                // If in manager, we might want to just stay here or redirect to edit of new item
                if (!isEdit.value && res.data) {
                    router.push({ name: 'inventory.locations.edit', params: { id: res.data.id } });
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
    <div :class="[hideHeader ? 'p-0' : 'p-6 max-w-4xl mx-auto']">
        <template v-if="!hideHeader">
            <AppBreadcrumb :locales="t" />
            
            <div class="flex items-center gap-4 mt-4 mb-8">
                <Button icon="pi pi-arrow-left" text rounded severity="secondary" @click="router.back()" class="bg-surface-100 dark:bg-surface-800" />
                <div>
                    <h2 class="text-4xl font-black tracking-tighter text-surface-900 dark:text-surface-50 m-0">
                        {{ isEdit ? 'Edit Location' : t.titles.create_location }}
                    </h2>
                    <p class="text-sm text-surface-500 m-0">{{ t.descriptions?.locations }}</p>
                </div>
            </div>
        </template>

        <div v-if="loading" class="flex justify-center p-20">
            <ProgressSpinner />
        </div>

        <form v-else @submit.prevent="onSubmit" class="grid grid-cols-1 lg:grid-cols-3 gap-8">
            <div class="lg:col-span-2 flex flex-col gap-6">
                <!-- Basic Info -->
                <Card class="rounded-3xl shadow-sm border-none bg-surface-0 dark:bg-surface-900 overflow-hidden">
                    <template #title><span class="text-sm font-black uppercase tracking-widest text-surface-400">Identification</span></template>
                    <template #content>
                        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                            <div class="flex flex-col gap-2">
                                <label class="font-bold text-sm">{{ t.labels?.name }}</label>
                                <InputText v-model="form.name" required class="w-full rounded-xl" />
                            </div>
                            <div class="flex flex-col gap-2">
                                <label class="font-bold text-sm">{{ t.labels?.code }}</label>
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
                                <label class="font-bold text-sm">{{ t.labels?.address }}</label>
                                <InputText v-model="form.address.address1" required class="w-full rounded-xl" />
                            </div>
                            <div class="flex flex-col gap-2">
                                <label class="font-bold text-sm">{{ t.labels?.city }}</label>
                                <InputText v-model="form.address.city" required class="w-full rounded-xl" />
                            </div>
                            <div class="flex flex-col gap-2">
                                <label class="font-bold text-sm">{{ t.labels?.zip }}</label>
                                <InputText v-model="form.address.zip_code" required class="w-full rounded-xl" />
                            </div>
                            <div class="flex flex-col gap-2">
                                <label class="font-bold text-sm">{{ t.labels?.country }}</label>
                                <InputText v-model="form.address.country_code" required class="w-full rounded-xl" maxlength="2" placeholder="e.g. US" />
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
                                <Dropdown v-model="form.type" :options="typeOptions" optionLabel="label" optionValue="value" class="w-full rounded-xl" />
                            </div>

                            <div class="flex items-center justify-between p-4 bg-surface-50 dark:bg-surface-800/50 rounded-2xl border border-surface-100 dark:border-surface-800">
                                <div class="flex flex-col">
                                    <span class="font-bold text-sm">{{ t.labels?.is_default }}</span>
                                    <small class="text-surface-500">Fallback for inventory logic</small>
                                </div>
                                <ToggleSwitch v-model="form.is_default" />
                            </div>

                            <Divider />

                            <Button type="submit" :label="isEdit ? t.actions.save : t.actions.new_location" icon="pi pi-check" class="w-full h-12 rounded-xl" :loading="submitting" />
                            <Button v-if="!hideHeader" label="Cancel" severity="secondary" text class="w-full" @click="router.back()" />
                        </div>
                    </template>
                </Card>
            </div>
        </form>
    </div>
</template>
