<script setup lang="ts">
import { ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { useOptionTypeStore } from '@/features/catalog/option-types/stores/option-type.store';
import { useOptionValueStore } from '@/features/catalog/option-types/option-values/stores/option-value.store';
import { productService } from '../../services/product.service';
import { variantService } from '../services/variant.service';
import { useToast } from '@/shared/composables/toast.use';
import type { OptionTypeDetail } from '@/features/catalog/option-types/types/option-type.response.type';
import type { OptionValueListItem } from '@/features/catalog/option-types/option-values/types/option-value.response.type';

interface AssignedOptionType extends OptionTypeDetail {
  availableValues?: OptionValueListItem[]
}

interface PreviewVariant {
  nameSuffix: string; skuSuffix: string; priceOffset: number; options: { id: string; name: string; presentation: string }[]
}

const { t } = useI18n();

const props = defineProps<{
    productId: string;
    visible: boolean;
}>();

const emit = defineEmits(['update:visible', 'generated']);

const { showToast } = useToast();
const optionValueStore = useOptionValueStore();

const activeStep = ref(0);
const loading = ref(false);
const generating = ref(false);

const assignedOptionTypes = ref<AssignedOptionType[]>([]);
const selectedValues = ref<Record<string, OptionValueListItem[]>>({});
const generatedPreview = ref<PreviewVariant[]>([]);

const defaultSku = 'SKU';
const defaultPrice = 0;

const loadOptionData = async () => {
    loading.value = true;
    try {
        const response = await productService.getOptionTypes(props.productId);
        if (response.isSuccess && response.value) {
            assignedOptionTypes.value = response.value;

            for (const type of assignedOptionTypes.value) {
                const valResponse = await optionValueStore.fetchValues(type.id);
                if (valResponse.isSuccess && valResponse.items) {
                    type.availableValues = valResponse.items;
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

const generateCombinations = () => {
    const activeTypes = assignedOptionTypes.value.filter((t: AssignedOptionType) => {
        const values = selectedValues.value[t.id];
        return values && values.length > 0;
    });

    if (activeTypes.length === 0) {
        generatedPreview.value = [];
        return;
    }

    const firstType = activeTypes[0];
    if (!firstType) return;
    const firstTypeValues = selectedValues.value[firstType.id];
    if (!firstTypeValues) return;

    let combinations: OptionValueListItem[][] = firstTypeValues.map((v: OptionValueListItem) => [v]);

    for (let i = 1; i < activeTypes.length; i++) {
        const activeType = activeTypes[i];
        if (!activeType) continue;
        const typeId = activeType.id;
        const values = selectedValues.value[typeId];

        if (!values || values.length === 0) continue;

        const nextCombinations: OptionValueListItem[][] = [];

        combinations.forEach(existingCombo => {
            values.forEach((val: OptionValueListItem) => {
                nextCombinations.push([...existingCombo, val]);
            });
        });

        combinations = nextCombinations;
    }

    generatedPreview.value = combinations.map((combo: OptionValueListItem[]) => {
        const nameSuffix = combo.map(v => v.presentation || v.name).join(' / ');
        const skuSuffix = combo.map(v => (v.name || '').toUpperCase().substring(0, 3)).join('-');

        return {
            nameSuffix: nameSuffix,
            skuSuffix: skuSuffix,
            priceOffset: 0,
            options: combo
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

const confirmGeneration = async () => {
    generating.value = true;
    let successCount = 0;
    let failCount = 0;

    try {
        for (const variant of generatedPreview.value) {
            const payload = {
                productId: props.productId,
                sku: `${defaultSku}-${variant.skuSuffix}`,
                price: defaultPrice,
                optionValues: variant.options.map((o: { id: string }) => o.id),
            };

            const createRes = await variantService.create(props.productId, {
                sku: payload.sku,
                price: payload.price,
                position: 0,
                trackInventory: true
            });

            if (createRes.isSuccess && createRes.value) {
                const variantId = createRes.value.id;
                await variantService.updateOptionValues(variantId, payload.optionValues);
                successCount++;
            } else {
                failCount++;
            }
        }

        showToast('success', t('common.success'), t('catalog.products.variants.wizard.generated', { count: successCount }));
        emit('generated');
        close();

    } catch (e) {
        console.error(e);
        showToast('error', t('common.error'), t('catalog.products.variants.messages.generation_failed'));
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
            <div class="flex items-center gap-4 text-sm text-surface-500 mb-2">
                <span :class="{'font-bold text-primary': activeStep === 0}">1. Select Options</span>
                <i class="pi pi-chevron-right text-xs"></i>
                <span :class="{'font-bold text-primary': activeStep === 1}">2. Preview & Generate</span>
            </div>

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

            <div v-if="activeStep === 1" class="flex flex-col gap-4">
                <div class="flex items-center justify-between">
                    <span class="font-bold">Preview</span>
                    <Badge :value="generatedPreview.length + ' Variants'" severity="info" />
                </div>

                <div class="max-h-[400px] overflow-y-auto border rounded-xl">
                    <DataTable :value="generatedPreview" size="small" stripedRows>
                        <Column header="Name">
                            <template #body="{ data }">
                                <span class="font-medium">{{ data.nameSuffix }}</span>
                            </template>
                        </Column>
                        <Column header="Generated SKU">
                            <template #body="{ data }">
                                <span class="font-mono text-xs">{{ defaultSku }}-{{ data.skuSuffix }}</span>
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
