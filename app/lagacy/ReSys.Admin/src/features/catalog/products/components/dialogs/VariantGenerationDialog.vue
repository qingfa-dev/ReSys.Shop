<script setup lang="ts">
import { ref, computed, watch } from 'vue';
import { useOptionTypeStore } from '@/features/catalog/option-types/stores/option-type.store';
import { useOptionValueStore } from '@/features/catalog/option-types/option-values/stores/option-value.store';
import { useProductStore } from '../../stores/product.store';
import { productService } from '../../services/product.service';
import { variantService } from '../../services/variant.service';
import { productLocales as t } from '../../locales/product.locales';
import { useToast } from '@/shared/composables/toast.use';
import apiClient from '@/shared/api/api.client';
import type { ApiResult } from '@/shared/api/api.types';

const props = defineProps<{
    productId: string;
    visible: boolean;
}>();

const emit = defineEmits(['update:visible', 'generated']);

const { showToast } = useToast();
const productStore = useProductStore();
const optionTypeStore = useOptionTypeStore();
const optionValueStore = useOptionValueStore();

const activeStep = ref(0);
const loading = ref(false);
const generating = ref(false);

// State for the wizard
const assignedOptionTypes = ref<any[]>([]);
const selectedValues = ref<Record<string, any[]>>({}); // Key: OptionTypeID, Value: List of OptionValue objects
const generatedPreview = ref<any[]>([]);

// --- STEP 1: LOAD & SELECT ---

const loadOptionData = async () => {
    loading.value = true;
    try {
        // 1. Get assigned types from product store or API
        const response = await productService.getOptionTypes(props.productId);
        if (response.success && response.data) {
            // We need full details (presentation, id)
            // The API might return just IDs or summaries. Let's hydrate from the store if needed or use what's returned.
            // Assuming API returns { id, name, presentation }
            assignedOptionTypes.value = response.data;

            // 2. Load values for each type
            for (const type of assignedOptionTypes.value) {
                // If not already in store, fetch. The store action 'fetchValues' populates 'values' ref.
                // But we need values for *multiple* types simultaneously.
                // Ideally, we fetch from API directly here to avoid store conflicts or use a tailored service method.
                const valResponse = await optionValueStore.fetchValues(type.id);
                // We'll store them locally for selection
                if (valResponse.success && valResponse.data) {
                    type.availableValues = valResponse.data;
                    // Default: Select all? Or select none. Let's select none.
                    if (!selectedValues.value[type.id]) {
                        selectedValues.value[type.id] = [];
                    }
                }
            }
        }
    } finally {
        loading.value = false;
    }
};

// --- STEP 2: PREVIEW ---

const generateCombinations = () => {
    // 1. Filter out types that have NO values selected
    const activeTypes = assignedOptionTypes.value.filter(t => {
        const values = selectedValues.value[t.id];
        return values && values.length > 0;
    });
    
    if (activeTypes.length === 0) {
        generatedPreview.value = [];
        return;
    }

    // 2. Cartesian Product
    // Start with the values of the first type
    const firstTypeValues = selectedValues.value[activeTypes[0].id];
    if (!firstTypeValues) return;

    let combinations: any[][] = firstTypeValues.map(v => [v]);

    // Iterate through the remaining types
    for (let i = 1; i < activeTypes.length; i++) {
        const typeId = activeTypes[i].id;
        const values = selectedValues.value[typeId];
        
        if (!values || values.length === 0) continue;

        const nextCombinations: any[][] = [];

        combinations.forEach(existingCombo => {
            values.forEach(val => {
                nextCombinations.push([...existingCombo, val]);
            });
        });

        combinations = nextCombinations;
    }

    // 3. Map to Preview Objects
    generatedPreview.value = combinations.map(combo => {
        // Generate a name/sku suffix
        const nameSuffix = combo.map(v => v.presentation || v.name).join(' / ');
        const skuSuffix = combo.map(v => (v.name || '').toUpperCase().substring(0, 3)).join('-');
        
        return {
            name_suffix: nameSuffix,
            sku_suffix: skuSuffix,
            price_offset: 0, // Could allow editing this in preview
            options: combo // The actual values to link
        };
    });
};

const nextStep = () => {
    if (activeStep.value === 0) {
        generateCombinations();
        activeStep.value = 1;
    }
};

const prevStep = () => {
    activeStep.value = 0;
};

// --- STEP 3: EXECUTE ---

