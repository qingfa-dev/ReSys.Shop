<script setup lang="ts">
import { ref, computed, watch } from 'vue';
import { useForm } from 'vee-validate';
import { toTypedSchema } from '@vee-validate/zod';
import * as z from 'zod';
import { productService } from '../services/product.service';
import { optionValueService } from '@/features/catalog/option-types/option-values/services/option-value.service';
import type { VariantDetail } from '../types/variant.types';
import { productLocales, type ProductLocales } from '../locales/product.locales';

const t = productLocales as ProductLocales;

const props = defineProps<{
    modelValue: boolean;
    variant?: VariantDetail | null;
    productId: string;
}>();

const emit = defineEmits<{
    (e: 'update:modelValue', value: boolean): void;
    (e: 'save', data: any): void;
}>();

const isEdit = computed(() => !!props.variant);
const visible = computed({
    get: () => props.modelValue,
    set: (val) => emit('update:modelValue', val)
});

const assignedOptionTypes = ref<any[]>([]);
const selectedOptionValues = ref<Record<string, string>>({}); // TypeID -> ValueID
const loadingOptions = ref(false);

const schema = z.object({
    sku: z.string().min(1, 'SKU is required'),
    price: z.number().min(0, 'Price must be non-negative'),
    track_inventory: z.boolean(),
    barcode: z.string().optional().nullable(),
    weight: z.number().optional().nullable(),
});

const { defineField, handleSubmit, errors, resetForm, setValues } = useForm({
    validationSchema: toTypedSchema(schema),
    initialValues: {
        sku: '',
        price: 0,
        track_inventory: true,
        barcode: '',
        weight: null as number | null,
    }
});

const [sku] = defineField('sku');
const [price] = defineField('price');
const [track_inventory] = defineField('track_inventory');
const [barcode] = defineField('barcode');
const [weight] = defineField('weight');

const fetchOptions = async () => {
    loadingOptions.value = true;
    try {
        // 1. Get assigned types for this product
        const res = await productService.getOptionTypes(props.productId);
        if (res.success && res.data) {
            const types = res.data;
            // 2. Fetch values for each type using the correct service
            for (const type of types) {
                const valRes = await optionValueService.list({ option_type_id: type.id });
                if (valRes.success && valRes.data) {
                    type.values = valRes.data;
                }
            }
            assignedOptionTypes.value = types;
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
            track_inventory: newVariant.track_inventory,
            barcode: newVariant.barcode || '',
            weight: newVariant.weight,
        });
        
        // Map existing options to selection
        const mapped: Record<string, string> = {};
        if (newVariant.option_value_ids && types.length > 0) {
            newVariant.option_value_ids.forEach((id: string) => {
                types.forEach(type => {
                    if (type.values && type.values.some((v: any) => v.id === id)) {
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
    
    emit('save', {
        ...values,
        option_value_ids: optionValueIds,
        product_id: props.productId
    });
});
</script>

<template>
    <Dialog v-model:visible="visible" modal :header="isEdit ? t.variants?.form?.edit_variant : t.variants?.form?.new_variant" class="w-full max-w-lg">
        <div class="flex flex-col gap-6 pt-4">
            <div class="flex flex-col gap-2">
                <label class="font-bold text-xs uppercase tracking-wider text-surface-500 ml-1">SKU</label>
                <InputText v-model="sku" class="w-full" :invalid="!!errors.sku" />
                <small class="text-red-500" v-if="errors.sku">{{ errors.sku }}</small>
            </div>

            <!-- Option Values Selection -->
            <div v-if="!variant?.is_master && assignedOptionTypes.length > 0" class="flex flex-col gap-4 p-4 bg-surface-50 dark:bg-surface-800/50 rounded-2xl border border-surface-200 dark:border-surface-700">
                <span class="font-bold text-xs uppercase tracking-wider text-surface-500">{{ t.variants?.form?.attributes }}</span>
                <div v-for="type in assignedOptionTypes" :key="type.id" class="flex flex-col gap-1">
                    <label class="text-xs font-medium">{{ type.presentation || type.name }}</label>
                    <Select v-model="selectedOptionValues[type.id]" :options="type.values" optionLabel="presentation" optionValue="id" placeholder="Select..." class="w-full" />
                </div>
            </div>

            <div class="grid grid-cols-2 gap-4">
                <div class="flex flex-col gap-2">
                    <label class="font-bold text-xs uppercase tracking-wider text-surface-500 ml-1">{{ t.labels?.price }}</label>
                    <InputNumber v-model="price" mode="currency" currency="USD" locale="en-US" class="w-full" :invalid="!!errors.price" />
                    <small class="text-red-500" v-if="errors.price">{{ errors.price }}</small>
                </div>
                <div class="flex flex-col gap-2">
                    <label class="font-bold text-xs uppercase tracking-wider text-surface-500 ml-1">{{ t.labels?.weight }}</label>
                    <InputNumber v-model="weight" mode="decimal" :minFractionDigits="2" class="w-full" />
                </div>
            </div>

            <div class="flex flex-col gap-2">
                <label class="font-bold text-xs uppercase tracking-wider text-surface-500 ml-1">{{ t.variants?.form?.barcode }}</label>
                <InputText v-model="barcode" class="w-full" />
            </div>

            <div class="flex items-center justify-between p-4 bg-surface-50 dark:bg-surface-800 rounded-xl border border-surface-100 dark:border-surface-700">
                <div class="flex flex-col">
                    <span class="font-bold text-sm">{{ t.variants?.form?.track_inventory }}</span>
                    <span class="text-xs text-surface-500">{{ t.variants?.form?.track_inventory_desc }}</span>
                </div>
                <ToggleSwitch v-model="track_inventory" />
            </div>
        </div>

        <template #footer>
            <Button :label="t.actions?.cancel" text severity="secondary" @click="visible = false" />
            <Button :label="isEdit ? t.actions?.save : t.actions?.new" icon="pi pi-check" @click="onSubmit" />
        </template>
    </Dialog>
</template>
