<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { usePropertyTypeStore } from '@/features/catalog/property-types/stores/property-type.store';
import { useApiErrorHandler } from '@/shared/composables/api-error-handler.use';
import { useToast } from '@/shared/composables/toast.use';
import { productService } from '../services/product.service';
import type { ProductProperty } from '../types/Product.Response.Type';

const props = defineProps<{
    productId: string;
}>();

const propertyTypeStore = usePropertyTypeStore();
const { handleApiResult } = useApiErrorHandler();
const { showToast } = useToast();
const { t } = useI18n();

const productProperties = ref<ProductProperty[]>([]);
const availablePropertyTypes = ref<any[]>([]);
const loading = ref(false);
const saving = ref(false);

// Form State
const selectedPropertyTypeId = ref<string | null>(null);
const propertyValue = ref('');

const loadData = async () => {
    loading.value = true;
    try {
        // 1. Get current properties for this product
        const result = await productService.getProperties(props.productId);
        if (result.isSuccess && result.items) {
            productProperties.value = result.items as unknown as ProductProperty[];
        }

        // 2. Get all available property types
        await propertyTypeStore.fetchList({ pageSize: 100 });
        availablePropertyTypes.value = propertyTypeStore.items.map(pt => ({
            label: `${pt.presentation} (${pt.name})`,
            value: pt.id
        }));
    } finally {
        loading.value = false;
    }
};

const onAddProperty = async () => {
    if (!selectedPropertyTypeId.value || !propertyValue.value) return;

    saving.value = true;
    try {
        const currentProps = productProperties.value.map(p => ({
            propertyTypeId: p.propertyTypeId,
            value: p.value
        }));

        currentProps.push({
            propertyTypeId: selectedPropertyTypeId.value,
            value: propertyValue.value
        });

        const result = await productService.updateProperties(props.productId, currentProps);

        if (handleApiResult(result)) {
            showToast('success', t('common.saved'), t('catalog.products.messages.property_assigned'));
            propertyValue.value = '';
            selectedPropertyTypeId.value = null;
            await loadData();
        }
    } finally {
        saving.value = false;
    }
};

const onRemoveProperty = async (propertyTypeId: string) => {
    const newProps = productProperties.value
        .filter(p => p.propertyTypeId !== propertyTypeId)
        .map(p => ({
            propertyTypeId: p.propertyTypeId,
            value: p.value
        }));

    const result = await productService.updateProperties(props.productId, newProps);
    
    if (handleApiResult(result)) {
        showToast('success', t('common.removed'), t('catalog.products.messages.property_removed'));
        await loadData();
    }
};

onMounted(() => {
    loadData();
});
</script>

<template>
    <div class="flex flex-col gap-8">
        <div>
            <h3 class="text-lg font-bold m-0">Technical Specifications</h3>
            <p class="text-sm text-surface-500 m-0">Manage detailed product properties and attributes.</p>
        </div>

        <!-- Add New Property Form -->
        <div class="p-6 border border-surface-200 dark:border-surface-700 rounded-3xl bg-surface-50 dark:bg-surface-800/50">
            <h4 class="text-sm font-bold uppercase tracking-wider text-surface-500 mt-0 mb-4">Add New Specification</h4>
            <div class="flex flex-col md:flex-row gap-4 items-end">
                <div class="flex flex-col gap-2 flex-1">
                    <label class="text-xs font-bold ml-1">Property Type</label>
                    <Select 
                        v-model="selectedPropertyTypeId" 
                        :options="availablePropertyTypes" 
                        optionLabel="label" 
                        optionValue="value" 
                        placeholder="Select type..." 
                        class="w-full rounded-xl"
                        filter
                    />
                </div>
                <div class="flex flex-col gap-2 flex-1">
                    <label class="text-xs font-bold ml-1">{{ t('catalog.products.labels.value') }}</label>
                    <InputText v-model="propertyValue" placeholder="e.g. 100% Cotton, 5000mAh" class="w-full rounded-xl" @keyup.enter="onAddProperty" />
                </div>
                <Button label="Add Property" icon="pi pi-plus" @click="onAddProperty" :loading="saving" :disabled="!selectedPropertyTypeId || !propertyValue" class="rounded-xl px-6" />
            </div>
        </div>

        <!-- List Section -->
        <div v-if="loading" class="flex justify-center py-12">
            <ProgressSpinner style="width: 40px; height: 40px" />
        </div>

        <div v-else-if="productProperties.length === 0" class="py-20 text-center border-2 border-dashed border-surface-200 dark:border-surface-800 rounded-3xl">
            <i class="pi pi-list text-4xl text-surface-200 mb-4"></i>
            <p class="text-surface-400 italic">No technical properties defined for this product yet.</p>
        </div>

        <div v-else class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div v-for="prop in productProperties" :key="prop.id" class="flex items-center justify-between p-4 bg-surface-0 dark:bg-surface-900 border border-surface-100 dark:border-surface-800 rounded-2xl shadow-sm group">
                <div class="flex flex-col overflow-hidden">
                    <span class="text-[10px] font-black uppercase tracking-widest text-surface-400 leading-none mb-1">{{ prop.propertyTypePresentation }}</span>
                    <span class="font-bold text-surface-900 dark:text-surface-0 truncate">{{ prop.value }}</span>
                </div>
                <Button icon="pi pi-trash" severity="danger" text rounded size="small" @click="onRemoveProperty(prop.propertyTypeId)" class="opacity-0 group-hover:opacity-100 transition-opacity" />
            </div>
        </div>
    </div>
</template>
