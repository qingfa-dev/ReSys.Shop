<script setup lang="ts">
import { ref, computed, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { useForm } from 'vee-validate';
import { toTypedSchema } from '@vee-validate/zod';
import * as z from 'zod';
import { productService } from '../../services/product.service';
import { optionValueService } from '@/features/catalog/option-types/option-values/services/option-value.service';
import type { OptionValueQuery } from '@/features/catalog/option-types/option-values/types/OptionValue.Query.Type';
import type { OptionValueListItem } from '@/features/catalog/option-types/option-values/types/OptionValue.Response.Type';
import type { OptionTypeDetail } from '@/features/catalog/option-types/types/OptionType.Response.Type';
import type { VariantDetail } from '../types/Variant.Response.Type';
import type { CreateVariantRequest } from '../types/Variant.Request.Type';

interface AssignedOptionType extends OptionTypeDetail {
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

const schema = z.object({
    sku: z.string().min(1, 'SKU is required'),
    price: z.number().min(0, 'Price must be non-negative'),
    trackInventory: z.boolean(),
    barcode: z.string().optional().nullable(),
    weight: z.number().optional().nullable(),
});

const { defineField, handleSubmit, errors, resetForm, setValues } = useForm({
    validationSchema: toTypedSchema(schema),
    initialValues: {
        sku: '',
        price: 0,
        trackInventory: true,
        barcode: '',
        weight: null as number | null,
    }
});

const [sku] = defineField('sku');
const [price] = defineField('price');
const [trackInventory] = defineField('trackInventory');
const [barcode] = defineField('barcode');
const [weight] = defineField('weight');

const fetchOptions = async () => {
    loadingOptions.value = true;
    try {
        const res = await productService.getOptionTypes(props.productId);
        if (res.isSuccess && res.value) {
            const types = res.value;
            for (const type of types) {
                const valRes = await optionValueService.list({ optionTypeId: type.id });
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

watch([() => props.variant, assignedOptionTypes], ([newVariant, types]) => {
    if (newVariant) {
        setValues({
            sku: newVariant.sku || '',
            price: newVariant.price,
            trackInventory: newVariant.trackInventory,
            barcode: newVariant.barcode || '',
            weight: newVariant.weight,
        });

        const mapped: Record<string, string> = {};
        if (newVariant.optionValueIds && types.length > 0) {
            newVariant.optionValueIds.forEach((id: string) => {
                types.forEach(type => {
                    if (type.values && type.values.some((v: OptionValueListItem) => v.id === id)) {
                        mapped[type.id] = id;
                    }
                });
            });
        }
        selectedOptionValues.value = mapped;
    } else {
        resetForm();
        selectedOptionValues.value = {};
    }
}, { immediate: true, deep: true });

const onSubmit = handleSubmit((values) => {
    const optionValueIds = Object.values(selectedOptionValues.value).filter(v => !!v);

    const payload: CreateVariantRequest = {
        sku: values.sku,
        price: values.price,
        position: 0,
        trackInventory: values.trackInventory,
        barcode: values.barcode ?? undefined,
        weight: values.weight,
        optionValueIds: optionValueIds,
        productId: props.productId,
    };
    emit('save', payload);
});
</script>

<template>
    <Dialog v-model:visible="visible" modal :header="isEdit ? t('catalog.products.variants.form.edit_variant') : t('catalog.products.variants.form.new_variant')" class="w-full max-w-lg">
        <div class="flex flex-col gap-6 pt-4">
            <div class="flex flex-col gap-2">
                <label class="font-bold text-xs uppercase tracking-wider text-surface-500 ml-1">SKU</label>
                <InputText v-model="sku" class="w-full" :invalid="!!errors.sku" />
                <small class="p-error" v-if="errors.sku">{{ errors.sku }}</small>
            </div>

            <div v-if="!variant?.isMaster && assignedOptionTypes.length > 0" class="flex flex-col gap-4 p-4 bg-surface-50 dark:bg-surface-800/50 rounded-2xl border border-surface-200 dark:border-surface-700">
                <span class="font-bold text-xs uppercase tracking-wider text-surface-500">{{ t('catalog.products.variants.form.attributes') }}</span>
                <div v-for="type in assignedOptionTypes" :key="type.id" class="flex flex-col gap-1">
                    <label class="text-xs font-medium">{{ type.presentation || type.name }}</label>
                    <Select v-model="selectedOptionValues[type.id]" :options="type.values" optionLabel="presentation" optionValue="id" placeholder="Select..." class="w-full" />
                </div>
            </div>

            <div class="grid grid-cols-2 gap-4">
                <div class="flex flex-col gap-2">
                    <label class="font-bold text-xs uppercase tracking-wider text-surface-500 ml-1">{{ t('catalog.products.labels.price') }}</label>
                    <InputNumber v-model="price" mode="currency" currency="USD" locale="en-US" class="w-full" :invalid="!!errors.price" />
                    <small class="p-error" v-if="errors.price">{{ errors.price }}</small>
                </div>
                <div class="flex flex-col gap-2">
                    <label class="font-bold text-xs uppercase tracking-wider text-surface-500 ml-1">{{ t('catalog.products.labels.weight') }}</label>
                    <InputNumber v-model="weight" mode="decimal" :minFractionDigits="2" class="w-full" />
                </div>
            </div>

            <div class="flex flex-col gap-2">
                <label class="font-bold text-xs uppercase tracking-wider text-surface-500 ml-1">{{ t('catalog.products.variants.form.barcode') }}</label>
                <InputText v-model="barcode" class="w-full" />
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
    </Dialog>
</template>
