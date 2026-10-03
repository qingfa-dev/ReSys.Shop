<script setup lang="ts">
import { ref, computed, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { useForm } from 'vee-validate';
import { toTypedSchema } from '@vee-validate/zod';
import { createVariantSchema } from '../types/variant.field';
import { productRepository } from '../products/api/product.api';
import { optionValueRepository } from '@/features/catalog/option-values';
import ModalDialog from '@/shared/components/overlays/ModalDialog.vue';
import FormField from '@/shared/components/form/FormField.vue';
import type { OptionValueListItem } from '@/features/catalog/option-values';
import type { VariantDetail } from '../models/variant.response';
import type { CreateVariantRequest } from '../models/variant.request';
import type { ProductOptionTypeItem } from '../../product-option-types/models/product-option-type.response';

interface AssignedOptionType extends ProductOptionTypeItem {
  values: OptionValueListItem[];
}

const { t } = useI18n();

const props = defineProps<{
    modelValue: boolean;
    variant?: VariantDetail | null;
    productId: string;
}>();

const emit = defineEmits<{
    (e: 'update:modelValue', value: boolean): void;
    (e: 'save', data: CreateVariantRequest): void;
}>();

const isEdit = computed(() => !!props.variant);
const visible = computed({
    get: () => props.modelValue,
    set: (val) => emit('update:modelValue', val)
});

const assignedOptionTypes = ref<AssignedOptionType[]>([]);
const selectedOptionValues = ref<Record<string, string>>({});
const loadingOptions = ref(false);

const schema = createVariantSchema(t);

const { defineField, handleSubmit, errors, resetForm, setValues } = useForm({
    validationSchema: toTypedSchema(schema),
    initialValues: {
        sku: '',
        price: 0,
        trackInventory: true,
    }
});

const [sku] = defineField('sku');
const [price] = defineField('price');
const [trackInventory] = defineField('trackInventory');

const fetchOptions = async () => {
    loadingOptions.value = true;
    try {
        const res = await productRepository.getOptionTypes(props.productId);
        if (res.isSuccess && res.value) {
            const types = res.value;
            for (const type of types) {
                const valRes = await optionValueRepository.list({ optionTypeId: type.id });
                if (valRes.isSuccess && valRes.items) {
                    (type as unknown as Record<string, unknown>).values = valRes.items;
                }
            }
            assignedOptionTypes.value = types as AssignedOptionType[];
        }
    } finally {
        loadingOptions.value = false;
    }
};

watch(() => props.modelValue, (newVal) => {
    if (newVal) {
        fetchOptions();
    }
});

watch([() => props.variant, assignedOptionTypes], ([newVariant]) => {
    if (newVariant) {
        setValues({
            sku: newVariant.sku || '',
            price: newVariant.price,
            trackInventory: newVariant.trackInventory,
        });
    } else {
        resetForm();
        selectedOptionValues.value = {};
    }
}, { immediate: true, deep: true });

const onSubmit = handleSubmit((values) => {
    const payload: CreateVariantRequest = {
        sku: values.sku,
        price: values.price,
        position: 0,
        trackInventory: values.trackInventory,
        productId: props.productId,
    };
    emit('save', payload);
});
</script>

<template>
    <ModalDialog v-model="visible" :header="isEdit ? t('catalog.products.variants.form.edit_variant') : t('catalog.products.variants.form.new_variant')">
        <div class="flex flex-col gap-6 pt-4">
            <FormField label="SKU" name="sku" :error="errors.sku">
                <InputText v-model="sku" class="w-full" :invalid="!!errors.sku" />
            </FormField>

            <div v-if="!variant?.isMaster && assignedOptionTypes.length > 0" class="flex flex-col gap-4 p-4 bg-surface-50 dark:bg-surface-800/50 rounded-2xl border border-surface-200 dark:border-surface-700">
                <span class="font-bold text-xs uppercase tracking-wider text-surface-500">{{ t('catalog.products.variants.form.attributes') }}</span>
                <div v-for="type in assignedOptionTypes" :key="type.id" class="flex flex-col gap-1">
                    <label class="text-xs font-medium">{{ type.presentation || type.name }}</label>
                    <Select v-model="selectedOptionValues[type.id]"
                      :options="type.values"
                      optionLabel="presentation"
                      optionValue="id"
                      placeholder="Select..."
                      class="w-full" />
                </div>
            </div>

            <div class="grid grid-cols-2 gap-4">
                <FormField :label="t('catalog.products.labels.price')" name="price" :error="errors.price">
                    <InputNumber v-model="price" mode="currency" currency="USD" locale="en-US" class="w-full" :invalid="!!errors.price" />
                </FormField>
            </div>

            <div class="flex items-center justify-between p-4 bg-surface-50 dark:bg-surface-800 rounded-xl border border-surface-100 dark:border-surface-700">
                <div class="flex flex-col">
                    <span class="font-bold text-sm">{{ t('catalog.products.variants.form.track_inventory') }}</span>
                    <span class="text-xs text-surface-500">{{ t('catalog.products.variants.form.track_inventory_desc') }}</span>
                </div>
                <ToggleSwitch v-model="trackInventory" />
            </div>
        </div>

        <template #footer>
            <Button :label="t('catalog.products.actions.cancel')" text severity="secondary" @click="visible = false" />
            <Button :label="isEdit ? t('catalog.products.actions.save') : t('catalog.products.actions.new')" icon="pi pi-check" @click="onSubmit" />
        </template>
    </ModalDialog>
</template>