const confirmGeneration = async () => {
    generating.value = true;
    let successCount = 0;
    let failCount = 0;

    try {
        // We will loop and create variants one by one.
        // A bulk endpoint would be better for performance, but this is safer without backend changes.
        for (const variant of generatedPreview.value) {
            const payload = {
                product_id: props.productId,
                sku: `${productStore.current_product?.sku || 'SKU'}-${variant.sku_suffix}`,
                price: productStore.current_product?.price || 0,
                option_values: variant.options.map((o: any) => o.id),
                // Metadata to track origin?
                public_metadata: { source: 'generator' }
            };

            // Strategy: Create Variant -> Add Option Values
            
            // 1. Create
            const createRes = await variantService.create(props.productId, {
                sku: payload.sku,
                price: payload.price,
                track_inventory: true
            });

            if (createRes.success && createRes.data) {
                // 2. Link Options
                const variantId = createRes.data.id;
                await variantService.updateOptionValues(variantId, payload.option_values);
                successCount++;
            } else {
                failCount++;
            }
        }

        showToast('success', 'Generation Complete', `Created ${successCount} variants.`);
        emit('generated');
        close();

    } catch (e) {
        console.error(e);
        showToast('error', 'Error', 'Failed to generate some variants.');
    } finally {
        generating.value = false;
    }
};

const close = () => {
    emit('update:visible', false);
    activeStep.value = 0;
    generatedPreview.value = [];
    selectedValues.value = {};
};

watch(() => props.visible, (val) => {
    if (val) loadOptionData();
});
</script>

<template>
    <Dialog 
        :visible="visible" 
        @update:visible="$emit('update:visible', $event)" 
        modal 
        header="Generate Variants" 
        :style="{ width: '800px' }"
        :closable="!generating"
    >
        <div class="flex flex-col gap-6">
            <!-- Stepper / Info -->
            <div class="flex items-center gap-4 text-sm text-surface-500 mb-2">
                <span :class="{'font-bold text-primary': activeStep === 0}">1. Select Options</span>
                <i class="pi pi-chevron-right text-xs"></i>
                <span :class="{'font-bold text-primary': activeStep === 1}">2. Preview & Generate</span>
            </div>

            <!-- STEP 1 -->
            <div v-if="activeStep === 0" class="flex flex-col gap-6">
                <p class="text-sm text-surface-600 m-0">
                    Select the option values you want to combine. A variant will be created for every possible combination.
                </p>

                <div v-if="loading" class="flex justify-center py-8"><ProgressSpinner /></div>
                
                <div v-else-if="assignedOptionTypes.length === 0" class="p-6 text-center bg-surface-50 rounded-xl border border-dashed">
                    <p class="font-bold">No Option Types Assigned</p>
                    <p class="text-sm mt-2">Please go to the "Options" tab and assign types (e.g. Size, Color) first.</p>
                </div>

                <div v-else class="flex flex-col gap-4">
                    <div v-for="type in assignedOptionTypes" :key="type.id" class="p-4 border rounded-xl bg-surface-50/50">
                        <div class="flex items-center justify-between mb-3">
                            <span class="font-bold">{{ type.presentation || type.name }}</span>
                            <span class="text-xs text-surface-500">{{ (selectedValues[type.id]?.length || 0) }} selected</span>
                        </div>
                        <MultiSelect 
                            v-model="selectedValues[type.id]" 
                            :options="type.availableValues" 
                            optionLabel="presentation" 
                            placeholder="Select values..." 
                            display="chip"
                            class="w-full"
                        />
                    </div>
                </div>
            </div>

            <!-- STEP 2 -->
            <div v-if="activeStep === 1" class="flex flex-col gap-4">
                <div class="flex items-center justify-between">
                    <span class="font-bold">Preview</span>
                    <Badge :value="generatedPreview.length + ' Variants'" severity="info" />
                </div>

                <div class="max-h-[400px] overflow-y-auto border rounded-xl">
                    <DataTable :value="generatedPreview" size="small" stripedRows>
                        <Column header="Name">
                            <template #body="{ data }">
                                <span class="font-medium">{{ data.name_suffix }}</span>
                            </template>
                        </Column>
                        <Column header="Generated SKU">
                            <template #body="{ data }">
                                <span class="font-mono text-xs">{{ productStore.current_product?.sku }}-{{ data.sku_suffix }}</span>
                            </template>
                        </Column>
                    </DataTable>
                </div>
            </div>
        </div>

        <template #footer>
            <div class="flex justify-between w-full">
                <Button label="Cancel" text severity="secondary" @click="close" :disabled="generating" />
                <div class="flex gap-2">
                    <Button v-if="activeStep === 1" label="Back" outlined severity="secondary" @click="prevStep" :disabled="generating" />
                    <Button v-if="activeStep === 0" label="Next" icon="pi pi-arrow-right" iconPos="right" @click="nextStep" :disabled="assignedOptionTypes.length === 0" />
                    <Button v-if="activeStep === 1" label="Generate Variants" icon="pi pi-check" severity="success" @click="confirmGeneration" :loading="generating" />
                </div>
            </div>
        </template>
    </Dialog>
</template>
